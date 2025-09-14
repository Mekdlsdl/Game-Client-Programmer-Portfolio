using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

using UnityEngine;

using BackEnd; // 뒤끝 SDK

using LitJson;
using Newtonsoft.Json;


public enum DataTableName
{
    //=============== 단일 행 ===============
    user_base,          // 데이터 버전, 닉네임, 진행 중인 스테이지, 돈, 다이아
    user_stats,         // 플레이어 스탯 관련 정보 (HpLevel ~ Critical)
    user_recipe,        // 해금 레시피, 메뉴판, (보상금 수령 여부)
    user_energy,        // 하트 관련 정보
    user_tutorial,      // 튜토리얼 안내 메시지 관련 정보
    single_row,         // 단일 행 테이블 개수 받을 때 사용

    //=============== 다중 행 ===============
    user_stageclear,    // 스테이지 클리어 관련 정보 (별, 최고기록 등)
    multi_row           // 다중 행 테이블 개수 받을 때 사용 (multi_row - single_row - 1)
}

public static class BackendRepo
{
    // 다중 행 테이블 기본 값 (stageclear 하나 뿐이라,, 나중에 이름 수정될 것 예방)
    // 이후에 추가된 다중 행 테이블은 호출 시 파라미터로 작성
    private const string _defaultKeyName = "user_stageclear";


    // ========= 설정값 =========
    static readonly int MaxRetries = 3;             // 즉시 재시도 횟수
    static readonly int BaseDelayMs = 300;          // 백오프 시작 딜레이
    static readonly int MaxQueueSize = 500;         // 오프라인 큐 최대 적재 개수
    static readonly string QueueFilePath = Path.Combine(Application.persistentDataPath, "backend_offline_queue.jsonl");
    static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(8); // 백그라운드 전송 간격


    // ========= 오프라인 큐 =========
    class QueueItem
    {
        public string Op = "Upsert"; // 확장 여지 (Insert/Update 등)
        public string Table;
        public Dictionary<string, object> Row;
        public string OpId = Guid.NewGuid().ToString(); // (선택) 중복 방지용
        public long CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // 생성 시간
    }

    // ======== 꼬임 방지 Lock ========
    static readonly object _queueLock = new object();
    static readonly Queue<QueueItem> _queue = new Queue<QueueItem>();
    static CancellationTokenSource _flushCts;
    static bool _loaded;


    // ======== 퍼블릭 JSON 유틸 ========
    public static string ToJson<T>(T obj) => JsonConvert.SerializeObject(obj);
    public static T FromJson<T>(string json) =>
        string.IsNullOrEmpty(json) ? default : JsonConvert.DeserializeObject<T>(json);


    // ======== 퍼블릭: 앱 시작/종료 시 호출 ========
    public static void InitializeQueueAndFlusher()  // 백그라운드 작업 시작
    {
        if (!_loaded) { LoadQueueFromDisk(); _loaded = true; }
        StartBackgroundFlusher();
    }
    public static void ShutdownFlusher()    // 종료, 로컬에 큐 상태 저장
    {
        _flushCts?.Cancel();
        _flushCts = null;
        SaveQueueToDisk();
    }


    // ======== High-level APIs ========

    /// 데이터 1행만 (다중 행 데이터도 필터링해서 하나만)
    public static async Task<(bool ok, Dictionary<string, object> row)> GetOneAsync(
        string table,
        string keyName = _defaultKeyName, int keyValue = -1)
    {
        var bro = await RetryAsync(() => GameDataGetMyDataAsync(table));
        if (!IsSuccess(bro)) return (false, null);

        var rows = GetRows(bro);
        if (rows == null || rows.Count == 0) return (false, null);
        
        var match = rows.FirstOrDefault(r =>
            r.TryGetValue(keyName, out var v) &&
            v != null &&
            v.ToString() == keyValue.ToString());

        if (match == null) return (false, null);
        return (true, match);
    }

