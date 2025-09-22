/*
 * UserData에 항목 추가 시 MakeDefaultData 함수 업데이트 필수!!
 */

using BackEnd;
using BackEnd.BackndLitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public class UserData // 임시 초기값은 여기서 지정
{
    // =============== user_base ===============
    
    /// 데이터 버전 관리용
    public int Version = 0;

    /// 아마도 닉네임
    public string UserName;

    /// 최고 스테이지 (도전 예정, 아직 클리어한 건 아님)
    public int StageLevel = 5001;

    /// 현재 진행 중인 스테이지 (가장 최근 진행한 스테이지)
    public int CurrentStageKey = 5001;

    /// 보유 코인
    public int Coin = 0;
    
    /// 다이아
    public int Dia = 0;



    // =============== user_stats ===============
    
    /// 체력 강화 레벨
    public int HpLevel = 0;

    /// 적용된 체력 값
    public float MaxHp = 150;

    /// 공격력 레벨
    public int AtkLevel = 0;

    /// 적용된 공격력
    public float Atk = 20;

    /// 이동 속도 레벨
    public int MoveSpeedLevel = 0;

    /// 적용된 이동속도
    public float MoveSpeed = 5;

    /// 공격 속도 레벨
    public int AtkSpeedLevel = 0;

    /// 적용된 공격 속도
    public float AtkSpeed = 1;

    /// 인벤토리 레벨
    public int BagSizeLevel = 0;

    /// 적용된 인벤토리 칸 수
    public int BagSize = 50;

    /// 크리티컬 레벨
    public int CriticalLevel = 0;

    /// 적용된 크리티컬 확률
    public float Critical = 0.1f;



    // =============== user_recipe ===============
    
    /// 해금한 레시피들 key값만 들고있기
    public List<int> UnlockRecipes = new List<int> {  };
    
    /// 메뉴판에서 선택한 레시피들 key값
    public List<int> SelectRecipes = new List<int> {  };
    
    /// 환급 받은 적 있는지 여부 (0 = 구매 레시피 / 1 = 메뉴판)
    public List<bool> HadReceivedRefund = new List<bool> { false, false };



    // =============== user_energy ===============
    
    /// 하트 (에너지)
    public int CurrentHearts = 5;

    /// 마지막 저장 시간
    public float LastTs;



    // =============== user_tutorial ===============
    /// 튜토리얼 버전 (추후 추가될 때를 대비)
    public int TutorialVersion = 0;

    /// 안내 메시지를 본 적이 있는지 여부
    public Dictionary<NoticeType, bool> HasSeenNotice = new Dictionary<NoticeType, bool>();

    /// 안내 메시지를 마지막으로 본 시간
    public Dictionary<NoticeType, DateTime> LastShownAt = new Dictionary<NoticeType, DateTime>();



    // =============== user_store ===============
    // 추가될 가능성 있고 업데이트 시간 파악을 위해 따로 만듦!
    /// 상점 구매 제한 잔여 횟수 (상품id, 횟수)
    public Dictionary<int, int> RemainingLimit = new Dictionary<int, int>();



    // =============== user_stageclear ===============
    public Dictionary<int, StageClear> StageClears = new Dictionary<int, StageClear> {
            { 5001 , new StageClear() } 
        };

    [Serializable]
    public class StageClear
    {
        /// 스테이지 아이디
        public int StageId = 5001;

        /// 몇번 클리어했는지
        public int ClearCount = 0;

        /// 별 개수
        public int Star = 0;

        // 가장 높은 점수 (골드?)
        public int BestScore = 0;

        // 가장 빠른 클리어 시간
        public int BestTimeMs = -1;

        // 처음 클리어했는지 (재시도 여부)
        public bool FirstCleared = false;
    }


    // =============== 로컬 ===============

    /// 설정 저장용
    public Settings setting = new Settings();

    [Serializable]
    public class Settings
    {
        public float JoystickValue = 55;
        public float BgmVolume = 20;
        public float SfxVolume = 20;
        public bool isMuted = false;
    }


    public event Action<int> CoinChanged;
    public event Action<int> DiaChanged;


    public bool TrySetUserName(string userName)
    {
        UserName = userName;
        _ = UserDataFile.SaveBaseAsync(this);

        return true;
    }
    
    /// 해금 레시피 추가 함수. 중복 방지를 위해 사용 권장.
    public bool TryAddRecipe(int key)
    {
        if (UnlockRecipes.Contains(key)) return false;

        UnlockRecipes.Add(key);
        _ = UserDataFile.SaveRecipeAsync(this);

        return true;
    }

    public bool TryChangeMenu(int idx, int key)
    {
        // 리스트가 비어있거나 부족하면 만들어두기
        if (SelectRecipes.Count < 3)
        {
            for (int i = 0; i < 3 - SelectRecipes.Count; i++)
            {
                SelectRecipes.Add(-1);
            }
        }

        if (SelectRecipes[idx] == key) return false; // 변동 없으면 굳이 안 바꿈

        SelectRecipes[idx] = key;
        _ = UserDataFile.SaveRecipeAsync(this);

        return true;
    }

    public bool TryConsumeCoin(int cost)
    {
        if (Coin < cost) return false;

        Coin -= cost;
        CoinChanged?.Invoke(-cost);
        _ = UserDataFile.SaveBaseAsync(this);

        return true;
    }

    public bool TryAddCoin(int cost)
    {
        Coin += cost;
        CoinChanged?.Invoke(cost);
        Debug.Log($"코인 {cost} 획득! 현재 코인: {Coin}");
        _ = UserDataFile.SaveBaseAsync(this);

        return true;
    }

    public bool TryAddDia(int cost)
    {
        Dia += cost;
        DiaChanged?.Invoke(cost);
        Debug.Log($"다이아 {cost} 획득! 현재 다이아: {Dia}");
        _ = UserDataFile.SaveBaseAsync(this);

        return true;
    }
    
    public bool TrySetHasSeenNotice(NoticeType notice, bool hasSeen)
    {
        // key가 없거나 값이 같으면 return false
        if (!HasSeenNotice.ContainsKey(notice) || HasSeenNotice[notice] == hasSeen) return false;

        HasSeenNotice[notice] = hasSeen;
        _ = UserDataFile.SaveTutorialAsync(this);

        return true;
    }
    
    public bool TrySyncHasSeenNotice()
    {
        var noticeArr = Enum.GetValues(typeof(NoticeType));
        int hasSeenNoticeCnt = HasSeenNotice.Count;  // 저장된 값 개수
        int noticeTypeCnt = noticeArr.Length;  // Enum 개수

        if (hasSeenNoticeCnt < noticeTypeCnt)
        {
            foreach (NoticeType noticeType in noticeArr)
            {
                if (!HasSeenNotice.ContainsKey(noticeType))
                {
                    HasSeenNotice[noticeType] = false;
                }
            }

            // 값이 다를 때만 저장
            _ = UserDataFile.SaveTutorialAsync(this);

            return true;
        }

        return false;
    }

    public bool TryChangeHadReceivedRefund(int idx)
    {
        // 이미 true이면 return false
        if (HadReceivedRefund[idx]) return false;

        HadReceivedRefund[idx] = true;
        _ = UserDataFile.SaveRecipeAsync(this);

        return true;
    }

    public bool TrySetRemainingLimit(int id, int remain)
    {
        RemainingLimit[id] = remain;
        _ = UserDataFile.SaveStoreAsync(this);

        return true;
    }

    public bool TryStageClear()
    {
        CurrentStageKey = Managers.Session.CurrentStageKey;

        if (CurrentStageKey + 1 > Managers.Data.Stage.ItemsList.Max(s => s.key))
        {
            Debug.Log("마지막 스테이지 클리어!");
            return false;
        }

        UpsertStageClearInfo(CurrentStageKey); // 클리어 정보 업데이트
        
        CurrentStageKey += 1;
        Managers.Session.CurrentStageKey += 1;
        Managers.Session.ChangeStage(CurrentStageKey); // 세션에도 반영

        _ = UserDataFile.SaveBaseAsync(this); // CurrentStageKey 저장
        
        Debug.Log($"스테이지 클리어! 다음 스테이지: {Managers.Session.CurrentStageKey} - {Managers.Session.StageData.DisplayName}");

        return true;
    }


    private void UpsertStageClearInfo(int stageId)
    {
        var sc = StageClears[stageId];

        sc.ClearCount ++;
        sc.Star = Managers.Session.Star;

        // TODO : BestScore, BestTimeMs 추가
        // sc.BestScore = math.max(sc.BestScore, );
        // sc.BestTimeMs = math.min(sc.BestTimeMs, );

        sc.FirstCleared = true;

        _ = UserDataFile.SaveStageClearAsync(this, stageId); 
    }
}

