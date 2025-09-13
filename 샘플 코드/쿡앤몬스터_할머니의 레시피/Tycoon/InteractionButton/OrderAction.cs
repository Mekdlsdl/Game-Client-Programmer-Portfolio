using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderAction : UIInteractActionBase
{
    private SpriteRenderer _plate;

    /*
    
        4) 손님 주문 레시피 출력 - OrderAction
                - 음식 안 들고 있을 때만
                - 손님이 있을 때만 (근데 어차피 계속 채워짐)
    
    */
    public override bool CanExecute(SpriteRenderer plate)
    {
        _plate = plate;

        bool check = false;

        // 음식 들고 있으면 false return
        if (_plate.sprite != null) return false;

        // 플레이어가 카운터 트리거 안에 있는지 체크
        if (TriggerManager.Instance.GetSnapshot().Focused.Type == TriggerType.Counter) check = true;

        return check;
    }

    public override void Execute()
    {
        Debug.Log("OrderAction");

        PlaceTrigger placeTrigger = TriggerManager.Instance.GetCurrent(TriggerType.Counter);
        CounterHandler.Instance.ActivateRecipePopup(placeTrigger.TriggerRecipeId);
    }
}
