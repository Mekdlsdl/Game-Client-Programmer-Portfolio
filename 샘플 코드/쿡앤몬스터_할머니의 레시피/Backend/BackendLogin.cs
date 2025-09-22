using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// 뒤끝 SDK namespace 추가
using BackEnd;
using UnityEngine.UI;
using Unity.Mathematics;


public class BackendLogin : MonoBehaviour
{
    private static BackendLogin _instance = null;

    public TMP_Text successMessage;
    [SerializeField] private GameObject _startText;
    [SerializeField] private Button _background;
    [SerializeField] private Image _loadingBar;

    private float _time = 0f;
    private float _start = 0f;

    public static BackendLogin Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BackendLogin>();
            }

            return _instance;
        }
    }

    private void Awake() 
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        _background.interactable = false;
    }

    public void StartGoogleLogin()
    {
        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(GoogleLoginCallback);
    }

    private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
    {
        if (isSuccess == false)
        {
            Debug.LogError(errorMessage);
            successMessage.text = $"로그인 실패..\n\n{errorMessage}";
            return;
        }

        bool isFirst = true;

        if (PlayerPrefs.GetInt("remember_google", 0) == 1) isFirst = false;

        Debug.Log("구글 토큰 : " + token);
        PlayerPrefs.SetInt("remember_google", 1); // 자동 로그인용
        PlayerPrefs.Save();

        successMessage.text = "Loading...";
        Backend.BMember.AuthorizeFederation(token, FederationType.Google, bro =>
        {
            //로딩 바 1/5 채우기
            _time = 0f;
            _start = _loadingBar.fillAmount;
            while (!bro.IsSuccess())
            {
                _time += Time.deltaTime / 0.01f;
                _loadingBar.fillAmount = Mathf.Lerp(_start, 0.2f, _time);
            }
            _loadingBar.fillAmount = 0.2f;
            
            if (!bro.IsSuccess())
            {
                Debug.LogError($"페데레이션 실패: {bro}");
                successMessage.text = $"로그인 실패..\n\n{bro}";
                return;
            }

            if (BackendManager.Instance == null)
            {
                Debug.LogError("[Login] BackendManager.Instance is null (DDOL 세팅 확인)");
                return;
            }
            // successMessage.text = "로그인 성공 신호 보내는 중..";
            StartCoroutine(LoadAndGo(isFirst));
        });
    }

    private IEnumerator LoadAndGo(bool isFirst)
    {
        BackendRepo.InitializeQueueAndFlusher();

        // 서버에 user_base가 이미 있는지 확인
        var probe = BackendRepo.GetManyAsync(DataTableName.user_base.ToString());
        yield return new WaitUntil(() => probe.IsCompleted);

        // 로딩 바 2/5 채우기
        _time = 0f;
        _start = 0.2f;
        while (!probe.IsCompleted)
        {
            _time += Time.deltaTime / 0.01f;
            _loadingBar.fillAmount = Mathf.Lerp(_start, 0.4f, _time);
        }
        _loadingBar.fillAmount = 0.4f;

        bool hasServerBase =
            probe.Result.ok &&
            probe.Result.rows != null &&
            probe.Result.rows.Count > 0;

        // TODO : 나중에 데이터 많아져서 너무 느리다 싶으면 로드 나누기
        // var t = UserDataFile.LoadAsync(DataTableName.user_base);

        // 앱 재설치해도 서버에 데이터 있으면 업서트 건너뜀
        if (!hasServerBase && (isFirst || PlayerPrefs.GetInt("data_all_load", 0) == 0))
        {
            // 처음이면 한번 upsert하고 가기
            successMessage.text = "데이터 세팅 중...";
            var userData = new UserData();
            yield return UserDataFile.SaveAllAsync(userData);

            // 바로 반영
            var flushTask = BackendRepo.FlushOnceAsync();
            yield return new WaitUntil(() => flushTask.IsCompleted);
        }

        PlayerPrefs.SetInt("data_all_load", 1);
        PlayerPrefs.Save();

        // 정식 로드
        var t = UserDataFile.LoadAsync();
        yield return new WaitUntil(() => t.IsCompleted);

        // 로딩 바 3/5 채우기
        _time = 0f;
        _start = 0.4f;
        while (!t.IsCompleted)
        {
            _time += Time.deltaTime / 0.01f;
            _loadingBar.fillAmount = Mathf.Lerp(_start, 0.6f, _time);
        }
        _loadingBar.fillAmount = 0.6f;

        var user = t.Result ?? new UserData();
        BackendManager.Instance.OnAuthorized(user);

        Managers.Instance.EnsureInitialized();
        yield return BackendManager.Instance.StartCoroutine(Managers.Instance.WaitUntilReadyCoroutine());

        // 로딩 바 4/5 채우기
        _time = 0f;
        _start = 0.6f;
        while (!Managers.Instance.IsReady)
        {
            _time += Time.deltaTime / 0.01f;
            _loadingBar.fillAmount = Mathf.Lerp(_start, 0.8f, _time);
        }
        _loadingBar.fillAmount = 0.8f;

        successMessage.text = "거의 다 됐어요!";
        // SceneManager.LoadScene("LobbyScene");

        // 로딩 바 나머지 다 채우기
        _time = 0f;
        _start = 0.8f;
        while (_time < 1f)
        {
            _time += Time.deltaTime / 2f;
            _loadingBar.fillAmount = Mathf.Lerp(_start, 1f, _time);
        }

        _loadingBar.fillAmount = 1f;

        yield return new WaitUntil(() => _loadingBar.fillAmount == 1f);

        successMessage.text = "";
        _startText.SetActive(true);
        _background.interactable = true;
    }

    public void SignOutGoogleLogin()
    {
        TheBackend.ToolKit.GoogleLogin.Android.GoogleSignOut(GoogleSignOutCallback);
        PlayerPrefs.DeleteKey("remember_google");
        PlayerPrefs.Save();
    }

    private void GoogleSignOutCallback(bool isSuccess, string error)
    {
        if (isSuccess == false)
        {
            Debug.Log("구글 로그아웃 에러 응답 발생 : " + error);
        }
        else
        {
            Debug.Log("로그아웃 성공");
        }
    }
}