public static class UserDataFile
{
    /*
        버전 업할 때 여기 기록하기
        25-09-11 [VERSION 1] StageClearInfo 추가
    */
    public const int CURRENT_VERSION = 1;

    
    // 기본적으로는 전체 로드 가능, 데이터테이블 이름 넣으면 부분만 로드 가능
    public static async Task<UserData> LoadAsync(DataTableName dataTable = DataTableName.all)
    {
        try
        {
            TMP_Text sucMsg = BackendLogin.Instance.successMessage;
            sucMsg.text = "유저 데이터 세팅 중..";

            UserData data = new UserData();


            // DataTableName Enum 순

            // 0) user_base - 데이터 버전, 닉네임, 진행 중인 스테이지, 돈, 다이아
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_base)
            {
                var baseRow = await BackendRepo.GetOneAsync(DataTableName.user_base.ToString());

                if (baseRow.ok && baseRow.row != null)
                {
                    var r = baseRow.row;

                    data.Version = int.Parse(r["Version"].ToString());
                    data.UserName = r["UserName"].ToString();
                    data.StageLevel = int.Parse(r["StageLevel"].ToString());
                    data.CurrentStageKey = int.Parse(r["CurrentStageKey"].ToString());
                    data.Coin = int.Parse(r["Coin"].ToString());
                    data.Dia = int.Parse(r["Dia"].ToString());
                }
            }


            // 1) user_stats - 플레이어 스탯 관련 정보 (HpLevel ~ Critical)
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_stats)
            {
                var statsRow = await BackendRepo.GetOneAsync(DataTableName.user_stats.ToString());

                if (statsRow.ok && statsRow.row != null)
                {
                    var r = statsRow.row;
                    data.HpLevel = int.Parse(r["HpLevel"].ToString());
                    data.MaxHp = float.Parse(r["MaxHp"].ToString());
                    data.AtkLevel = int.Parse(r["AtkLevel"].ToString());
                    data.Atk = float.Parse(r["Atk"].ToString());
                    data.MoveSpeedLevel = int.Parse(r["MoveSpeedLevel"].ToString());
                    data.MoveSpeed = float.Parse(r["MoveSpeed"].ToString());
                    data.AtkSpeedLevel = int.Parse(r["AtkSpeedLevel"].ToString());
                    data.AtkSpeed = float.Parse(r["AtkSpeed"].ToString());
                    data.BagSizeLevel = int.Parse(r["BagSizeLevel"].ToString());
                    data.BagSize = int.Parse(r["BagSize"].ToString());
                    data.CriticalLevel = int.Parse(r["CriticalLevel"].ToString());
                    data.Critical = float.Parse(r["Critical"].ToString());
                }
            }


