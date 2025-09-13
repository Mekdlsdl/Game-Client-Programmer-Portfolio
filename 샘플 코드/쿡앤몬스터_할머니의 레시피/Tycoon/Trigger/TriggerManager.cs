///<summary>
/// 
/// TriggerManager 스크립트
/// 
/// - Trigger 겹침 상태 관리 / 현재 포커스 대상 선정 / 이벤트 외부로 재발행 기능 담당
/// 
/// 
/// - 인터페이스
///     ITriggerSink : TriggerZone(2D/3D)에서 올라오는 Enter/Exit 이벤트를 수신
///                    카테고리(TriggerType) 및 서브타입(DesignEnums.Cookwares)별로 겹치는 트리거 집합 유지
///                    SelectionPolicy에 따라 “현재 활성(포커스)” 트리거 선정
///                    포커스된 IPlaceState의 변경(조리 여부/레시피/게이지)을 구독하여 이벤트로 재발행
/// 
/// - 주요 함수
///     RebuildRegistry() : 전체 트리거 관리 (안 밟고 있어도 접근 가능)
/// 
///     (enum) SelectionPolicy
///         - LastEntered : 마지막으로 진입한 트리거 우선
///         - Closest : 플레이어와 가장 가까운 트리거 우선
///     GetCurrent(TriggerType) / GetCurrent(DesignEnums.Cookwares) : 현재 포커스된 트리거 즉시 조회 API
///     GetSnapshot() : 현재 포커스된 IPlaceState의 스냅샷 조회 (IsCooking/RecipeId/GaugeRatio)
/// 
/// - 이벤트
///     OnCurrentTypeChanged(TriggerType, PlaceTrigger)
///     OnCurrentCookwareChanged(DesignEnums.Cookwares, PlaceTrigger)
///     OnPlaceStateFocused(IPlaceState) : 포커스 대상 교체 시
///     OnIsCookingChanged(bool)         : 조리 여부 변경
///     OnRecipeChanged(int)             : 레시피 변경
///     OnGaugeChanged(float)            : 게이지(0~1) 변경
/// 
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerSink
{
    void OnZoneEvent(PlaceTrigger place, bool entered);
}


[DefaultExecutionOrder(-100)]
public class TriggerManager : MonoBehaviour, ITriggerSink
{
    public enum SelectionPolicy
    {
        LastEntered,
        Closest
    }

