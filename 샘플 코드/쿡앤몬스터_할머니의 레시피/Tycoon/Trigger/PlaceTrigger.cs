using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    Cookware,
    Counter,
    Empty,
    Bin
}
public interface IPlaceState
{
    TriggerType Type { get; }
    DesignEnums.Cookwares? Cookwares { get; }   // Cookware일 때만 의미

    int PlaceIndex { get; } // 고정값

    SpriteRenderer PlacedPlate { get; } // 고정값
    GameObject GaugeObject { get; } // 고정값
    bool IsCooking { get; }
    int TriggerRecipeId { get; }
    float GaugeRatio { get; }

    // 필요한 것만 쓰기
    event Action<Sprite> OnPlacedPlateChanged;
    event Action<bool> OnIsCookingChanged; // Cookware 활성화
    event Action<int> OnRecipeChanged; // 각 제작대/카운터에서 갖고 있는 음식
    event Action<float> OnGaugeChanged; // Cookware Gauge
}


public class PlaceTrigger : MonoBehaviour, IPlaceState
{
    [Header("Classification")]
    public TriggerType TriggerType;
    public DesignEnums.Cookwares Cookware;   // Category==Cookware일 때만 사용

    [Header("Metadata")]
    [SerializeField] int placeIndex = -1;

    [Header("Runtime")]
    [SerializeField] SpriteRenderer placedPlate;
    [SerializeField] GameObject gaugeObject;
    [SerializeField] bool isCooking = false;
    [SerializeField] int triggerRecipeId = -1;
    [SerializeField, Range(0, 1)] float gaugeRatio = 0f;

    // IPlaceState 구현

    public TriggerType Type => TriggerType;
    public DesignEnums.Cookwares? Cookwares
        => Type == TriggerType.Cookware ? (DesignEnums.Cookwares?)Cookware : null;

    public int PlaceIndex => placeIndex;

    public SpriteRenderer PlacedPlate => placedPlate;
    public GameObject GaugeObject => gaugeObject;
    public bool IsCooking => isCooking;
    public int TriggerRecipeId => triggerRecipeId;
    public float GaugeRatio => gaugeRatio;

    public event Action<Sprite> OnPlacedPlateChanged;
    public event Action<bool> OnIsCookingChanged;
    public event Action<int> OnRecipeChanged;
    public event Action<float> OnGaugeChanged;

    // 값 변경 메서드(꼭 이걸로 바꾸기 - 이벤트 나가야함)
    public void SetPlacedPlate(Sprite s)
    {
        if (placedPlate.sprite == s) return;

        placedPlate.sprite = s;

        OnPlacedPlateChanged?.Invoke(s);
    }

    public void SetIsCooking(bool v)
    {
        if (isCooking == v) return;

        isCooking = v;

        OnIsCookingChanged?.Invoke(v);
    }

    public void SetRecipeId(int id)
    {
        if (triggerRecipeId == id) return;
        triggerRecipeId = id;
        OnRecipeChanged?.Invoke(id);
    }

    public void SetGaugeRatio(float g)
    {
        g = Mathf.Clamp01(g);
        if (Mathf.Approximately(gaugeRatio, g)) return;
        gaugeRatio = g; OnGaugeChanged?.Invoke(g);
    }

    // 인스펙터에서 바로 수정하는 경우 초기 이벤트가 필요하면, Start에서 한 번 쏠 수도 있음.
    void Start()
    {
        // 초기 브로드캐스트가 필요하면 주석 해제
        // OnIsCookingChanged?.Invoke(isCooking);
        // OnRecipeChanged?.Invoke(triggerRecipeId);
        // OnGaugeChanged?.Invoke(gauge01);
    }
}