    public static async Task<(bool ok, List<Dictionary<string, object>> rows)> GetManyAsync(string table)
    {
        var bro = await RetryAsync(() => GameDataGetMyDataAsync(table));
        if (!IsSuccess(bro)) return (false, null);

        var rows = GetRows(bro);
        if (rows == null || rows.Count == 0) return (false, null);

        return (true, rows);
    }


    /// Upsert: 실패 시 -> 오프라인 큐에 적재 후 false 반환(호출부는 fire&forget이면 무시해도 됨)
    // 단일 행 Upsert
    public static async Task<bool> UpsertAsync(string table, Dictionary<string, object> row)
    {
        // 즉시 재시도
        var ok = await TryUpsertOnceWithRetry(table, row);
        if (ok) return true;

        // 여전히 실패 -> 네트워크/서버 일시 이슈로 간주하고 큐 적재
        Enqueue(table, row);
        SaveQueueToDisk();
        Debug.LogWarning($"[BackendRepo] Upsert enqueued. table={table}, queued={_queue.Count}");
        return false;
    }

    // 다중 행 Upsert
    public static async Task<bool> UpsertByKeyAsync(
        string table, string keyName, int keyValue, Dictionary<string, object> row)
    {
        try
        {
            // 1) where 조건으로 내 데이터 중 해당 키값 검색
            var where = new Where();
            where.Equal(keyName, keyValue);

            var get = await GameDataGetMyDataAsync(table, where);
            if (IsSuccess(get))
            {
                var rows = GetRows(get);
                if (rows != null && rows.Count > 0)
                {
                    // 이미 존재 → Update
                    var inDate = rows[0]["inDate"]?.ToString();
                    if (!string.IsNullOrEmpty(inDate))
                    {
                        var updateBro = await GameDataUpdateAsync(table, ToParam(row), keyValue);
                        return IsSuccess(updateBro);
                    }
                }
            }

            // 2) 없으면 Insert (키 포함 보장)
            var p = ToParam(row);
            if (!row.ContainsKey(keyName))
                p.Add(keyName, keyValue);

            var insertBro = await GameDataInsertAsync(table, p);
            return IsSuccess(insertBro);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[BackendRepo] UpsertByKeyAsync fail: {e}");
            return false;
        }
    }

    // 작은 편의 메서드
    static async Task<bool> ThenIsSuccess(this Task<BackendReturnObject> t)
    {
        var bro = await t;
        return IsSuccess(bro);
    }



