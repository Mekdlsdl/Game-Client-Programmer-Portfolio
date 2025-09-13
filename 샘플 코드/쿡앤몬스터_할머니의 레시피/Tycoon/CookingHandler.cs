/// <summary>
/// 
/// CookingHandler 스크립트
/// 
/// - Cooking 기능 담당
/// 
/// - 주요 함수:
///     ActivateCookware(): 선택한 레시피 Cookware만 세팅
/// 
///     ActivateCookingPopup() : CookingPopup 활성화 및 Cookware Type 세팅
///     InactivateCookingPopup() : CookingPopup 비활성화
///     ShowRecipe() : Cooking Popup에 레시피 띄우기
///     CookingPopupReset() : CookingPopup 내용 리셋
/// 
///     ActivateCheckPopup() : 요리 확인창 활성화
///     InactivateCheckPopup() : 요리 확인창 비활성화
///     CheckPopupReset() : 요리 확인창 내용 리셋
/// 
///     InactivateErrorPopup() : 에러 창 비활성화
/// 
///     CanCook() : 요리하려고 할 때, 재료 종류와 개수 확인
///     Cooking() : 요리 - 트리거 세팅, 재료 사용, Cookware 세팅
///     FillGauge() : Cookware 게이지 채우기
///     ResetCookware() : Cookware 리셋
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Analytics;

public class CookingHandler : MonoBehaviour
{
    public static CookingHandler Instance { get; private set; }

    [Header("CookingPopup UI")]
    [SerializeField] private Sprite[] _activeBacks; // 등급 포함
    [SerializeField] private Sprite inactiveBack;

    [SerializeField] private GameObject cookingPopup;
    [SerializeField] private Image cookwareIcon;
    [SerializeField] private TMP_Text cookwareType;

    [SerializeField] private List<GameObject> cookwares;
    [SerializeField] private List<GameObject> recipeImages;
    [SerializeField] private List<TMP_Text> recipeNameTexts;
    [SerializeField] private List<TMP_Text> requiredTimesSec;
    [SerializeField] private List<GameObject> requiredTimes;


    [Header("CheckPopup UI")]
    [SerializeField] private Sprite[] _checkRecipeGradeBacks;
    [SerializeField] private GameObject checkPopup;
    [SerializeField] private Image checkCookwareIcon;
    [SerializeField] private Image checkRecipeBack;
    [SerializeField] private Image checkRecipeIcon;
    [SerializeField] private TMP_Text checkRecipeNameText;
    [SerializeField] private List<Image> checkIngreImages;
    [SerializeField] private List<TMP_Text> checkIngreCounts;
    [SerializeField] private TMP_Text checkIngreListText;
    [SerializeField] private GameObject okButton;


    [Header("ErrorPopup UI")]
    [SerializeField] private GameObject errorPopup;


