using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* 레시피 목록 + 구매 */
public class RecipeTabController : MonoBehaviour
{
    [Header("Hierarchy")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _itemPrefab;

    [Header("Recipe Info Panel")]
    [SerializeField] private GameObject _recipeInfoPanel;
    [SerializeField] private Image _recipeInfoBackImage;
    [SerializeField] private Image _recipeIconImage;
    [SerializeField] private Image _cookwareNameTag;
    [SerializeField] private Image _gradeNameTag;
    [SerializeField] private TMP_Text _recipeNameText;
    [SerializeField] private TMP_Text _recipePriceText;
    [SerializeField] private List<GameObject> _ingreObjects;
    [SerializeField] private List<TMP_Text> _ingreCounts;
    [SerializeField] private GameObject _buyRecipeButton;
    [SerializeField] private TMP_Text _recipeCostText;

    [Header("Default Sprites")]
    [SerializeField] private Sprite[] _recipeGradeBacks;
    [SerializeField] private Sprite[] _ingreGradeBacks;
    [SerializeField] private Sprite[] _cookwareNameTagSprites;
    [SerializeField] private Sprite[] _gradeNameTagSprites;


    private int MAXINGRECNT = 3;

    // =================== 레시피 풀 관련 ===================
    private List<GameObject> _pool;

    private HashSet<int> _unlockRecipes; // 해금된 레시피 정보

    private List<RecipeData> _totalRecipeDataList; // 모든 레시피 데이터
    private List<RecipeData> _unlockRecipeDataList; // 해금된 레시피 데이터
    private List<RecipeData> _lockRecipeDataList; // 해금되지 않은 레시피 데이터

    private readonly Dictionary<int, RecipeData> _recipeDataDict; // id로 찾아야 할때 활용



    // 레시피 목록 로드
    void Start()
    {
        _pool = new List<GameObject>();

        // 초기값 세팅
        _unlockRecipes = new HashSet<int>(Managers.Data.User.UnlockRecipes);
        _totalRecipeDataList = new List<RecipeData>(Managers.Data.Recipes.ItemsList);

        // Pool은 레시피 총 개수만큼 만들어두기
        EnsurePoolSize(_totalRecipeDataList.Count);

        SetData();
    }

    private void SetData()
    {
        _totalRecipeDataList = _totalRecipeDataList
            .Where(r => r.availableChapter <= Managers.Session.StageData.Chapter) // 해금 가능한 레시피만 불러오기
            .ToList();

        _unlockRecipeDataList = _totalRecipeDataList
            .Where(r => _unlockRecipes.Contains(r.key)) // 해금된 레시피만 불러오기
            .OrderBy(r => r.Price) // 판매 가격순으로 정렬
            .ToList();

        _lockRecipeDataList = _totalRecipeDataList
            .Where(r => !_unlockRecipes.Contains(r.key)) // 해금되지 않은 레시피만 불러오기
            .OrderBy(r => r.Cost) // 레시피 구매 가격순으로 정렬
            .ToList();

        // 해금된 레시피 -> 잠긴 레시피 순으로 출력
        Populate(_unlockRecipeDataList, 0, true);
        Populate(_lockRecipeDataList, _unlockRecipes.Count, false);
    }

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
    public void Populate(IReadOnlyList<RecipeData> recipes, int startIdx, bool isUnlocked)
    {
        int endIdx = recipes.Count + startIdx;

        // 필요한 개수만 활성화 / 데이터 바인딩
        //  - 추후에 레벨에 따라 다르게 보여줄 수도 있으니..
        //  - 일단은 레시피 수만큼 보이게 해뒀습니당
        for (int i = startIdx; i < endIdx; i++)
        {
            var view = _pool[i].GetComponent<RecipeItemView>();
            if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
            view.Bind(recipes[i - startIdx], isUnlocked);
        }

        // 남는 Pool 아이템 비활성화
        for (int i = endIdx; i < _pool.Count; i++)
        {
            if (_pool[i].gameObject.activeSelf) _pool[i].gameObject.SetActive(false);
        }

        // Grid / Layout 강제 갱신 필요할 경우 주석 해제
        // LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
    }

    // 요리 설명창 띄우기
    public void ActivateRecipeInfoPanel(GameObject button)
    {
        int recipeId = button.GetComponent<ButtonInfo>().Id;
        int recipedCnt = button.GetComponent<ButtonInfo>().Count;

        // 요리 설명창 세팅
        SetRecipeInfoPanel(recipeId, recipedCnt);

        // 패널들 켜기
        SetUpController.Instance.GuardPanel.SetActive(true); // 디폴트씬 Top Button 눌림 방지
        _recipeInfoPanel.SetActive(true);
    }

    public void InactivateRecipeInfoPanel()
    {
        if (!_recipeInfoPanel.activeSelf) return;

        ResetRecipeInfoPanel();

        _recipeInfoPanel.SetActive(false);
        SetUpController.Instance.GuardPanel.SetActive(false);
    }

    public void SetRecipeInfoPanel(int id, int cnt)
    {
        bool isUnlocked = cnt == 1 ? true : false;
        var data = Managers.Data.Recipes.GetByKey(id);

        // 패널 배경 세팅 (레시피 등급)
        _recipeInfoBackImage.sprite = _recipeGradeBacks[(int)data.Grade];

        // 아이콘 세팅
        _recipeIconImage.sprite = Utils.LoadIconSprite(data.Icon);

        // 네임태그 세팅
        _cookwareNameTag.sprite = _cookwareNameTagSprites[(int)data.Cookware];
        _gradeNameTag.sprite = _gradeNameTagSprites[(int)data.Grade];

        // 레시피 이름, 가격 세팅
        _recipeNameText.text = data.DisplayName;
        _recipePriceText.text = data.Price.ToString("N0");

        // 재료 이미지, 개수 세팅
        List<int> ingredients = data.Ingredients
        .OrderBy(x => (int)Managers.Data.Ingredients.GetByKey(x).Grade)
        .ThenBy(x => x).ToList();

        for (int i = 0; i < ingredients.Count; i++)
        {
            _ingreObjects[i].SetActive(true); // 재료 수 만큼 켜기

            var ingreData = Managers.Data.Ingredients.GetByKey(ingredients[i]);

            // 재료 배경 (등급)
            _ingreObjects[i].GetComponent<Image>().sprite = _ingreGradeBacks[(int)ingreData.Grade];

            // 재료 아이콘, 개수
            _ingreObjects[i].transform.GetChild(0).GetComponent<Image>().sprite = Utils.LoadIconSprite(ingreData.Icon);
            _ingreCounts[i].text = data.Counts[i].ToString("N0");
        }

        // 레시피 구매 버튼 세팅
        if (!isUnlocked)
        {
            _recipeCostText.text = data.Cost.ToString("N0");
            _buyRecipeButton.GetComponent<ButtonInfo>().Id = id;
            _buyRecipeButton.SetActive(true);
        }
    }

    public void ResetRecipeInfoPanel()
    {
        // 재료 이미지, 개수 초기화
        for (int i = 0; i < MAXINGRECNT; i++) // 재료 최대 개수 = 3
        {
            // 재료 배경 (등급)
            _ingreObjects[i].GetComponent<Image>().sprite = null;

            // 재료 아이콘, 개수
            _ingreObjects[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
            _ingreCounts[i].text = "";

            _ingreObjects[i].SetActive(false); // 재료 모두 꺼놓기
        }

        // 레시피 구매 버튼 세팅
        if (_buyRecipeButton.activeSelf)
        {
            _recipeCostText.text = "0";
            _buyRecipeButton.SetActive(false);
        }
    }


    // 레시피 구매
    public void BuyRecipe(GameObject button)
    {
        int id = button.GetComponent<ButtonInfo>().Id;
        int cost = Managers.Data.Recipes.GetByKey(id).Cost;

        if (Managers.Data.User.TryConsumeCoin(cost))
        {
            Managers.Data.User.TryAddRecipe(id);

            _unlockRecipes.Add(id);
            SetData();
            LayoutRebuilder.MarkLayoutForRebuild(_content);

            button.SetActive(false);
        }
        else
        {
            Debug.Log("소지금이 부족합니다.");
        }
    }
}
