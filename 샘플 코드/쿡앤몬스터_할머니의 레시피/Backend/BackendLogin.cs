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
                _instance = new BackendLogin();
            }

            return _instance;
        }
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

        Debug.Log("구글 토큰 : " + token);
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("페데레이션 로그인 결과 : " + bro);
        // successMessage.text = $"로그인 성공!\n\n페데레이션 로그인 결과 : {bro}";
        successMessage.text = "Loading...";
        SceneManager.LoadScene("LobbyScene");
    }

    public void SignOutGoogleLogin()
    {
        TheBackend.ToolKit.GoogleLogin.Android.GoogleSignOut(GoogleSignOutCallback);
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


    public void UpdateNickname(string nickname)
    {
        // TODO : 닉네임 변경 기능
    }
}