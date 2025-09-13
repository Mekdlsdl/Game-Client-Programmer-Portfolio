using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHUDListener : MonoBehaviour
{
    TriggerManager manager;
    [SerializeField] private GameObject _gauge;

    void OnEnable()
    {
        TryHook();
        TriggerManager.OnReady += Hook;
    }

    void Start()
    {
        TryHook();
    }

    void OnDisable()
    {
        if (manager != null) UnHook(manager);
        TriggerManager.OnReady -= Hook;
    }


    void TryHook()
    {
        if (manager != null) return;

        var triggerManager = TriggerManager.Instance ?? FindObjectOfType<TriggerManager>();
        if (triggerManager != null) Hook(triggerManager);
    }

    void Hook(TriggerManager triggerManager)
    {
        if (triggerManager == null) return;
        if (manager != null) return;

        manager = triggerManager;

        manager.OnPlaceStateFocused += HandleFocus;

        var s = manager.GetSnapshot();
        _gauge = s.Focused?.GaugeObject;

        triggerManager.OnCookwareEntered += HandleCookwareEnter;
        triggerManager.OnCookwareExited += HandleCookwareExit;
        triggerManager.OnCounterEntered += HandleCounterEnter;
        triggerManager.OnCounterExited += HandleCounterExit;
        triggerManager.OnEmptyEntered += HandleEmptyEnter;
        triggerManager.OnEmptyExited += HandleEmptyExit;
        triggerManager.OnBinEntered += HandleBinEnter;
        triggerManager.OnBinExited += HandleBinExit;
    }

    void UnHook(TriggerManager triggerManager)
    {
        triggerManager.OnCookwareEntered -= HandleCookwareEnter;
        triggerManager.OnCookwareExited -= HandleCookwareExit;
        triggerManager.OnCounterEntered -= HandleCounterEnter;
        triggerManager.OnCounterExited -= HandleCounterExit;
        triggerManager.OnEmptyEntered -= HandleEmptyEnter;
        triggerManager.OnEmptyExited -= HandleEmptyExit;
        triggerManager.OnBinEntered -= HandleBinEnter;
        triggerManager.OnBinExited -= HandleBinExit;
    }


    void HandleFocus(IPlaceState st)
    {
        _gauge = st?.GaugeObject;
    }

    void HandleCookwareEnter(PlaceTrigger place)
    {
        if (place.IsCooking && _gauge == null)
        {
            if (place.TriggerRecipeId == -1)
            {
                CookingHandler.Instance.ActivateCookingPopup(place.Cookware);
            }
        }
    }

    void HandleCookwareExit(PlaceTrigger place)
    {
        if (place.IsCooking)
        {
            CookingHandler.Instance.InactivateCookingPopup();
        }
    }

    void HandleCounterEnter(PlaceTrigger place)
    {
        CounterHandler.Instance.CanServing(place);
    }

    void HandleCounterExit(PlaceTrigger place)
    {
        CounterHandler.Instance.InactivateRecipePopup();
    }

    void HandleEmptyEnter(PlaceTrigger place)
    {

    }

    void HandleEmptyExit(PlaceTrigger place)
    {

    }
    
    void HandleBinEnter(PlaceTrigger place)
    {
        Debug.Log("쓰레기통에 닿음");

        if (BinManager.Instance.CanOpenBinChenck())
        {
            StartCoroutine(BinManager.Instance.ActivateBin());
        }
    }

    void HandleBinExit(PlaceTrigger place)
    {
        
    }
}
