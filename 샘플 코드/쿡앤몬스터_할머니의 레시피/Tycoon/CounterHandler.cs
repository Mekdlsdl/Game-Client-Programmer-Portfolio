using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CounterHandler : MonoBehaviour
{
    public static CounterHandler Instance { get; private set; }

    public GameObject player;
    public TMP_Text coin;

    [SerializeField] private GameObject _panel;


    [Header("RecipePopup UI")]
    [SerializeField] private Sprite[] _recipeBackSprites;
    [SerializeField] private Sprite[] _recipeGradeSprites;
    [SerializeField] private Sprite[] _recipeCookwareSprites;
    [SerializeField] private GameObject _recipePopup;
    [SerializeField] private Image _cookwareIcon;
    [SerializeField] private Image _gradeIcon;
    [SerializeField] private Image _recipeBack;
    [SerializeField] private Image _recipeIcon;
    [SerializeField] private TMP_Text _recipeNameText;
    [SerializeField] private TMP_Text _coinText;


    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

    }

    public void Selling(int recipeId, bool critical = false)
    {
        int price = Managers.Data.Recipes.GetByKey(recipeId).Price;

        price = critical ? (int)(price * 1.5f) : price;

        UIManager.Instance.ShowFloatingText($"+ {price:N0}", player.transform.position);
        coin.text = (StageManager.Instance.CurMoney + price).ToString("N0");
        StageManager.Instance.AddMoney(price);
    }


    public void CanServing(PlaceTrigger place)
    {
        Debug.Log($"Type - {place.Type} / Index - {place.PlaceIndex}");
        SpriteRenderer plate = StageManager.Instance.TycoonInfo.HoldingPlateObject;

        bool check = false;

        // 음식 안 들고 있으면 false return
        if (plate.sprite == null)
        {
            Debug.Log("CanServing - 음식 안 들고 있음");
            return;
        }

        // 손님이 주문한 음식과 들고 있는 음식이 동일한지 체크
        int holdingPlateId = StageManager.Instance.TycoonInfo.HoldingPlateId;
        int orderRecipeId = place.TriggerRecipeId;
        Debug.Log($"ServeAction - holding = {holdingPlateId}, order = {orderRecipeId}");

        if (holdingPlateId == orderRecipeId) check = true;
        if (holdingPlateId != orderRecipeId)
        {
            Debug.Log("CanServing - 주문한 음식이 아님");
            return;
        }


        if (check) Serving(place);
    }

    public void Serving(PlaceTrigger place)
    {
        Debug.Log("ServeAction - Button(x)");

        /* 
            카운터의 경우
                PlaceTrigger의 triggerRecipeId - 손님이 주문한 음식
                TriggerManager의 PlacedPlate - 카운터에 놓은 음식

            => 카운터에 음식 놓는다고 해서 RecipeId 바꾸지 않기
        */

        SpriteRenderer plate = StageManager.Instance.TycoonInfo.HoldingPlateObject;

        SpriteRenderer placedPlate = place.PlacedPlate;
        int holdingPlateId = StageManager.Instance.TycoonInfo.HoldingPlateId;

        // 카운터에 음식 놓기
        placedPlate.sprite = plate.sprite;
        plate.sprite = null;

        StageManager.Instance.TycoonInfo.HoldingPlateId = -1;

        // 손님이 받아가기
        int laneIndex = place.PlaceIndex;
        Debug.Log($"{laneIndex}번 손님");
        CustomerSpawner.Instance.ServeLane(laneIndex, place);
    }

    // 레시피 창 활성화
    public void ActivateRecipePopup(int recipeId)
    {
        if (recipeId == -1) return;

        RecipeData _data = Managers.Data.Recipes.GetByKey(recipeId);

        // 아이콘(레시피,요리도구), 요리명, 재료 리스트 이미지, 재료 리스트 개수, 재료 리스트 텍스트 변경
        _cookwareIcon.sprite = _recipeCookwareSprites[(int)_data.Cookware];
        _recipeBack.sprite = _recipeBackSprites[(int)_data.Grade];
        _recipeIcon.sprite = Utils.LoadIconSprite(_data.Icon);

        _gradeIcon.sprite = _recipeGradeSprites[(int)_data.Grade];

        Color recipeColor = _recipeIcon.color;
        recipeColor.a = 1f;
        _recipeIcon.color = recipeColor;
        _recipeNameText.text = _data.DisplayName;

        _coinText.text = _data.Price.ToString("N0");

        _recipePopup.SetActive(true);
        _panel.SetActive(true);
    }

    // 레시피 창 비활성화
    public void InactivateRecipePopup()
    {
        if (_recipePopup.activeSelf)
        {
            RecipePopupReset();
            _recipePopup.SetActive(false);
            _panel.SetActive(false);
        }
    }

    // 레시피 창 내용 리셋
    public void RecipePopupReset()
    {
        _cookwareIcon.sprite = null;
        _recipeIcon.sprite = null;

        _gradeIcon.sprite = null;

        Color recipeColor = _recipeIcon.color;
        recipeColor.a = 0f;
        _recipeIcon.color = recipeColor;

        _recipeNameText.text = "";

        _coinText.text = "0";
    }
}
