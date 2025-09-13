using System;
using UnityEngine;
using System.Collections;

public class AreaScaler : MonoBehaviour
{
    [NonSerialized] private RectTransform _centerArea;  // 스케일을 조정할 대상
    [SerializeField] private RectTransform _checkRect;  // 닿는지 확인할 영역
    [SerializeField] private RectTransform _boundsRect;  // 경계 영역
    [SerializeField] private Camera _uiCamera;

    IEnumerator Start()
    {
        if (!_centerArea) _centerArea = GetComponent<RectTransform>();
        if (!_centerArea || !_boundsRect) yield break;

        // 레이아웃 계산이 모두 끝나도록 한 프레임(or 두 프레임) 대기
        yield return null; // 1프레임
        Canvas.ForceUpdateCanvases();
        yield return null; // 필요시 한 번 더
        Canvas.ForceUpdateCanvases();


        if (IsOverlapping(_boundsRect, _checkRect, _uiCamera))
        {
            // 경계를 벗어나면 스케일 줄이기
            _centerArea.localScale = new Vector3(0.9f, 0.9f, 1f);

            // 살짝 밑으로 내리기
            Vector2 offMax = _centerArea.offsetMax;
            offMax.y = -110f;
            _centerArea.offsetMax = offMax;
        }
    }

    bool IsOverlapping(RectTransform bounds, RectTransform area, Camera cam)
    {
        Rect ra = ToScreenRect(area, cam);
        Rect rb = ToScreenRect(bounds, cam);
        return ra.Overlaps(rb, true);
    }

    static Rect ToScreenRect(RectTransform rt, Camera cam)
    {
        Vector3[] wc = new Vector3[4];
        rt.GetWorldCorners(wc);

        // 월드 → 스크린
        Vector2 p0 = RectTransformUtility.WorldToScreenPoint(cam, wc[0]);
        Vector2 p2 = RectTransformUtility.WorldToScreenPoint(cam, wc[2]);

        float xMin = Mathf.Min(p0.x, p2.x);
        float xMax = Mathf.Max(p0.x, p2.x);
        float yMin = Mathf.Min(p0.y, p2.y);
        float yMax = Mathf.Max(p0.y, p2.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}
