using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 뒤끝 SDK namespace 추가
using BackEnd;
using TMPro;

public class BackendLogin : MonoBehaviour
{
    private static BackendLogin _instance = null;

    public TMP_Text successMessage;

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
            successMessage.text = "로그인 성공 신호 보내는 중..";
            StartCoroutine(LoadAndGo(isFirst));
        });
    }
    
    private IEnumerator LoadAndGo(bool isFirst)
    {
        // TODO : 나중에 데이터 많아져서 너무 느리다 싶으면 로드 나누기
        // var t = UserDataFile.LoadAsync(DataTableName.user_base);

        if (isFirst || PlayerPrefs.GetInt("data_all_load", 0) == 0) // 테스트할땐 주석처리
        {
            // 처음이면 한번 upsert하고 가기
            var userData = new UserData();
            var s = UserDataFile.SaveAllAsync(userData);
            successMessage.text = "데이터 세팅 중...";
            yield return new WaitUntil(() => s.IsCompleted);
            PlayerPrefs.SetInt("data_all_load", 1);
        }

        BackendRepo.InitializeQueueAndFlusher();

        var t = UserDataFile.LoadAsync();
        yield return new WaitUntil(() => t.IsCompleted);

        var user = t.Result ?? new UserData();
        BackendManager.Instance.OnAuthorized(user);

        Managers.Instance.EnsureInitialized();
        yield return BackendManager.Instance.StartCoroutine(Managers.Instance.WaitUntilReadyCoroutine());
        // successMessage.text = "거의 다 됐어요!";
        SceneManager.LoadScene("LobbyScene");
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