using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private Transform[] _playerLoc;
    [SerializeField] private GameObject _player;


    [Header("Scene")]
    [SerializeField] private GameObject _mainScene;
    [SerializeField] private GameObject _setUpScene;
    [SerializeField] private GameObject _guide;


    [Header("Error Popup")]
    [SerializeField] private RectTransform _errorPanel;
    [SerializeField] private GameObject _errorPopup;
    // [SerializeField] private TMP_Text _errorContentText;


    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGM(SoundManager.Instance.BgmClips[0].name);

        _player.GetComponent<Transform>().position = _playerLoc[0].position;
        _player.SetActive(true);

        if (Managers.Session.hasVisitedGame) MonsterToLobby();

        _guide.SetActive(Managers.Session.isTutorial);
    }

    public void ActivateTempPopup()
    {
        ActivateErrorPopup("준비 중입니다.");
    }

    public void PlayButtonClick()
    {
        _player.SetActive(false);
        _mainScene.SetActive(false);
        _setUpScene.SetActive(true);

        _player.GetComponent<Transform>().position = _playerLoc[1].position;
        _player.SetActive(true);
    }

    public void HomeButtonClick()
    {
        _player.SetActive(false);
        _mainScene.SetActive(true);
        _setUpScene.SetActive(false);

        _player.GetComponent<Transform>().position = _playerLoc[0].position;
        _player.SetActive(true);
    }

    public void ActivateErrorPopup(string content)
    {
        // _errorContentText.text = content;
        // _errorPopup.SetActive(true);

        var view = Instantiate(_errorPopup, _errorPanel);

        view.transform.GetChild(0).GetComponent<TMP_Text>().text = content;
        view.SetActive(true);
    }

    public void MonsterToLobby()
    {
        _player.SetActive(false);

        _mainScene.SetActive(false);
        _setUpScene.SetActive(true);
    }

    public void GoStoryScene()
    {
        SceneManager.LoadScene("StoryScene");
    }
}