    // ======== 내부: 즉시 재시도 구현 ========
    static async Task<bool> TryUpsertOnceWithRetry(string table, Dictionary<string, object> row)
    {
        BackendReturnObject last = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                // 존재 여부 확인
                var get = await RetryAsync(() => GameDataGetMyDataAsync(table));
                if (!IsSuccess(get))
                {
                    if (!IsTransient(get)) return false; // 영구 오류면 즉시 실패
                }

                var param = ToParam(row);

                if (!IsSuccess(get) || IsEmpty(get))
                {
                    // INSERT
                    last = await RetryAsync(() => GameDataInsertAsync(table, param));
                    if (IsSuccess(last)) return true;
                }
                else
                {
                    // UPDATE
                    var rows = GetRows(get);
                    var inDate = rows?[0]?["inDate"]?.ToString();
                    if (string.IsNullOrEmpty(inDate)) return false;

                    last = await RetryAsync(() => GameDataUpdateAsync(table, param));
                    if (IsSuccess(last)) return true;
                }

                if (!IsTransient(last)) return false; // 영구 오류
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BackendRepo] Upsert attempt error: {e.Message}");
            }

            if (attempt < MaxRetries)
                await Task.Delay(BackoffDelay(attempt));
        }

        return false;
    }

    private static async Task<T> RetryAsync<T>(
        Func<Task<T>> action,
        int maxRetries = 2,
        int baseDelayMs = 300)
    {
        int attempt = 0;
        Exception lastEx = null;

        while (attempt <= maxRetries)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                lastEx = ex;
                Debug.LogWarning($"[BackendRepo] RetryAsync attempt {attempt + 1} failed: {ex.Message}");
            }

            attempt++;
            if (attempt <= maxRetries)
            {
                // 지수 백오프 + 랜덤 지터
                int delay = Math.Min(baseDelayMs * (int)Math.Pow(2, attempt), 4000); // 4000 = 최대 4초
                delay += UnityEngine.Random.Range(0, 120);
                await Task.Delay(delay);
            }
        }

        throw lastEx ?? new Exception("RetryAsync failed without exception");
    }


    // 서버 오류 대비 임시 저장 용도
    // ======== 오프라인 큐: 적재/저장/불러오기/전송 ========
    static void Enqueue(string table, Dictionary<string, object> row)
    {
        lock (_queueLock)
        {
            if (_queue.Count >= MaxQueueSize) // 오래된 것 드랍
                _queue.Dequeue();

            _queue.Enqueue(new QueueItem { Table = table, Row = row });
        }
    }

    static void SaveQueueToDisk()
    {
        try
        {
            lock (_queueLock)
            {
                using var w = new StreamWriter(QueueFilePath, false);
                foreach (var item in _queue)
                    w.WriteLine(JsonConvert.SerializeObject(item));
            }
        }
        catch (Exception e) { Debug.LogWarning($"[BackendRepo] SaveQueueToDisk fail: {e.Message}"); }
    }

    static void LoadQueueFromDisk()
    {
        try
        {
            if (!File.Exists(QueueFilePath)) return;
            int count = 0;
            foreach (var line in File.ReadLines(QueueFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var item = JsonConvert.DeserializeObject<QueueItem>(line);
                if (item != null) { _queue.Enqueue(item); count++; }
            }
            Debug.Log($"[BackendRepo] Offline queue loaded: {count}");
        }
        catch (Exception e) { Debug.LogWarning($"[BackendRepo] LoadQueueFromDisk fail: {e.Message}"); }
    }

    public static void StartBackgroundFlusher()
    {
        if (_flushCts != null) return;
        _flushCts = new CancellationTokenSource();
        _ = FlushLoopAsync(_flushCts.Token);
    }

    static async Task FlushLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await FlushOnceAsync(ct); }
            catch (Exception e) { Debug.LogWarning($"[BackendRepo] Flush loop error: {e.Message}"); }
            await Task.Delay(FlushInterval, ct);
        }
    }

    // 큐에서 하나씩 꺼내서 서버로 옮기기
    // 성공하면 큐에서 없애고 큐 상태 저장
    // 실패하면 다음 주기에 다시 시도
    public static async Task FlushOnceAsync(CancellationToken ct = default)
    {
        while (true)
        {
            QueueItem item = null;
            lock (_queueLock)
            {
                if (_queue.Count == 0) break;
                item = _queue.Peek(); // 성공 시 Dequeue
            }

            bool ok = false;
            try
            {
                if (item.Op == "Upsert")
                    ok = await TryUpsertOnceWithRetry(item.Table, item.Row);
                else
                    ok = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BackendRepo] Flush item error: {e.Message}");
                ok = false;
            }

            if (ok)
            {
                lock (_queueLock) { _queue.Dequeue(); }
                SaveQueueToDisk(); // 안전하게 즉시 저장
            }
            else
            {
                // 아직 실패 -> 다음 주기까지 보류
                break;
            }

            if (ct.IsCancellationRequested) break;
        }
    }

    // ======== SDK 호출 래퍼 ========

    static Task<BackendReturnObject> GameDataGetMyDataAsync(string table, Where where = null)
    {
        if (where == null) where = new Where();

        var tcs = new TaskCompletionSource<BackendReturnObject>();
        Backend.GameData.GetMyData(table, where, bro => tcs.TrySetResult(bro));
        return tcs.Task;
    }
    static Task<BackendReturnObject> GameDataInsertAsync(string table, Param param)
    {
        var tcs = new TaskCompletionSource<BackendReturnObject>();
        Backend.GameData.Insert(table, param, bro => tcs.TrySetResult(bro));
        return tcs.Task;
    }
    static Task<BackendReturnObject> GameDataUpdateAsync(
        string table, Param param,
        int keyValue = -1, // 서브 키 개념이라 웬만하면 데테에서도 int로 두기
        string keyName = _defaultKeyName)
    {
        var tcs = new TaskCompletionSource<BackendReturnObject>();
        var where = new Where();
        
        // 다중 행
        if (keyValue != -1) where.Equal(keyName, keyValue);
            
        Backend.GameData.Update(table, where, param, bro => tcs.TrySetResult(bro));
        return tcs.Task;
    }


    // ======== 공통 헬퍼 ========
    static bool IsSuccess(BackendReturnObject bro)
    {
        if (bro == null) return false;
        if (bro.IsSuccess()) return true;
        Debug.LogWarning($"[BackendRepo] API fail: {bro}");
        return false;
    }

    static bool IsEmpty(BackendReturnObject bro)
    {
        var rows = GetRows(bro);
        return rows == null || rows.Count == 0;
    }


    /// backend object -> list data
    static List<Dictionary<string, object>> GetRows(BackendReturnObject bro)
    {
        try
        {
            JsonData json = bro.GetReturnValuetoJSON();
            if (json == null || !json.ContainsKey("rows")) return null;
            var rowsJson = json["rows"];
            var list = new List<Dictionary<string, object>>();
            for (int i = 0; i < rowsJson.Count; i++)
            {
                var rowJson = rowsJson[i];
                var dict = new Dictionary<string, object>();
                foreach (var key in rowJson.Keys)
                    dict[key] = ToPlainValue(rowJson[key]);
                list.Add(dict);
            }
            return list;
        }
        catch (Exception e)
        {
            Debug.LogError($"[BackendRepo] Parse rows failed: {e}");
            return null;
        }
    }

    /// json data -> value
    static object ToPlainValue(JsonData v)
    {
        if (v == null) return null;
        if (v.IsString) return (string)v;
        if (v.IsInt) return (int)v;
        if (v.IsLong) return (long)v;
        if (v.IsBoolean) return (bool)v;
        if (v.IsDouble) return (double)v;
        if (double.TryParse(v.ToString(), out var d)) return d;
        return v.ToJson(); // object/array → JSON string
    }

    /// list data -> param
    static Param ToParam(Dictionary<string, object> row)
    {
        var p = new Param();
        if (row == null) return p;
        foreach (var kv in row)
        {
            if (kv.Key == "inDate" || kv.Key == "owner_inDate") continue;

            var val = kv.Value;
            switch (val)
            {
                case null: break;
                case string s: p.Add(kv.Key, s); break;
                case int i32: p.Add(kv.Key, i32); break;
                case long i64: p.Add(kv.Key, Convert.ToDouble(i64)); break; // epoch sec
                case float f32: p.Add(kv.Key, Convert.ToDouble(f32)); break;
                case double f64: p.Add(kv.Key, f64); break;
                case bool b: p.Add(kv.Key, b); break;
                case DateTime dt: p.Add(kv.Key, dt); break; // datetime 컬럼
                default: p.Add(kv.Key, JsonConvert.SerializeObject(val)); break; // JSON string
            }
        }
        return p;
    }

    static int BackoffDelay(int attempt)
    {
        // 지수 백오프 + 소량 지터
        var rand = UnityEngine.Random.Range(0, 120);

        return Math.Min(BaseDelayMs * (int)Math.Pow(2, attempt) + rand, 4000); // 4000 = 최대 4초
    }

    static bool IsTransient(BackendReturnObject bro)
    {
        // 네트워크/타임아웃/서버오류(5xx) 등.. 재시도 가치가 있는가..?
        // TODO : 프로젝트 기준으로 뒤끝 에러 코드를 매핑해 사용 (일단 아주 나중에,,)
        // 여기선 보수적으로 대부분 재시도 대상으로 간주.

        if (bro == null) return true;

        string msg = bro.GetMessage();

        if (string.IsNullOrEmpty(msg)) return true;

        msg = msg.ToLowerInvariant();

        return msg.Contains("timeout") || msg.Contains("network") || msg.Contains("temporarily");
    }
}
