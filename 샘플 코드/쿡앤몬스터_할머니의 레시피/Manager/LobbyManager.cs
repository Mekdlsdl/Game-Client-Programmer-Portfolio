using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

using BackEnd;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private Transform[] _playerLoc;
    [SerializeField] private GameObject _player;

    [SerializeField] private GameObject _nicknamePanel;
    [SerializeField] private TMP_Text _nicknameText;


    [Header("Scene")]
    [SerializeField] private GameObject _mainScene;
    [SerializeField] private GameObject _setUpScene;
    [SerializeField] private GameObject _chapterSelectScene;
    [SerializeField] private GameObject _guide;


    [Header("Error Popup")]
    public RectTransform _errorPanel;
    [SerializeField] private GameObject _errorPopup;
    // [SerializeField] private TMP_Text _errorContentText;


    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;

        if (Managers.Session.hasVisitedGame) GoToSetUpScene();
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGM(SoundManager.Instance.BgmClips[0].name);

        StartCoroutine(RefrashScene());
    }

    private IEnumerator RefrashScene()
    {
        yield return Managers.Instance.WaitUntilReadyCoroutine();

        GoToMainScene();
        SetNicknamePanel();
        _guide.SetActive(Managers.Session.isTutorial);

        // 해금된 레시피 초기값
        if (Managers.Data.User.UnlockRecipes.Count < 3)
        {
            List<int> unlockRecipes = Managers.Data.Recipes.ItemsList
                .Where(r => r.FirstRecipeStatus)
                .Select(r => r.key).ToList();

            for (int i = 0; i < unlockRecipes.Count; i++)
            {
                int recipe = unlockRecipes[i];
                Managers.Data.User.TryAddRecipe(recipe);
                Managers.Data.User.TryChangeMenu(i, recipe);
            }
        }
    }

    public void SetNicknamePanel()
    {
        string userName = Managers.Data.User.UserName;

        // 닉네임 설정했으면 패널 끄기
        if (userName == null || userName == "" || userName.Length < 2)
        {
            _nicknamePanel.SetActive(true); // 안 켜져있을 것 대비
        }
        else
        {
            _nicknameText.text = Managers.Data.User.UserName;
            _nicknamePanel.SetActive(false);
        }
    }

    public void ActivateTempPopup()
    {
        ActivateErrorPopup("준비 중입니다.");
    }

    public void PlayButtonClick()
    {
        GoToChapterSelectScene();
    }

    public void HomeButtonClick()
    {
        GoToMainScene();
    }


    // 닉네임 설정
    public void UserNameButtonClick(TMP_Text userNameText)
    {
        string userName = userNameText.text.Trim();

        if (SetUserName(userName))
        {
            _nicknamePanel.SetActive(false);
        }
    }

    public bool SetUserName(string userName)
    {
        if (!CheckingNameValidation(userName))
            return false;

        var bro = Backend.BMember.UpdateNickname(userName);

        if (!bro.IsSuccess())
        {
            ActivateErrorPopup("닉네임 설정을 실패했습니다.");
            Debug.Log(bro.Message);
        }
        else
        {
            Managers.Data.User.TrySetUserName(userName);
            _nicknameText.text = Managers.Data.User.UserName;
        }

        return bro.IsSuccess();
    }

    public bool CheckingNameValidation(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            ActivateErrorPopup("닉네임을 입력해주세요.");
            return false;
        }

        if (userName.Length > 12) // 뒤끝 닉네임 최대 길이
        {
            ActivateErrorPopup(string.Format("닉네임의 길이는 {0}글자를 넘을 수 없습니다.", 12));
            return false;
        }

        if (userName.Contains(' '))
        {
            ActivateErrorPopup("닉네임에 띄어쓰기를 포함할 수 없습니다.");
            return false;
        }

        var bro = Backend.BMember.CheckNicknameDuplication(userName);
        if (!bro.IsSuccess())
        {
            if (bro.StatusCode == 409)
            {
                ActivateErrorPopup("이미 존재하는 닉네임입니다.");
            }

            else
            {
                ActivateErrorPopup("닉네임 설정을 실패했습니다.");
                Debug.Log(bro.Message);
            }

            return false;
        }

        return true;
    }


    // ================= 에러 팝업 =================
    public void ActivateErrorPopup(string content)
    {
        _errorPanel.gameObject.SetActive(true);

        var view = Instantiate(_errorPopup, _errorPanel);

        view.transform.GetChild(0).GetComponent<TMP_Text>().text = content;
        view.SetActive(true);
    }

    public void InactiveErrorPopup()
    {
        if (_errorPanel.transform.childCount > 1) return;

        _errorPanel.gameObject.SetActive(false);
    }

    // ================= 씬 전환 =================
    public void GoToSetUpScene()
    {
        _player.SetActive(false);

        _mainScene.SetActive(false);
        _chapterSelectScene.SetActive(false);
        _setUpScene.SetActive(true);

        _player.GetComponent<Transform>().position = _playerLoc[1].position;
        _player.SetActive(true);
    }

    public void GoToMainScene()
    {
        _player.SetActive(false);
        _mainScene.SetActive(true);
        _setUpScene.SetActive(false);
        _chapterSelectScene.SetActive(false);

        _player.GetComponent<Transform>().position = _playerLoc[0].position;
        _player.SetActive(true);

        _nicknameText.text = Managers.Data.User.UserName;
    }

    public void GoToChapterSelectScene()
    {
        _player.SetActive(false);
        _mainScene.SetActive(false);
        _chapterSelectScene.SetActive(true);
        ChapterSelectController.Instance.SetChapter();
    }

    public void GoStoryScene()
    {
        SceneManager.LoadScene("StoryScene");
    }
}