    public static TriggerManager Instance { get; private set; }
    public static event Action<TriggerManager> OnReady;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        OnReady  = null; // 정리(씬 재시작 시 중복 구독 방지)
    }


    [Header("Selection Policy")]
    [SerializeField] private SelectionPolicy selectionPolicy = SelectionPolicy.LastEntered;
    [SerializeField] private Transform subject; // Closest 기준 (= 플레이어 transform)


    // 상태 저장: 카테고리/서브타입 별로 "현재 겹치는 트리거" 집합
    private readonly Dictionary<TriggerType, HashSet<PlaceTrigger>> insideByType = new();
    private readonly Dictionary<DesignEnums.Cookwares, HashSet<PlaceTrigger>> insideByCookware = new();

    // 진입 시각 기록(LastEntered 선택 정책용)
    private readonly Dictionary<PlaceTrigger, float> enterTime = new();

    // 현재 활성 트리거들
    public PlaceTrigger CurrentCounter { get; private set; }
    public PlaceTrigger CurrentEmpty { get; private set; }
    public PlaceTrigger CurrentBin { get; private set; }

    // 조리대: 타입별 현재값
    private readonly Dictionary<DesignEnums.Cookwares, PlaceTrigger> currentCookware = new();
    public PlaceTrigger CurrentAnyCookware { get; private set; } // (카테고리 레벨)

    // 포커스된 상태(IPlaceState)
    private IPlaceState currentState;

    // 이벤트: 변경 시 브로드캐스트
    public event Action<TriggerType, PlaceTrigger> OnCurrentTypeChanged;   // Cookware/Counter/Empty/Bin 단위
    public event Action<DesignEnums.Cookwares, PlaceTrigger> OnCurrentCookwareChanged;    // Pot/Pan/Blender 단위

    public event Action<IPlaceState> OnPlaceStateFocused; // 포커스 대상 바뀔 때
    public event Action<Sprite> OnPlacedPlateChanged;
    public event Action<bool> OnIsCookingChanged;
    public event Action<int>   OnRecipeChanged;
    public event Action<float> OnGaugeChanged;

    public event Action<PlaceTrigger> OnCookwareEntered;
    public event Action<PlaceTrigger> OnCookwareExited;
    public event Action<PlaceTrigger> OnCounterEntered;
    public event Action<PlaceTrigger> OnCounterExited;
    public event Action<PlaceTrigger> OnEmptyEntered;
    public event Action<PlaceTrigger> OnEmptyExited;
    public event Action<PlaceTrigger> OnBinEntered;
    public event Action<PlaceTrigger> OnBinExited;



    // ========== Place Registry & Remote Setters ==========
    // 밟고 있지 않아도 참조할 트리거들
    private readonly Dictionary<(TriggerType type, int index), PlaceTrigger> byTypeIndex = new();
    private readonly Dictionary<(DesignEnums.Cookwares cookware, int index), PlaceTrigger> byCookwareIndex = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); return;
        }

        Instance = this;
        RebuildRegistry(); // 씬 로드 시 1회 구축
        OnReady?.Invoke(this);
    }

    public void RebuildRegistry()
    {
        byTypeIndex.Clear();
        byCookwareIndex.Clear();

        // 비활성 포함 전체 스캔
        var all = FindObjectsOfType<PlaceTrigger>(true);
        foreach (var p in all)
        {
            byTypeIndex[(p.Type, p.PlaceIndex)] = p;
            if (p.Type == TriggerType.Cookware)
                byCookwareIndex[(p.Cookware, p.PlaceIndex)] = p;
        }
    }

    // 헬퍼
    public PlaceTrigger GetPlace(TriggerType type, int placeIndex)
    => byTypeIndex.TryGetValue((type, placeIndex), out var p) ? p : null;

    public PlaceTrigger GetCookware(DesignEnums.Cookwares cookware, int placeIndex)
    => byCookwareIndex.TryGetValue((cookware, placeIndex), out var p) ? p : null;




    // 세터 API
    // RecipeId
    public bool SetPlacedPlate(TriggerType type, int placeIndex, Sprite plateSprite)
    {
        var place = GetPlace(type, placeIndex);
        if (!place) return false;
        place.SetPlacedPlate(plateSprite);  // ← PlaceTrigger에 있는 세터(이벤트 발행)
        return true;
    }
    public bool SetRecipeId(TriggerType type, int placeIndex, int recipeId)
    {
        var place = GetPlace(type, placeIndex);
        if (!place) return false;
        place.SetRecipeId(recipeId);
        return true;
    }

    public bool SetRecipeId(DesignEnums.Cookwares cookware, int placeIndex, int recipeId)
    {
        var place = GetCookware(cookware, placeIndex);
        if (!place) return false;
        place.SetRecipeId(recipeId);
        return true;
    }

    // 게이지 상태
    public bool SetGaugeRatio(DesignEnums.Cookwares cookware, int placeIndex, float ratio)
    {
        var place = GetCookware(cookware, placeIndex);
        if (!place) return false;
        place.SetGaugeRatio(ratio);
        return true;
    }

    // Cookware 활성화상태
    public bool SetIsCooking(DesignEnums.Cookwares cookware, int placeIndex, bool v)
    {
        var place = GetCookware(cookware, placeIndex);
        if (!place) return false;
        place.SetIsCooking(v);
        return true;
    }



    // 핸들러
    private void HandlePlate(Sprite s) => OnPlacedPlateChanged?.Invoke(s);
    private void HandleCooking(bool v) => OnIsCookingChanged?.Invoke(v);
    private void HandleRecipe(int id)  => OnRecipeChanged?.Invoke(id);
    private void HandleGauge(float g)  => OnGaugeChanged?.Invoke(g);


    // ========== ITriggerSink ==========
    public void OnZoneEvent(PlaceTrigger place, bool entered)
    {
        if (place == null) return;

        // 공통: 카테고리 집합 갱신
        var typeSet = GetSet(insideByType, place.Type);
        // -> 닿은 카테고리들


        if (entered)
        {
            typeSet.Add(place);
            enterTime[place] = Time.time;

            switch (place.Type)
            {
                case TriggerType.Cookware:
                    OnCookwareEntered?.Invoke(place);
                    break;
                case TriggerType.Counter:
                    OnCounterEntered?.Invoke(place);
                    break;
                case TriggerType.Empty:
                    OnEmptyEntered?.Invoke(place);
                    break;
                case TriggerType.Bin:
                    OnBinEntered?.Invoke(place);
                    break;
            }
        }
        else
        {
            typeSet.Remove(place);
            enterTime.Remove(place);

            switch (place.Type)
            {
                case TriggerType.Cookware:
                    OnCookwareExited?.Invoke(place);
                    break;
                case TriggerType.Counter:
                    OnCounterExited?.Invoke(place);
                    break;
                case TriggerType.Empty:
                    OnEmptyExited?.Invoke(place);
                    break;
                case TriggerType.Bin:
                    OnBinExited?.Invoke(place);
                    break;
            }
        }

        // 카테고리별 현재값 재계산
        RecomputeType(place.Type);

        // Cookware 하위타입 집합/현재값도 갱신
        if (place.Type == TriggerType.Cookware)
        {
            var key = place.Cookware;
            var cookwareSet = GetSet(insideByCookware, key);
            if (entered)
            {
                cookwareSet.Add(place);
                // enterTime은 위에서 이미 기록됨
            }
            else
            {
                cookwareSet.Remove(place);
            }
            RecomputeCookwareType(key);
        }

        // 변화 있을 경우 다시 평가
        RecomputeFocus();
    }

    // ========== Public Query APIs ==========
    public PlaceTrigger GetCurrent(TriggerType type)
    {
        return type switch
        {
            TriggerType.Counter => CurrentCounter,
            TriggerType.Empty => CurrentEmpty,
            TriggerType.Cookware => CurrentAnyCookware,
            TriggerType.Bin => CurrentBin,
            _ => null
        };
    }

    public PlaceTrigger GetCurrent(DesignEnums.Cookwares type)
    {
        return currentCookware.TryGetValue(type, out var p) ? p : null;
    }

    public IPlaceState CurrentState => currentState; // 즉시 조회


    public PlaceSnapshot GetSnapshot()
    {
        return new PlaceSnapshot
        {
            Focused = currentState,
            PlacedPlate = currentState != null ? currentState.PlacedPlate : null,
            GaugeObject = currentState != null ? currentState.GaugeObject : null,
            IsCooking = currentState != null ? currentState.IsCooking : false,
            RecipeId = currentState != null ? currentState.TriggerRecipeId : -1,
            GaugeRatio = currentState != null ? currentState.GaugeRatio : 0f
        };
    }

    public struct PlaceSnapshot
    {
        public IPlaceState Focused;
        public SpriteRenderer PlacedPlate;
        public GameObject GaugeObject;
        public bool IsCooking;
        public int RecipeId;
        public float GaugeRatio;
    }
    
    private void RecomputeType(TriggerType type)
    {
        var set = GetSet(insideByType, type);
        var next = SelectActive(set);

        switch (type)
        {
            case TriggerType.Counter:
                if (CurrentCounter != next)
                {
                    CurrentCounter = next;
                    OnCurrentTypeChanged?.Invoke(TriggerType.Counter, next);
                }
                break;
            case TriggerType.Empty:
                if (CurrentEmpty != next)
                {
                    CurrentEmpty = next;
                    OnCurrentTypeChanged?.Invoke(TriggerType.Empty, next);
                }
                break;
            case TriggerType.Cookware:
                if (CurrentAnyCookware != next)
                {
                    CurrentAnyCookware = next;
                    OnCurrentTypeChanged?.Invoke(TriggerType.Cookware, next);
                }
                break;
            case TriggerType.Bin:
                if (CurrentBin != next)
                {
                    CurrentBin = next;
                    OnCurrentTypeChanged?.Invoke(TriggerType.Bin, next);
                }
                break;
        }
    }
    private void RecomputeCookwareType(DesignEnums.Cookwares type)
    {
        var set = GetSet(insideByCookware, type);
        var next = SelectActive(set);

        currentCookware.TryGetValue(type, out var prev);
        if (prev != next)
        {
            currentCookware[type] = next;
            OnCurrentCookwareChanged?.Invoke(type, next);
        }

        // Cookware 카테고리의 통합(CurrentAnyCookware)도 재계산(선택 정책에 따라 달라질 수 있음)
        RecomputeType(TriggerType.Cookware);
    }

    private void RecomputeFocus()
    {
        // 우선순위: Cookware → Counter → Bin → Empty
        var focus = CurrentAnyCookware ?? CurrentCounter ?? CurrentBin ?? CurrentEmpty;
        UpdateFocusedState(focus);
    }

    private void UpdateFocusedState(PlaceTrigger focused)
    {
        // 이전 구독 해제
        if (currentState != null)
        {
            currentState.OnPlacedPlateChanged -= HandlePlate;
            currentState.OnIsCookingChanged -= HandleCooking;
            currentState.OnRecipeChanged    -= HandleRecipe;
            currentState.OnGaugeChanged     -= HandleGauge;
        }

        // 새 대상 결합
        currentState = null;
        if (focused)
        {
            // 부모/자식 어디에 붙었든 찾기
            currentState = focused.GetComponentInParent<IPlaceState>() ?? focused.GetComponentInChildren<IPlaceState>();
        }

        // 구독 + 초기값 브로드캐스트
        if (currentState != null)
        {
            currentState.OnPlacedPlateChanged += HandlePlate;
            currentState.OnIsCookingChanged += HandleCooking;
            currentState.OnRecipeChanged    += HandleRecipe;
            currentState.OnGaugeChanged     += HandleGauge;

            OnPlaceStateFocused?.Invoke(currentState);
            OnPlacedPlateChanged?.Invoke(currentState.PlacedPlate.sprite);
            OnIsCookingChanged?.Invoke(currentState.IsCooking);
            OnRecipeChanged?.Invoke(currentState.TriggerRecipeId);
            OnGaugeChanged?.Invoke(currentState.GaugeRatio);
        }
        else
        {
            OnPlaceStateFocused?.Invoke(null);
            OnPlacedPlateChanged?.Invoke(null);
            OnIsCookingChanged?.Invoke(false);
            OnRecipeChanged?.Invoke(-1);
            OnGaugeChanged?.Invoke(0f);
        }
    }

    // ========== Selection Policies ==========
    private PlaceTrigger SelectActive(HashSet<PlaceTrigger> set)
    {
        if (set == null || set.Count == 0) return null;
        return selectionPolicy == SelectionPolicy.LastEntered
            ? SelectLastEntered(set)
            : SelectClosest(set);
    }

    private PlaceTrigger SelectLastEntered(HashSet<PlaceTrigger> set)
    {
        PlaceTrigger best = null;
        float bestTime = float.NegativeInfinity;
        foreach (var p in set)
        {
            if (!enterTime.TryGetValue(p, out var t)) t = float.NegativeInfinity;
            if (t > bestTime) { bestTime = t; best = p; }
        }
        return best;
    }

    private Vector3 Origin => subject ? subject.position : transform.position;

    private PlaceTrigger SelectClosest(HashSet<PlaceTrigger> set)
    {
        PlaceTrigger best = null;
        float bestSqr = float.PositiveInfinity;
        var origin = Origin; // 매니저를 플레이어 루트에 붙였다는 가정

        foreach (var p in set)
        {
            if (p == null) continue;
            var sqr = (p.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = p; }
        }
        return best;
    }

    // ========== Utils ==========
    private static HashSet<PlaceTrigger> GetSet<TKey>(Dictionary<TKey, HashSet<PlaceTrigger>> dict, TKey key)
    {
        if (!dict.TryGetValue(key, out var set))
        {
            set = new HashSet<PlaceTrigger>();
            dict[key] = set;
        }
        return set;
    }


}
