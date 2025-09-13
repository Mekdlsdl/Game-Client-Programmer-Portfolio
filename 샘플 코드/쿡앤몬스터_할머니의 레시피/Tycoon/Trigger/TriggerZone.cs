using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private PlaceTrigger place;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 트리거에 플레이어가 닿았을 때만 동작
        if (!other.transform.parent.GetComponent<PlayerController>()) return;

        // PlaceTrigger 끼워져있는지 확인
        if (!place) place = GetComponentInParent<PlaceTrigger>() ?? GetComponentInChildren<PlaceTrigger>();
        if (place == null) return;

        var sink = other.GetComponentInParent<ITriggerSink>() ?? other.GetComponentInChildren<ITriggerSink>();
        if (sink != null)
        {
            sink.OnZoneEvent(place, entered: true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 트리거에 플레이어가 닿았을 때만 동작
        if (!other.transform.parent.GetComponent<PlayerController>()) return;

        // PlaceTrigger 끼워져있는지 확인
        if (!place) place = GetComponentInParent<PlaceTrigger>() ?? GetComponentInChildren<PlaceTrigger>();
        if (place == null) return;

        var sink = other.GetComponentInParent<ITriggerSink>() ?? other.GetComponentInChildren<ITriggerSink>();
        if (sink != null)
        {
            sink.OnZoneEvent(place, entered: false);
        }
    }
}
