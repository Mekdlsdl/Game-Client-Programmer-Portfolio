/// <summary>
/// 
/// Customer 스크립트
/// 
/// - 손님 루틴 관리 담당
/// 
/// - 주요 함수:
///     
/// 
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    public event Action<Customer, int> OnArrivedAtCounter;
    public event Action<Customer, int> OnLeftLane;

    [SerializeField] private Animator _animator;
    // [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private AnimatorOverrideController[] _customerControllers;
    [SerializeField] private GameObject _bubble;
    [SerializeField] private GameObject _criticalEffect;
    [SerializeField] private TMP_Text _priceText;


    int _laneIndex = -1;

    Vector3 _start, _door, _counter, _exit;
    float _timeIn, _timeWait, _timeOut;
    bool _served;
    bool _didCritical;
    Coroutine _co;


    public void Begin(int laneIndex, Vector3 start, Vector3 door, Vector3 counter, Vector3 exit, float timeIn, float timeWait, float timeOut)
    {
        _laneIndex = laneIndex;
        _start = start; _door = door; _counter = counter; _exit = exit;
        _timeIn = timeIn; _timeOut = timeOut; _timeWait = timeWait;
        _served = false;

        gameObject.SetActive(true);

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        // 1. 입장
        transform.position = _start;
        _animator.Play("Back_Move");
        yield return MoveTo(_door, _timeIn);

        // 2. 카운터 도착 + 대기(주문..?)
        yield return MoveTo(_counter, _timeWait);
        _animator.SetFloat("IdleSpeed", UnityEngine.Random.Range(0.5f, 1.2f));
        _animator.Play("Idle");
        OnArrivedAtCounter?.Invoke(this, _laneIndex);


        // 3. 음식 대기
        while (!_served) yield return null;


        // 4. 퇴장
        _animator.Play("Move");
        yield return MoveTo(_door, _timeWait);
        yield return MoveTo(_exit, _timeOut);


        // 5. 레인 비움
        OnLeftLane?.Invoke(this, _laneIndex);
    }


    // 목표까지 t 속도로 이동
    IEnumerator MoveTo(Vector3 target, float t)
    {
        if (t <= 0f)
        {
            transform.position = target;
            yield break;
        }


        Vector3 from = transform.position;
        float e = 0f;

        while (e < 1f)
        {
            e += Time.deltaTime / t;
            transform.position = Vector3.Lerp(from, target, Mathf.Clamp01(e));
            yield return null;
        }
    }

    public IEnumerator Serve(PlaceTrigger place)
    {
        GameObject completeMark = _bubble.GetComponent<Transform>().GetChild(1).gameObject;

        yield return new WaitForSeconds(0.3f);

        // 크리티컬 확률 체크 후 발동
        yield return StartCoroutine(Critical());

        // CompleteMark 잠깐 켜기
        completeMark.SetActive(true);

        // 판매 (크리티컬 터졌으면 가격 1.5배)
        CounterHandler.Instance.Selling(place.TriggerRecipeId, _didCritical);
        yield return new WaitForSeconds(0.5f);

        _didCritical = false;

        // CompleteMark 끄기
        completeMark.SetActive(false);

        // 말풍선 비활성화
        yield return new WaitForSeconds(0.1f);
        _bubble.SetActive(false);

        // 그 자리에 레시피 못 뜨게 막기
        TriggerManager.Instance.SetRecipeId(TriggerType.Counter, place.PlaceIndex, -1);

        // 손님이 음식 가져가기
        place.PlacedPlate.sprite = null;

        yield return new WaitForSeconds(0.1f);
        _served = true;
    }

    public IEnumerator Critical()
    {
        float rand = UnityEngine.Random.value;
        // rand = 0.01f; // 테스트용
        Debug.Log($"Customer Critical : rand = {rand}, critical = {Managers.Data.User.Critical}");

        if (rand <= Managers.Data.User.Critical)
        {
            _didCritical = true;
            // 폭죽 터지는 효과
            _criticalEffect.SetActive(true);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.SfxClips[(int)Define.MonsterSceneSFX.CustomerCritical].name);
            yield return new WaitForSeconds(0.9f);
            _criticalEffect.SetActive(false);

            Color textCol;
            ColorUtility.TryParseHtmlString("#FF3177", out textCol);
            _priceText.color = textCol;
        }
    }

    public void ResetForReuse()
    {
        // CompleteMark 리셋
        _bubble.GetComponent<Transform>().GetChild(1).gameObject.SetActive(false);
        // 말풍선 끈 상태로 입장
        _bubble.SetActive(false);

        // 손님 스프라이트 랜덤 선택
        if (_customerControllers != null && _customerControllers.Length > 0)
        {
            _animator.runtimeAnimatorController = _customerControllers[UnityEngine.Random.Range(0, _customerControllers.Length)];
        }

        // TODO : 손님 오브젝트 리셋 (필요하면)
    }

    public void Order(int laneIndex)
    {
        Image recipeImage = _bubble.GetComponent<Transform>().GetChild(0).GetComponent<Image>();
        // List<int> selectedRecipe = Test.Instance.GetSelectedRecipes();
        List<int> selectedRecipe = Managers.Data.User.SelectRecipes;

        // int n = 0;
        // List<int> ableList = new List<int>();

        // for (int i = 0; i < selectedRecipe.Count; i++)
        // {
        //     if (selectedRecipe[i] != -1)
        //     {
        //         n++;
        //         ableList.Add(i);
        //     }
        // }

        // int ranNum = UnityEngine.Random.Range(0, n);
        // int recipeId = selectedRecipe[ableList[ranNum]];

        int recipeId = CustomerOrder.Instance.GetOrderRecipe();

        TriggerManager.Instance.SetRecipeId(TriggerType.Counter, laneIndex, recipeId);

        _bubble.SetActive(true);
        recipeImage.sprite = Utils.LoadIconSprite(Managers.Data.Recipes.GetByKey(recipeId).Icon);
    }
}
