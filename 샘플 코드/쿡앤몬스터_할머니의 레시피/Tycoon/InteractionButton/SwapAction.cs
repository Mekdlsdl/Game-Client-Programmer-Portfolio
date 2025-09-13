using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapAction : UIInteractActionBase
{
    private SpriteRenderer _plate;
    /*
    
        1) 제작대 음식 스왑 - SwapAction
                - 제작대에 음식 없을 경우
                    - 손에 음식이 있으면 그냥 놓기
                    - 손에 음식이 없으면 아무일도 없음

                - 제작대에 음식 있을 경우
                    - 손에 음식이 있으면 스왑
                    - 손에 음식이 없으면 들기

    */
    public override bool CanExecute(SpriteRenderer plate)
    {
        _plate = plate;

        bool check = true;
        var snapshot = TriggerManager.Instance.GetSnapshot();

        // Cookware가 활성화 되어있는지 체크
        if (snapshot.IsCooking) return false;

        // 카운터나 쓰레기통인지 체크
        if (snapshot.Focused.Type == TriggerType.Counter || snapshot.Focused.Type == TriggerType.Bin) return false;

        // if (snapshot.PlacedPlate == null) return false;

        return check;
    }

    public override void Execute()
    {
        Debug.Log("SwapAction");

        var placeSnapshot = TriggerManager.Instance.GetSnapshot();
        SpriteRenderer placedPlate = placeSnapshot.PlacedPlate;
        int holdingPlateId = StageManager.Instance.TycoonInfo.HoldingPlateId;

        // if (placedPlate == null) return;

        // 제작대에 음식 없을 경우
        if (placedPlate.sprite == null)
        {
            // 손에 음식이 있을 경우
            if (_plate.sprite != null)
            {
                placedPlate.sprite = _plate.sprite;
                _plate.sprite = null;

                TriggerManager.Instance.SetRecipeId(placeSnapshot.Focused.Type, placeSnapshot.Focused.PlaceIndex, holdingPlateId);
                StageManager.Instance.TycoonInfo.HoldingPlateId = -1;
            }
        }
        // 제작대에 음식 있을 경우
        else
        {
            // 손에 음식이 있을 경우
            if (_plate.sprite != null)
            {
                Sprite tempSprite = placedPlate.sprite;

                placedPlate.sprite = _plate.sprite;
                _plate.sprite = tempSprite;

                int tempInt = placeSnapshot.RecipeId;
                TriggerManager.Instance.SetRecipeId(placeSnapshot.Focused.Type, placeSnapshot.Focused.PlaceIndex, holdingPlateId);
                StageManager.Instance.TycoonInfo.HoldingPlateId = tempInt;

            }
            // 손에 음식이 없을 경우
            else
            {
                _plate.sprite = placedPlate.sprite;
                placedPlate.sprite = null;

                TriggerManager.Instance.SetRecipeId(placeSnapshot.Focused.Type, placeSnapshot.Focused.PlaceIndex, -1);
                StageManager.Instance.TycoonInfo.HoldingPlateId = placeSnapshot.RecipeId;
            }
        }
    }
}