            // 2) user_recipe - 해금 레시피, 메뉴판, (보상금 수령 여부)
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_recipe)
            {
                var recipeRow = await BackendRepo.GetOneAsync(DataTableName.user_recipe.ToString());
                if (recipeRow.ok && recipeRow.row != null)
                {
                    var r = recipeRow.row;
                    data.UnlockRecipes = BackendRepo.FromJson<List<int>>(r["UnlockRecipes"].ToString());
                    data.SelectRecipes = BackendRepo.FromJson<List<int>>(r["SelectRecipes"].ToString());
                    data.HadReceivedRefund = BackendRepo.FromJson<List<bool>>((string)r["HadReceivedRefund"].ToString());
                }
            }


            // 3) user_energy - 하트 관련 정보
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_energy)
            {
                var energyRow = await BackendRepo.GetOneAsync(DataTableName.user_energy.ToString());
                if (energyRow.ok && energyRow.row != null)
                {
                    var r = energyRow.row;
                    data.CurrentHearts = int.Parse(r["CurrentHearts"].ToString());
                    data.LastTs = float.Parse(r["LastTs"].ToString());
                }
            }


            // 4) user_tutorial - 튜토리얼 안내 메시지 관련 정보
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_tutorial)
            {
                var tutRow = await BackendRepo.GetOneAsync(DataTableName.user_tutorial.ToString());
                if (tutRow.ok && tutRow.row != null)
                {
                    var r = tutRow.row;
                    data.TutorialVersion = int.Parse(r["TutorialVersion"].ToString());
                    data.HasSeenNotice = BackendRepo.FromJson<Dictionary<NoticeType, bool>>(r["HasSeenNotice"].ToString());
                    data.LastShownAt = BackendRepo.FromJson<Dictionary<NoticeType, DateTime>>(r["LastShownAt"].ToString());
                }
            }


            // 5) user_store - 상점 관련 정보
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_store)
            {
                var storeRow = await BackendRepo.GetOneAsync(DataTableName.user_store.ToString());
                if (storeRow.ok && storeRow.row != null)
                {
                    var r = storeRow.row;
                    data.RemainingLimit = BackendRepo.FromJson<Dictionary<int, int>>(r["RemainingLimit"].ToString());
                }
            }

            // 6) user_stageclear - 스테이지 클리어 관련 정보 (별, 최고기록 등)
            if (dataTable == DataTableName.all || dataTable == DataTableName.user_stageclear)
            {
                var (ok, rows) = await BackendRepo.GetManyAsync(DataTableName.user_stageclear.ToString());
                if (ok && rows != null)
                {
                    data.StageClears = new Dictionary<int, UserData.StageClear>();
                    foreach (LitJson.JsonData r in rows)
                    {
                        int stageId = int.Parse(r["StageId"].ToString());
                        data.StageClears[stageId] = new UserData.StageClear
                        {
                            StageId = stageId,
                            ClearCount = int.Parse(r["ClearCount"].ToString()),
                            Star = int.Parse(r["Star"].ToString()),
                            BestScore = int.Parse(r["BestScore"].ToString()),
                            BestTimeMs = int.Parse(r["BestTimeMs"].ToString()),
                            FirstCleared = bool.Parse(r["FirstCleared"].ToString())
                        };
                    }
                }
            }


            if (dataTable == DataTableName.all || dataTable == DataTableName.user_settings)
            {
                // settings (로컬 전용)
                data.setting = LoadSettingsLocalOrDefault(data.setting);
            }


            if (data.Version == 0)  // Version 변수 처음 적용
            {
                data.Version = 1; // 최초 버전 설정
            }

            if (Migrate(data, data.Version, CURRENT_VERSION) || data.Version != CURRENT_VERSION)
            {
                data.Version = CURRENT_VERSION;
                Save(data);

                return data;
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"User data load failed.\n{e}");
            BackendLogin.Instance.successMessage.text = $"User data load failed.\n{e}";
            return null;
        }

    }

    // true = 실제 데이터 변경이 있었다
    private static bool Migrate(UserData data, int fromVersion, int toVersion)
    {
        bool changed = false;

        // v1 -> v2: 이런식으로 추가하면 될듯
        // if (fromVersion < 2) { }

        return changed;
    }

    private static bool _isSuccessAllSave = false;

    // 기존 함수 연결
    public static IEnumerator SaveAllAsync(UserData userData)
    {
        _isSuccessAllSave = false;
        Save(userData);
        yield return new WaitUntil(() => _isSuccessAllSave);
    }

    // 모든 데이터 전체 저장 (잘 안쓸듯)
    public static void Save(UserData userData)
    {
        try
        {
            if (userData == null)
            {
                Debug.LogError("Save failed. user data is null.");
                return;
            }

            var tran = new List<TransactionValue>();

            // user_base
            var baseRow = new Param();
            baseRow.Add("Version", userData.Version);
            baseRow.Add("UserName", userData.UserName);
            baseRow.Add("StageLevel", userData.StageLevel);
            baseRow.Add("CurrentStageKey", userData.CurrentStageKey);
            baseRow.Add("Coin", userData.Coin);
            baseRow.Add("Dia", userData.Dia);
            tran.Add(TransactionValue.SetInsert(DataTableName.user_base.ToString(), baseRow));

            // user_stats
            var statsRow = new Param();
            statsRow.Add("HpLevel", userData.HpLevel);
            statsRow.Add("MaxHp", userData.MaxHp);
            statsRow.Add("AtkLevel", userData.AtkLevel);
            statsRow.Add("Atk", userData.Atk);
            statsRow.Add("MoveSpeedLevel", userData.MoveSpeedLevel);
            statsRow.Add("MoveSpeed", userData.MoveSpeed);
            statsRow.Add("AtkSpeedLevel", userData.AtkSpeedLevel);
            statsRow.Add("AtkSpeed", userData.AtkSpeed);
            statsRow.Add("BagSizeLevel", userData.BagSizeLevel);
            statsRow.Add("BagSize", userData.BagSize);
            statsRow.Add("CriticalLevel", userData.CriticalLevel);
            statsRow.Add("Critical", userData.Critical);
            tran.Add(TransactionValue.SetInsert(DataTableName.user_stats.ToString(), statsRow));

            // user_recipe
            var recipeRow = new Param();
            recipeRow.Add("UnlockRecipes", BackendRepo.ToJson(userData.UnlockRecipes));
            recipeRow.Add("SelectRecipes", BackendRepo.ToJson(userData.SelectRecipes));
            recipeRow.Add("HadReceivedRefund", BackendRepo.ToJson(userData.HadReceivedRefund));
            tran.Add(TransactionValue.SetInsert(DataTableName.user_recipe.ToString(), recipeRow));

            // user_energy
            var energyRow = new Param();
            energyRow.Add("CurrentHearts", userData.CurrentHearts);
            energyRow.Add("LastTs", userData.LastTs);
            tran.Add(TransactionValue.SetInsert(DataTableName.user_energy.ToString(), energyRow));

            // user_tutorial
            var tutRow = new Param();
            tutRow.Add("TutorialVersion", userData.TutorialVersion);
            tutRow.Add("HasSeenNotice", BackendRepo.ToJson(userData.HasSeenNotice));
            tutRow.Add("LastShownAt", BackendRepo.ToJson(userData.LastShownAt));
            tran.Add(TransactionValue.SetInsert(DataTableName.user_tutorial.ToString(), tutRow));

            // user_store
            var storeRow = new Param();
            storeRow.Add("RemainingLimit", BackendRepo.ToJson(userData.RemainingLimit));
            tran.Add(TransactionValue.SetInsert(DataTableName.user_store.ToString(), storeRow));

            // user_stageclear (여러 행)
            foreach (var sc in userData.StageClears.Values)
            {
                var scRow = new Param();
                scRow.Add("StageId", sc.StageId);
                scRow.Add("ClearCount", sc.ClearCount);
                scRow.Add("Star", sc.Star);
                scRow.Add("BestScore", sc.BestScore);
                scRow.Add("BestTimeMs", sc.BestTimeMs);
                scRow.Add("FirstCleared", sc.FirstCleared);
                tran.Add(TransactionValue.SetInsert(DataTableName.user_stageclear.ToString(), scRow));
            }

            // 트랜잭션 실행
            var bro = Backend.GameData.TransactionWriteV2(tran);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"SaveAll Transaction failed: {bro}");
            }

            _isSuccessAllSave = true;

            // 로컬 데이터 세팅
            SaveSettingsLocal(userData.setting);
        }
        catch (Exception e)
        {
            Debug.LogError($"User data save failed.\n{e}");
        }
    }

    // public static async Task SaveAllAsync(UserData userData)
    // {
    //     try
    //     {
    //         if (userData == null)
    //         {
    //             Debug.LogError("Save failed. user data is null.");
    //             return;
    //         }

    //         // 테이블별 저장 (await로 순차 처리, 필요시 WhenAll로 병렬화 가능)
    //         await SaveBaseAsync(userData);
    //         await SaveStatsAsync(userData);
    //         await SaveRecipeAsync(userData);
    //         await SaveEnergyAsync(userData);
    //         await SaveTutorialAsync(userData);
    //         await SaveStoreAsync(userData);

    //         // 스테이지 여러개 한번에 저장
    //         await SaveStageClearAllAsync(userData);

    //         // setting은 로컬 파일
    //         SaveSettingsLocal(userData.setting);
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"User data save failed.\n{e}");
    //     }
    // }


    // user_base 저장
    public static async Task SaveBaseAsync(UserData userData)
    {
        var row = new Dictionary<string, object>
        {
            ["Version"] = userData.Version,
            ["UserName"] = userData.UserName,
            ["StageLevel"] = userData.StageLevel,
            ["CurrentStageKey"] = userData.CurrentStageKey,
            ["Coin"] = userData.Coin,
            ["Dia"] = userData.Dia
        };
        await BackendRepo.UpsertAsync(DataTableName.user_base.ToString(), row);
    }

    // user_stats 저장
    public static async Task SaveStatsAsync(UserData userData)
    {
        var row = new Dictionary<string, object>
        {
            ["HpLevel"]         = userData.HpLevel,
            ["MaxHp"]           = userData.MaxHp,
            ["AtkLevel"]        = userData.AtkLevel,
            ["Atk"]             = userData.Atk,
            ["MoveSpeedLevel"]  = userData.MoveSpeedLevel,
            ["MoveSpeed"]       = userData.MoveSpeed,
            ["AtkSpeedLevel"]   = userData.AtkSpeedLevel,
            ["AtkSpeed"]        = userData.AtkSpeed,
            ["BagSizeLevel"]    = userData.BagSizeLevel,
            ["BagSize"]         = userData.BagSize,
            ["CriticalLevel"]   = userData.CriticalLevel,
            ["Critical"]        = userData.Critical
        };
        await BackendRepo.UpsertAsync(DataTableName.user_stats.ToString(), row);
    }

    // user_recipe 저장
    public static async Task SaveRecipeAsync(UserData userData)
    {
        var row = new Dictionary<string, object>
        {
            ["UnlockRecipes"]     = BackendRepo.ToJson(userData.UnlockRecipes),
            ["SelectRecipes"]     = BackendRepo.ToJson(userData.SelectRecipes),
            ["HadReceivedRefund"] = BackendRepo.ToJson(userData.HadReceivedRefund)
        };
        await BackendRepo.UpsertAsync(DataTableName.user_recipe.ToString(), row);
    }

    // user_energy 저장
    public static async Task SaveEnergyAsync(UserData userData)
    {
        var row = new Dictionary<string, object>
        {
            ["CurrentHearts"] = userData.CurrentHearts,
            ["LastTs"]        = userData.LastTs
        };
        await BackendRepo.UpsertAsync(DataTableName.user_energy.ToString(), row);
    }

    // user_tutorial 저장
    public static async Task SaveTutorialAsync(UserData userData)
    {
        var row = new Dictionary<string, object>
        {
            ["TutorialVersion"] = userData.TutorialVersion,
            ["HasSeenNotice"] = BackendRepo.ToJson(userData.HasSeenNotice),
            ["LastShownAt"] = BackendRepo.ToJson(userData.LastShownAt),
        };
        await BackendRepo.UpsertAsync(DataTableName.user_tutorial.ToString(), row);
    }

    // user_store 저장
    public static async Task SaveStoreAsync(UserData userData)
    {
        var row = new Dictionary<string, object>
        {
            ["RemainingLimit"] = BackendRepo.ToJson(userData.RemainingLimit)
        };
        await BackendRepo.UpsertAsync(DataTableName.user_store.ToString(), row);
    }

    // user_stageclear 저장
    // 스테이지 하나만 저장
    public static async Task SaveStageClearAsync(UserData userData, int stageId)
    {
        // userData.StageClears에서 해당 스테이지 찾기
        var sc = userData.StageClears[stageId];
        if (sc == null) return; // 없으면 아무 것도 안 함

        var row = new Dictionary<string, object>
        {
            ["StageId"] = sc.StageId,          // ★ 다중행 키 (반드시 포함)
            ["ClearCount"] = sc.ClearCount,
            ["Star"] = sc.Star,
            ["BestScore"] = sc.BestScore,
            ["BestTimeMs"] = sc.BestTimeMs,
            ["FirstCleared"] = sc.FirstCleared
        };

        // 다중행 Upsert: (table, keyName, keyValue, row)
        await BackendRepo.UpsertByKeyAsync(
            DataTableName.user_stageclear.ToString(),
            "StageId", stageId,
            row
        );
    }
    
    // 스테이지 전부 저장
    public static async Task SaveStageClearAllAsync(UserData userData)
    {
        if (userData.StageClears == null || userData.StageClears.Count == 0) return;

        var tasks = new List<Task>();
        foreach (var sc in userData.StageClears.Values)
        {
            var row = new Dictionary<string, object>
            {
                ["StageId"]    = sc.StageId,
                ["ClearCount"] = sc.ClearCount,
                ["Star"]       = sc.Star,
                ["BestScore"]  = sc.BestScore,
                ["BestTimeMs"] = sc.BestTimeMs
            };

            tasks.Add(BackendRepo.UpsertByKeyAsync(
                DataTableName.user_stageclear.ToString(),
                "StageId", sc.StageId,
                row
            ));
        }
        await Task.WhenAll(tasks);
    }


    // settings 저장
    static string SettingsPath = Path.Combine(Application.persistentDataPath, "UserSettings.json");

    public static void SaveSettingsLocal(UserData.Settings s)
    {
        var json = BackendRepo.ToJson(s);
        File.WriteAllText(SettingsPath, json, System.Text.Encoding.UTF8);
    }

    // settings 로더
    private static UserData.Settings LoadSettingsLocalOrDefault(UserData.Settings def)
    {
        if (!File.Exists(SettingsPath)) return def;
        var json = File.ReadAllText(SettingsPath, System.Text.Encoding.UTF8);
        var loaded = BackendRepo.FromJson<UserData.Settings>(json);
        return loaded ?? def;
    }


    public static void ResetStat()
    {
        Managers.Data.User.HpLevel = 0;
        Managers.Data.User.MaxHp = 150f;
        Managers.Data.User.AtkLevel = 0;
        Managers.Data.User.Atk = 12f;
        Managers.Data.User.MoveSpeedLevel = 0;
        Managers.Data.User.MoveSpeed = 5f;
        Managers.Data.User.AtkSpeedLevel = 0;
        Managers.Data.User.AtkSpeed = 1f;
        Managers.Data.User.BagSizeLevel = 0;
        Managers.Data.User.BagSize = 500;
        Managers.Data.User.CriticalLevel = 0;
        Managers.Data.User.Critical = 0.1f;
    }
}

