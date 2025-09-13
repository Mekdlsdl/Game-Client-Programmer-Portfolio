using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class InteractionButtonRouter : MonoBehaviour
{
    public SpriteRenderer plate;

    [SerializeField] private UnityEngine.UI.Button button;
    [SerializeField] private MonoBehaviour[] actionComponents;
    private IInteractAction[] actions;



    // TODO : 제작대에 음식 올리기
    //        음식 손에 들기
    //        음식 픽업


    /*
        MEMO : 상호작용 키 사용할 액션 목록
            
            1) 제작대 음식 스왑 - SwapAction
                - 제작대에 음식 없을 경우
                    - 손에 음식이 있으면 그냥 놓기
                    - 손에 음식이 없으면 아무일도 없음

                - 제작대에 음식 있을 경우
                    - 손에 음식이 있으면 스왑
                    - 손에 음식이 없으면 들기


            2) 완성된 음식 픽업 - CookAction
                - 음식 들고 있을 때는 불가
                - 조리 중일 때도 불가


            3) 손님에게 서빙 (주문처리) - ServeAction (X - 닿기만 해도 서빙으로 수정)
                -> 보류, 혹시나 음식을 앞에 둬야 할수도 있으니 일단 두기
                - 음식 들고 있는 경우만
                    - 맞는 음식일 때는 서빙
                    - 틀린 음식일 때는 .. (들고 있는 음식 위에 X 표시..? 보류)


            4) 손님 주문 레시피 출력 - OrderAction
                - 음식 안 들고 있을 때만
                - 손님이 있을 때만 (근데 어차피 계속 채워짐)
    */
    void Awake()
    {
        actions = actionComponents.OfType<IInteractAction>().ToArray();

        button.onClick.AddListener(OnPressed);
    }

    private void OnPressed()
    {
        // Debug.Log("버튼 클릭");

        var best = actions
            .Where(a => a.CanExecute(plate))
            .FirstOrDefault();

        if (best != null)
            best.Execute();
        else
            Debug.Log("가능한 행동 없음");
    }

}
