using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* 메뉴판 (메뉴 선택) */
public class MenuTabController : MonoBehaviour
{
    [SerializeField] private Sprite _menuEmptySprite;
    [SerializeField] private Sprite[] _recipeGradeSprites;


    [Header("Menu Tab")]
    [SerializeField] private List<GameObject> _menuButtons;
    [SerializeField] private List<Image> _menuIconImages;
    [SerializeField] private List<GameObject> _menuNameText;


    [Header("Select Recipe Panel")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private GameObject _selectRecipePanel;


    private List<int> _selectedRecipes;

    private List<GameObject> _pool;
    private HashSet<int> _unlockRecipes; // 해금된 레시피 정보
    private List<RecipeData> _totalRecipeDataList; // 모든 레시피 데이터
    private List<RecipeData> _unlockRecipeDataList; // 해금된 레시피 데이터

    private int _currentMenuIdx = -1; // 현재 변경하려는 메뉴 인덱스

    private int _selectMarkchild = -1; // 선택 마크 자식 위치
    private int _currentSelectIdx = -1; // 현재 선택된 위치


    private void Start()
    {
        _pool = new List<GameObject>();
        _totalRecipeDataList = new List<RecipeData>(Managers.Data.Recipes.ItemsList)
            .Where(r => r.availableChapter <= Managers.Session.StageData.Chapter) // 해금 가능한 레시피만 불러오기
            .ToList();

        SetSelectedRecipes();

        // Pool은 레시피 총 개수만큼 만들어두기
        EnsurePoolSize(_totalRecipeDataList.Count);
    }

    // 선택된 레시피 세팅
    public void SetSelectedRecipes()
    {
        _selectedRecipes = new List<int>(Managers.Data.User.SelectRecipes); // 직접 바꾸면 안되니까 복사해오기
        Debug.Log($"현재 선택된 레시피 : [{_selectedRecipes[0]}, {_selectedRecipes[1]}, {_selectedRecipes[2]}]");

        bool hasExcluded = false;
        int refund = 0; // 환급금

        for (int i = 0; i < _menuIconImages.Count; i++)
        {
            int id = _selectedRecipes[i];

            if (id == -1) continue;

            if (SetUpController.Instance.ExcludeNotOpenedRecipe(i, id))
            {
                hasExcluded = true;
                refund += Managers.Data.Recipes.GetByKey(id).Cost / 2;
                
                if (Managers.Data.User.TryChangeMenu(i, -1))
                
                continue;
            }

            ButtonSetting(i, id);
        }

        if (!Managers.Data.User.HadReceivedRefund[1] && hasExcluded)
        {
            // LobbyManager.Instance.ActivateErrorPopup("아직 열리지 않은 레시피를\n메뉴판에서 발견했어요.");
            string message = "아직 열리지 않은 레시피를\n메뉴판에서 발견했어요.";
            StartCoroutine(SetUpController.Instance.PayRefund(1, refund, message));
            return;
        }
    }

    public void ButtonSetting(int idx, int id)
    {
        if (id == -1)
        {
            // 버튼 아이디 초기화
            _menuButtons[idx].GetComponent<ButtonInfo>().Id = -1;

            // 배경 초기화
            _menuButtons[idx].GetComponent<Image>().sprite = _menuEmptySprite;

            // 아이콘 초기화
            Color menuIconColor = _menuIconImages[idx].color;
            menuIconColor.a = 0f;
            _menuIconImages[idx].color = menuIconColor;

            _menuIconImages[idx].sprite = null;

            // 요리명 세팅
            _menuNameText[idx].transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            _menuNameText[idx].SetActive(false);

            return;
        }

        var data = Managers.Data.Recipes.GetByKey(id);

        // 버튼에 아이디 세팅
        _menuButtons[idx].GetComponent<ButtonInfo>().Id = id;

        // 배경(등급) 세팅
        _menuButtons[idx].GetComponent<Image>().sprite = _recipeGradeSprites[(int)data.Grade];

        // 아이콘 세팅
        Color menuIconCol = _menuIconImages[idx].color;
        menuIconCol.a = 1f;
        _menuIconImages[idx].color = menuIconCol;

        _menuIconImages[idx].sprite = Utils.LoadIconSprite(data.Icon);

        // 요리명 세팅
        _menuNameText[idx].SetActive(true);

        _menuNameText[idx].transform.GetChild(0).GetComponent<TMP_Text>().text = data.DisplayName;
    }

    // 팝업 활성화
    public void ActivateSelectRecipePanel(GameObject button)
    {
        _currentMenuIdx = button.transform.GetSiblingIndex();

        int id = button.GetComponent<ButtonInfo>().Id;

        SetData(id);

        // 현재 누른 레시피 선택된 상태로 출력
        for (int i = 0; i < _content.childCount; i++)
        {
            GameObject cur = _content.GetChild(i).gameObject;
            if (cur.activeSelf && cur.GetComponent<ButtonInfo>().Id == id)
            {
                ChangeSelectMarkLoc(cur);
                break;
            }
        }

        _selectRecipePanel.SetActive(true);
    }

    // 팝업 비활성화 
    public void InactivatedSelectedRecipePanel()
    {
        ResetSelectedRecipePanel();

        _selectRecipePanel.SetActive(false);
    }

    // 닫기 버튼 눌렀을 때
    public void ClosedSelectedRecipePanel()
    {
        // 원래대로 돌리기
        _selectedRecipes[_currentMenuIdx] = Managers.Data.User.SelectRecipes[_currentMenuIdx];
        InactivatedSelectedRecipePanel();
    }

    public void ResetSelectedRecipePanel()
    {
        if (_currentSelectIdx != -1)
        {
            _content.transform.GetChild(_currentSelectIdx).GetChild(_selectMarkchild).gameObject.SetActive(false);
            _currentSelectIdx = -1;
        }
    }

    private void SetData(int id)
    {
        // 초기값 세팅
        _unlockRecipes = new HashSet<int>(Managers.Data.User.UnlockRecipes);

        // 팝업 열린 레시피는 일단 초기화
        _selectedRecipes[_currentMenuIdx] = -1;

        // 선택된 레시피는 제외하고 리스트 출력 (방금 누른건 포함)
        _unlockRecipes.ExceptWith(_selectedRecipes);

        _unlockRecipeDataList = _totalRecipeDataList
            .Where(r => _unlockRecipes.Contains(r.key)) // 해금된 레시피만 불러오기
            .OrderBy(r => r.Price) // 판매 가격순으로 정렬
            .ToList();

        // 해금된 레시피만 출력
        Populate(_unlockRecipeDataList, 0);
    }

    // 풀 생성
    private void EnsurePoolSize(int target)
    {
        while (_pool.Count < target)
        {
            var view = Instantiate(_itemPrefab, _content);
            view.gameObject.SetActive(false);
            _pool.Add(view);
        }
    }

    // UI Set
    public void Populate(IReadOnlyList<RecipeData> recipes, int startIdx)
    {
        int endIdx = recipes.Count + startIdx;

        // 필요한 개수만 활성화 / 데이터 바인딩
        //  - 추후에 레벨에 따라 다르게 보여줄 수도 있으니..
        //  - 일단은 레시피 수만큼 보이게 해뒀습니당
        for (int i = startIdx; i < endIdx; i++)
        {
            var view = _pool[i].GetComponent<RecipeItemView>();
            if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
            view.Bind(recipes[i - startIdx], true);
        }

        // 남는 Pool 아이템 비활성화
        for (int i = endIdx; i < _pool.Count; i++)
        {
            if (_pool[i].gameObject.activeSelf) _pool[i].gameObject.SetActive(false);
        }

        // Grid / Layout 강제 갱신 필요할 경우 주석 해제
        // LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
    }

    // 커서 옮기기
    public void ChangeSelectMarkLoc(GameObject button)
    {
        _selectMarkchild = button.transform.childCount - 1;

        // 이전 커서 지우기
        if (_currentSelectIdx != -1)
        {
            _content.transform.GetChild(_currentSelectIdx).GetChild(_selectMarkchild).gameObject.SetActive(false);
        }

        // 현재 커서 갱신
        _currentSelectIdx = button.transform.GetSiblingIndex();
        Debug.Log($"현재 커서 : {_currentSelectIdx}");

        button.transform.GetChild(_selectMarkchild).gameObject.SetActive(true);
    }

    // 변경 버튼 눌렀을 때
    public void ApplySelectedRecipe()
    {
        int id = _content.transform.GetChild(_currentSelectIdx).GetComponent<ButtonInfo>().Id;

        // 적용할 수 없을 경우
        if (!CanApply(id)) return;


        if (Managers.Data.User.TryChangeMenu(_currentMenuIdx, id))


        // 메뉴판 버튼 세팅
        ButtonSetting(_currentMenuIdx, id);

        InactivatedSelectedRecipePanel();
    }

    // 메뉴 고르고 '변경' 눌렀을 때 저장 가능한지 판별
    public bool CanApply(int id, int idx = -1)
    {
        // SetUpController에서 접근했을 때
        if (idx != -1) _currentMenuIdx = idx;


        // 누른 레시피는 이미 제외하고 들어있음
        Debug.Log($"[{_selectedRecipes[0]}, {_selectedRecipes[1]}, {_selectedRecipes[2]}] 제외했는지 확인");

        // 변경하려는 레시피 반영하고 체크해보기
        _selectedRecipes[_currentMenuIdx] = id;

        Debug.Log($"[{_selectedRecipes[0]}, {_selectedRecipes[1]}, {_selectedRecipes[2]}] 가능한지 확인");


        // 등급별 초기 세팅
        int[] gradeCount = { 0, 0, 0 };
        // int grade = -1;

        for (int i = 0; i < _selectedRecipes.Count; i++)
        {
            int selectedId = _selectedRecipes[i];
            Debug.Log($"{i}번 selectedId = {selectedId}");

            // 선택하지 않았으면 패스
            if (selectedId == -1) continue;

            int grade = (int)Managers.Data.Recipes.GetByKey(selectedId).Grade;
            gradeCount[grade]++;
            Debug.Log($"{i}번째 메뉴 등급은 {grade}");
        }


        Debug.Log($"등급 개수 : [{gradeCount[0]}, {gradeCount[1]}, {gradeCount[2]}]");
        string errorMessage;

        // 1) 커먼 등급 레시피는 하나 이상 들어가야 함
        if ((gradeCount[1] > 0 || gradeCount[2] > 0) && gradeCount[0] == 0)
        {
            // 에러 팝업 띄우기
            errorMessage = "안 돼요!\n커먼 등급의 메뉴는 반드시 1개 이상 선택해야 합니다.";
            LobbyManager.Instance.ActivateErrorPopup(errorMessage);

            // 다시 지우기
            _selectedRecipes[_currentMenuIdx] = -1;

            return false;
        }

        // 2) 레전드 등급 레시피는 하나만 등록 가능함
        if (gradeCount[2] > 1)
        {
            // 에러 팝업 띄우기
            errorMessage = "안 돼요!\n레전드 등급의 메뉴는 1개까지만 선택 가능합니다.";
            LobbyManager.Instance.ActivateErrorPopup(errorMessage);

            // 다시 지우기
            _selectedRecipes[_currentMenuIdx] = -1;

            return false;
        }

        // +) 아무것도 선택하지 않았을 경우
        if (gradeCount[0] == 0 && gradeCount[1] == 0 && gradeCount[2] == 0)
        {
            // 에러 팝업 띄우기
            errorMessage = "안 돼요!\n메뉴는 하나 이상 선택해야 합니다.";
            LobbyManager.Instance.ActivateErrorPopup(errorMessage);

            // 다시 지우기
            _selectedRecipes[_currentMenuIdx] = -1;

            return false;
        }

        if (Managers.Session.CurrentStageKey == 5000 && gradeCount[0] + gradeCount[1] + gradeCount[2] < 3)
        {
            // 에러 팝업 띄우기
            errorMessage = "안 돼요!\n메뉴 변경은 튜토리얼 이후에 가능합니다.";
            LobbyManager.Instance.ActivateErrorPopup(errorMessage);

            // 다시 지우기
            _selectedRecipes[_currentMenuIdx] = -1;

            return false;
        }

        // 변경 가능하면 안 지우기
        return true;
    }
}
