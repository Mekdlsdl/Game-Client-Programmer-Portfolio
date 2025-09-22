using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 뒤끝 SDK namespace 추가
using BackEnd;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }
    public bool IsAuthorized { get; private set; } = false;
    public UserData CurrentUser { get; private set; }

    [SerializeField] private GameObject _loginButton;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _loadingPlayer;
    [SerializeField] private Image _loadingBar;

    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
            // _loginButton.SetActive(true);
            StartCoroutine(TryAutoLoginOrShowButton());
        }
        else
        {
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
        }
    }

    private IEnumerator TryAutoLoginOrShowButton()
    {
        _loginButton.SetActive(false);

        // 예전에 구글 로그인한 적 있으면 자동 시도
        if (PlayerPrefs.GetInt("remember_google", 0) == 1)
        {
            BackendLogin.Instance.successMessage.text = "자동 로그인 중..";
            BackendLogin.Instance.StartGoogleLogin();

            // 잠깐 기다렸다가(네트워크 지연 대비) 성공 못 하면 버튼 노출
            float timeout = 15f;
            float t = 0f;
            while (!IsAuthorized && t < timeout)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!IsAuthorized)
            {
                Debug.Log("[AutoLogin] 실패 또는 취소 → 버튼 노출");
                BackendLogin.Instance.successMessage.text = "자동 로그인을 실패했습니다.\n다시 로그인해주세요!";
                _loginButton.SetActive(true);
            }
            yield break;
        }

        // 기록 없으면 버튼 켜기
        _loginButton.SetActive(true);
    }

    // 로그인 성공 시 반드시 한 번 호출
    public void OnAuthorized(UserData user)
    {
        CurrentUser = user;
        // BackendLogin.Instance.successMessage.text = "로그인 성공";
        IsAuthorized = true;
        _loginButton.SetActive(false);
    }

    // 코루틴에서 기다릴 때 사용
    public IEnumerator WaitAuthCoroutine()
    {
        // BackendLogin.Instance.successMessage.text = "로그인 성공 기다리는 중";
        yield return new WaitUntil(() => IsAuthorized);
    }

    public void TapToStartClick()
    {
        // SceneManager.LoadScene("LobbyScene");
        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        _loadingPanel.SetActive(true);
        _loadingPlayer.SetActive(true);
        _loadingBar.fillAmount = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync("LobbyScene");
        op.allowSceneActivation = false;

        float time = 0f;
        while(!op.isDone)
        {
            yield return null;
            _loadingBar.fillAmount = op.progress;

            if (op.progress > 0.8f)
            {
                time += Time.deltaTime / 1f;
                _loadingBar.fillAmount = Mathf.Lerp(0.8f, 1f, time);
                _loadingBar.fillAmount = 1f;

                yield return new WaitUntil(() => _loadingBar.fillAmount == 1f);
                
                // _loadingPanel.SetActive(false);
                // _loadingPlayer.SetActive(false);

                op.allowSceneActivation = true;

                yield break;
            }
        }
    }

    async void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            await BackendRepo.FlushOnceAsync();
            BackendRepo.ShutdownFlusher();
        }
        else
        {
            BackendRepo.InitializeQueueAndFlusher();
            _ = BackendRepo.FlushOnceAsync();
        }
    }

    async void OnApplicationQuit()
    {
        await BackendRepo.FlushOnceAsync();
        BackendRepo.ShutdownFlusher();
    }
}