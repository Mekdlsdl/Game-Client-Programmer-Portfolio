using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookAction : UIInteractActionBase
{
    private SpriteRenderer _plate;
    [SerializeField] private GameObject Gauge;

    /*
    
        2) 완성된 음식 픽업 - CookAction
            - 가능 조건
                + 게이지가 활성화되어있을 때
                + 게이지가 다 찼을 때
                + 음식 안 들고 있을 때
                + 트리거와 닿아 있을 때

            - 불가능 조건
                - 음식 들고 있을 때는 불가
                - 조리 중일 때도 불가
    
    */


    public override bool CanExecute(SpriteRenderer plate)
    {
        bool check = false;

        // 요리 트리거 위에 있을 때
        if (TriggerManager.Instance.GetSnapshot().Focused?.Type == TriggerType.Cookware) check = true;

        _plate = plate;
        
        // 음식 들고 있으면 false
        if (_plate.sprite != null)
        {
            Debug.Log("CookAction - 음식을 이미 들고 있음");
            return false;
        }

        Gauge = TriggerManager.Instance.CurrentState.GaugeObject;

        // 게이지가 없는 트리거라면 false
        if (Gauge == null)
        {
            Debug.Log("CookAction - 게이지 없는 트리거임");
            return false;
        }

        // Cookware에 게이지 활성화 안 되어있으면(요리 중 아니면) false
        // Image gaugeBar = Gauge.transform.GetChild(0).GetChild(1).GetComponent<Image>();
        float gaugeBar = TriggerManager.Instance.GetSnapshot().GaugeRatio;
        if (!Gauge.activeSelf || gaugeBar != 1f)
        {
            Debug.Log("CookAction - 게이지가 활성화 되어있지 않거나 게이지가 꽉 차지 않음");
            return false;
        }

        return check;
    }

    public override void Execute()
    {
        Debug.Log("CookAction");

        var placeSnapshot = TriggerManager.Instance.GetSnapshot();
        int recipeId = placeSnapshot.RecipeId;
        string icon = Managers.Data.Recipes.GetByKey(recipeId).Icon;

        // 음식 손에 들기
        _plate.GetComponent<SpriteRenderer>().sprite = Utils.LoadIconSprite(icon);
        StageManager.Instance.TycoonInfo.HoldingPlateId = recipeId;
        TriggerManager.Instance.SetRecipeId(placeSnapshot.Focused.Type, placeSnapshot.Focused.PlaceIndex, -1);

        // Cookware 리셋
        CookingHandler.Instance.ResetCookware(recipeId);
    }
}