    string[] _gaugeColors = { "#FFA800", "#FF3177" };
    // 정비 턴에서 선택한 레시피
    List<int> selectedRecipes = new();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // TODO : 스테이지, 정비 턴 생기면 수정
        selectedRecipes = Managers.Data.User.SelectRecipes;
        ActivateCookware();
    }

    // 선택한 레시피 Cookware만 세팅
    public void ActivateCookware()
    {
        for (int i = 0; i < selectedRecipes.Count; i++)
        {
            if (selectedRecipes[i] == -1) continue;

            RecipeData _data = Managers.Data.Recipes.GetByKey(selectedRecipes[i]);

            int cookwareId = (int)_data.Cookware;
            cookwares[cookwareId].SetActive(true);
            TriggerManager.Instance.SetIsCooking(_data.Cookware, cookwareId, true);
        }
    }

    // CookingPopup 활성화 및 Cookware Type 세팅
    public void ActivateCookingPopup(DesignEnums.Cookwares triggerType)
    {
        string cookware = "";

        // if (TriggerManager.Instance.GetSnapshot().RecipeId != -1)

        cookingPopup.SetActive(true);

        switch ((int)triggerType)
        {
            case 0:
                cookware = DesignEnums.Cookwares.Pot.ToString();
                cookwareIcon.sprite = Utils.LoadIconSprite(cookware);
                cookwareType.text = "냄비 요리";
                ShowRecipe(cookware);
                break;

            case 1:
                cookware = DesignEnums.Cookwares.Pan.ToString();
                cookwareIcon.sprite = Utils.LoadIconSprite(cookware);
                cookwareType.text = "팬 요리";
                ShowRecipe(cookware);
                break;

            case 2:
                cookware = DesignEnums.Cookwares.Blender.ToString();
                cookwareIcon.sprite = Utils.LoadIconSprite(cookware);
                cookwareType.text = "블랜더 요리";
                ShowRecipe(cookware);
                break;

            default:
                break;
        }
    }

    public void InactivateCookingPopup()
    {
        if (cookingPopup != null && cookingPopup.activeSelf)
        {
            cookingPopup.SetActive(false);
            CookingPopupReset();
        }
    }

    // Cooking Popup에 레시피 띄우기
    public void ShowRecipe(string cookware)
    {
        int idx = 0;

        for (int i = 0; i < selectedRecipes.Count; i++)
        {
            if (selectedRecipes[i] == -1) continue;

            RecipeData _data = Managers.Data.Recipes.GetByKey(selectedRecipes[i]);
            string recipeCookware = _data.Cookware.ToString();

            if (recipeCookware == cookware)
            {
                // 활성화된 배경
                recipeImages[idx].GetComponent<Image>().sprite = _activeBacks[(int)_data.Grade];

                // 레시피 이미지
                Image recipeImage = recipeImages[idx].GetComponent<Transform>().GetChild(0).GetComponent<Image>();
                recipeImage.sprite = Utils.LoadIconSprite(_data.Icon);
                Color color = recipeImage.color;
                color.a = 1f;
                recipeImage.color = color;

                // 레시피 이름
                recipeNameTexts[idx].text = "<" + _data.DisplayName + ">";

                // 소요시간
                requiredTimes[idx].SetActive(true);
                requiredTimesSec[idx].text = _data.CookingTime + "초";

                // 버튼 id
                recipeImages[idx].GetComponent<ButtonInfo>().Id = selectedRecipes[i];

                idx++;
            }
        }

    }

    // CookingPopup 내용 리셋
    public void CookingPopupReset()
    {
        for (int i = 0; i < recipeImages.Count; i++)
        {
            // 활성화된 배경
            recipeImages[i].GetComponent<Image>().sprite = inactiveBack;

            // 조리도구 이미지
            cookwareIcon.sprite = null;

            // 레시피 이미지
            Image recipeImage = recipeImages[i].GetComponent<Transform>().GetChild(0).GetComponent<Image>();
            recipeImage.sprite = null;
            Color color = recipeImage.color;
            color.a = 0f;
            recipeImage.color = color;

            // 레시피 이름
            recipeNameTexts[i].text = "<미선택>";

            // 소요시간
            requiredTimes[i].SetActive(false);

            // 버튼 id 리셋
            recipeImages[i].GetComponent<ButtonInfo>().Id = -1;
        }
    }

    // 요리 확인창 활성화
    public void ActivateCheckPopup(GameObject button)
    {
        // 키보드 입력 막기
        StageManager.Instance.Player.CanKeyboardInput = false;

        int id = button.GetComponent<ButtonInfo>().Id;

        if (id == -1) return;

        InactivateCookingPopup();
        checkPopup.SetActive(true);

        RecipeData _data = Managers.Data.Recipes.GetByKey(id);

        // 아이콘, 요리명, 재료 리스트 이미지, 재료 리스트 개수, 재료 리스트 텍스트 변경
        checkCookwareIcon.sprite = Utils.LoadIconSprite(_data.Cookware.ToString());
        checkRecipeBack.sprite = _checkRecipeGradeBacks[(int)_data.Grade];
        checkRecipeIcon.sprite = Utils.LoadIconSprite(_data.Icon);
        Color recipeColor = checkRecipeIcon.color;
        recipeColor.a = 1f;
        checkRecipeIcon.color = recipeColor;
        checkRecipeNameText.text = "요리 : " + _data.DisplayName;

        List<int> ingredients = _data.Ingredients;
        List<int> counts = _data.Counts;
        string ingreListStr = "필요재료 : ";

        for (int i = 0; i < ingredients.Count; i++)
        {
            IngredientData ingredientData = Managers.Data.Ingredients.GetByKey(ingredients[i]);
            Image checkIngreImage = checkIngreImages[i];
            checkIngreImage.sprite = Utils.LoadIconSprite(ingredientData.Icon);
            Color color = checkIngreImage.color;
            color.a = 1f;
            checkIngreImage.color = color;
            checkIngreCounts[i].text = counts[i].ToString("N0");
            ingreListStr += ingredientData.DisplayName + ", ";
        }

        checkIngreListText.text = ingreListStr.Substring(0, ingreListStr.Length - 2);

        okButton.GetComponent<ButtonInfo>().Id = id;
    }

    // 요리 확인창 비활성화
    public void InactivateCheckPopup()
    {
        // 키보드 입력 다시 풀기
        StageManager.Instance.Player.CanKeyboardInput = false;

        CheckPopupReset();
        checkPopup.SetActive(false);
    }

    // 요리 확인창 내용 리셋
    public void CheckPopupReset()
    {
        checkCookwareIcon.sprite = null;
        checkRecipeBack.sprite = null;
        checkRecipeIcon.sprite = null;
        Color recipeColor = checkRecipeIcon.color;
        recipeColor.a = 0f;
        checkRecipeIcon.color = recipeColor;

        checkRecipeNameText.text = "요리 : ";

        for (int i = 0; i < checkIngreImages.Count; i++)
        {
            Image checkIngreImage = checkIngreImages[i];
            checkIngreImage.sprite = null;
            Color color = checkIngreImage.color;
            color.a = 0f;
            checkIngreImage.color = color;

            checkIngreCounts[i].text = "0";
        }

        checkIngreListText.text = "필요재료 : ";
    }

    // 에러 창 비활성화
    IEnumerator InactivateErrorPopup()
    {
        yield return new WaitForSeconds(1.5f);

        errorPopup.SetActive(false);
        checkPopup.SetActive(false);
    }

    // 요리 가능 여부 확인 (재료 종류와 개수 확인)
    public int CanCookCheck(int recipe)
    {
        // 요리에 필요한 재료 데이터
        RecipeData _data = Managers.Data.Recipes.GetByKey(recipe);
        List<int> ingredients = _data.Ingredients;
        List<int> counts = _data.Counts;

        int totalCanCookCount = -1;

        if (ingredients == null) { return 0;}
        
        
        for (int i = 0; i < ingredients.Count; i++)
        {
            int ingredient = ingredients[i];
            int count = counts[i];

            // Debug.Log($"{ingredient}: {count}");

            int canCookCount = InventoryManager.Instance.IsInInven(ingredient, count);
            // 인벤토리에 있는지 확인
            if (totalCanCookCount == -1) { totalCanCookCount = canCookCount; }
            else { totalCanCookCount = Math.Min(totalCanCookCount, canCookCount); }
        }
        return totalCanCookCount;
    }

    // 요리 가능하면 시작
    public void CanCook(int recipe)
    {
        if (CanCookCheck(recipe) > 0)
        {
            InactivateCheckPopup();
            Cooking(recipe);
        }
        else
        {
            Debug.Log("재료가 부족합니다.");
            errorPopup.SetActive(true);
            StartCoroutine(InactivateErrorPopup());
        }
    }

    // 요리 중 - 트리거 세팅, 재료 사용, Cookware 세팅
    public void Cooking(int recipe)
    {
        RecipeData _data = Managers.Data.Recipes.GetByKey(recipe);

        // PlaceTrigger.TriggerRecipeId 바꿔주기
        TriggerManager.Instance.SetRecipeId(_data.Cookware, (int)_data.Cookware, recipe);

        // 재료 사용 처리
        List<int> ingredients = _data.Ingredients;
        List<int> counts = _data.Counts;
        for (int i = 0; i < ingredients.Count; i++)
        {
            InventoryManager.Instance.UseIngredient(ingredients[i], counts[i]);
        }


        // 요리 중 표시
        cookwares[(int)_data.Cookware].GetComponent<Transform>().GetChild(0).gameObject.SetActive(false);
        cookwares[(int)_data.Cookware].GetComponent<Transform>().GetChild(1).gameObject.SetActive(true);

        // 사운드
        SoundManager.Instance.PlayCookingSound(_data.Sound, _data.CookingTime); // 사운드 받으면 주석 해제

        // 게이지
        GameObject gaugeBar = TriggerManager.Instance.GetSnapshot().GaugeObject;
        gaugeBar.SetActive(true);

        StartCoroutine(FillGauge(_data.Cookware, _data.CookingTime));
        // StartCoroutine(FillGauge(gauge, 25f)); // 테스트용

    }

    // Cookware 게이지 채우기
    private IEnumerator FillGauge(DesignEnums.Cookwares cookware, float cookingTime)
    {
        Image gauge = TriggerManager.Instance.GetCurrent(cookware).GaugeObject.transform.GetChild(1).GetComponent<Image>();
        float time = 0f;

        while (time < cookingTime)
        {
            time += Time.deltaTime;
            gauge.fillAmount = Mathf.Clamp01(time / cookingTime);
            yield return null;
        }

        gauge.fillAmount = 1f;

        Color gaugeCol;
        ColorUtility.TryParseHtmlString(_gaugeColors[1], out gaugeCol);
        gauge.color = gaugeCol;

        TriggerManager.Instance.SetGaugeRatio(cookware, (int)cookware, 1f);
    }

    // Cookware 리셋
    public void ResetCookware(int recipeId)
    {
        DesignEnums.Cookwares cookware = Managers.Data.Recipes.GetByKey(recipeId).Cookware;
        int cookwareId = (int)cookware;

        // 요리 중 표시 없애기 (ex. 애니메이션)
        cookwares[cookwareId].GetComponent<Transform>().GetChild(0).gameObject.SetActive(true);
        cookwares[cookwareId].GetComponent<Transform>().GetChild(1).gameObject.SetActive(false);

        // 게이지 0f로 만들고 비활성화
        GameObject gauge = TriggerManager.Instance.GetCurrent(cookware).GaugeObject;
        gauge.transform.GetChild(1).GetComponent<Image>().fillAmount = 0f;

        Color gaugeCol;
        ColorUtility.TryParseHtmlString(_gaugeColors[0], out gaugeCol);
        gauge.transform.GetChild(1).GetComponent<Image>().color = gaugeCol;

        gauge.SetActive(false);
        TriggerManager.Instance.SetGaugeRatio(cookware, (int)cookware, 0f);

        // PlaceTrigger에서 TriggerRecipeId 원래대로 하기
        TriggerManager.Instance.SetRecipeId(cookware, cookwareId, -1);
    }
}
