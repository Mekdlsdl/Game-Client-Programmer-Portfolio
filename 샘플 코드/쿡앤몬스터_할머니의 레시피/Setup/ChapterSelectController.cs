using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ChapterSelectController : MonoBehaviour
{
    public static ChapterSelectController Instance { get; private set; }

    [SerializeField] private TMP_Text _title;
    [SerializeField] private GameObject _chapterPanel;
    [SerializeField] private GameObject _homeButton;
    [SerializeField] private GameObject _stagePanel;
    [SerializeField] private Image _stageBackground;
    [SerializeField] private List<Sprite> _stageBacks;

    [Header("Chapter Button")]
    [SerializeField] private Transform _chapterContent;
    [SerializeField] private GameObject _chapterButton;
    [SerializeField] private List<Sprite> _chapterBacks;

    [Header("Stage Button")]
    [SerializeField] private Transform _stageContent;
    [SerializeField] private GameObject _stageButton;
    [SerializeField] private GameObject _emptyButton;

    private List<GameObject> _chapterPool = new List<GameObject>();
    private List<GameObject> _stagePool = new List<GameObject>();
    private List<GameObject> _emptyPool = new List<GameObject>();

    private List<string> _chapterNames = new List<string> // 나중에 데테 나오게 되면 교체
    {
        "붉은모래 행성 바라칸", "얼음의 행성 프리지노스", "초원의 행성 실바니아", "사막의 행성 아레나스",
        "제노스의 요새", "황토 협곡 테라코타", "용암의 행성 인페르니스"
    };


    void Awake()
    {
        Instance = this;
    }
    
    void OnEnable()
    {
        
    }
    
    // =============== Chapter ===============
    // 버튼 풀링
    public void SetChapter()
    {
        // _chapterPool = new List<GameObject>();
        // int totalChapter = Managers.Data.Stage.ItemsDict.Count - 1;
        int totalChapter = _chapterNames.Count;

        for (int i = 0; i < totalChapter; i++)
        {
            var chapterButton = Instantiate(_chapterButton, _chapterContent);
            _chapterPool.Add(chapterButton);
            SetChapterButton(i);
        }
    }

    // 버튼 배경, 번호, 이름 세팅
    private void SetChapterButton(int chapId)
    {
        var chapButton = _chapterPool[chapId].GetComponent<ChapterSelectButton>();
        bool isUnlocked = (chapId + 1 <= Managers.Data.Stage.ItemsDict[Managers.Data.User.StageLevel].Chapter) ? true : false;

        chapButton.SetButton(chapId, isUnlocked, _chapterBacks[chapId], _chapterNames[chapId]);
    }

    public void ChapterButtonClick(ChapterSelectButton chapButton)
    {
        if (!chapButton.IsUnlocked) return;

        // 챕터 패널 비활성화
        _chapterPanel.SetActive(false);
        _homeButton.SetActive(false);

        _stagePanel.SetActive(true);

        // 타이틀 - '스테이지 선택'으로 변경
        _title.text = "스테이지 선택";

        // TODO : 배경 세팅 - 에셋 받으면 넣고 주석 해제
        // _stageBackground.sprite = _stageBacks[chapButton.ChapterId];

        SetStage(chapButton.ChapterId);
    }

    public void HomeButtonClick()
    {
        foreach (GameObject button in _chapterPool)
        {
            Destroy(button);
        }
        _chapterPool.Clear();

        LobbyManager.Instance.GoToMainScene();
        gameObject.SetActive(false);
    }


    // =============== Stage ===============
    // TODO : 버튼 위치 세팅
    private void SetStage(int chapId)
    {
        var stageList = Managers.Data.Stage.ItemsDict
            .Where(k => k.Value.Chapter == chapId + 1)
            .OrderBy(k => k.Key).ToList();
        int totalStage = stageList.Count;
        // LobbyManager.Instance.ActivateErrorPopup($"스테이지 총 {totalStage}개");

        int cnt = -1;
        int i = 0;

        while (cnt < totalStage - 1)
        {
            if (i % 4 == 1 || i % 4 == 2)
            {
                var stageButton = Instantiate(_emptyButton, _stageContent);
                _emptyPool.Add(stageButton);
            }
            else
            {
                var stageButton = Instantiate(_stageButton, _stageContent);
                _stagePool.Add(stageButton);
                cnt++;
                SetStageButton(cnt, stageList[cnt].Key);
            }
            i++;
        }
    }

    // 스테이지 아이디, 이름, 별 개수, lock 세팅
    private void SetStageButton(int idx, int stageId)
    {
        var stageButton = _stagePool[idx].GetComponent<StageSelectButton>();
        bool isUnlocked = (stageId <= Managers.Data.User.StageLevel) ? true : false;

        string stageName = Managers.Data.Stage.GetByKey(stageId).DisplayName;

        var stageclears = Managers.Data.User.StageClears;
        int star = (stageclears != null && stageclears.ContainsKey(stageId)) ? stageclears[stageId].Star : 0;

        stageButton.SetButton(stageId, isUnlocked, stageName, star);
    }

    public void StageButtonClick(StageSelectButton stageButton)
    {
        if (!stageButton.IsUnlocked) return;

        int key = stageButton.Key;

        Managers.Session.CurrentStageKey = key;
        Managers.Session.StageData = Managers.Data.Stage.GetByKey(key);
        LobbyManager.Instance.GoToSetUpScene();
    }
    
    public void BackButtonClick()
    {
        foreach (GameObject button in _stagePool)
        {
            Destroy(button);
        }

        foreach (GameObject button in _emptyPool)
        {
            Destroy(button);
        }

        _stagePool.Clear();
        _emptyPool.Clear();

        _stagePanel.SetActive(false);

        _chapterPanel.SetActive(true);
        _homeButton.SetActive(true);

        // 타이틀 - '챕터 선택'으로 변경
        _title.text = "챕터 선택";
    }
}
