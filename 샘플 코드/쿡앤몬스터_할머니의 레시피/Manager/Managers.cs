using UnityEngine;
using System.Collections;


public class Managers : MonoBehaviour
{
    private static Managers instance;
    public static Managers Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<Managers>();
                if (instance == null)
                {
                    GameObject go = new GameObject("@Managers");
                    instance = go.AddComponent<Managers>();

                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public static DataManager Data => Instance._data;
    public static PoolManager Pool => Instance._pool;
    public static StageSession Session => Instance._session;

    private DataManager _data;
    private PoolManager _pool;
    private StageSession _session;

    private bool _initializing;
    private bool _ready;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EnsureInitialized()
    {
        if (_ready || _initializing) return;
        _initializing = true;
        StartCoroutine(InitializeRoutine());
    }

    private IEnumerator InitializeRoutine()
    {
        // 1) 로그인 완료까지 대기
        yield return BackendManager.Instance.WaitAuthCoroutine();

        // BackendLogin.Instance.successMessage.text = "Managers 로드";
        // 2) 초기화 (여기 내부에서는 Managers.* 다시 참조하지 말기)
        _data = gameObject.AddComponent<DataManager>();
        _data.InitSync(BackendManager.Instance.CurrentUser); // I/O 없는 메모리 세팅만

        _pool = gameObject.AddComponent<PoolManager>();
        _pool.Init();

        _session = new StageSession(_data.User.StageLevel);

        _ready = true;
        _initializing = false;
    }

    public IEnumerator WaitUntilReadyCoroutine()
    {
        EnsureInitialized();
        yield return new WaitUntil(() => _ready);
    }

    public bool IsReady => _ready;
}