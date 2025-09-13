using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Barigate : MonoBehaviour
{
    [SerializeField] private Collider2D _block;
    [SerializeField] private GameObject _errorMessage;

    private readonly Dictionary<GameObject, HashSet<Collider2D>> ignoredPerPlayer = new();
    private readonly Dictionary<GameObject, int> insideCount = new();
    

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{collision} 닿음");

        PlayerController playerController = collision.transform.parent.GetComponent<PlayerController>();

        // 플레이어 아니면 return
        if (playerController == null) return;


        // =============== 플레이어일 경우 ===============

        Debug.Log("바리게이트에 플레이어 닿음");

        GameObject root = playerController.gameObject;
        insideCount.TryGetValue(root, out var count);
        insideCount[root] = count + 1;

        // 음식 들고 있으면 에러 메시지
        if (StageManager.Instance.TycoonInfo.HoldingPlateId != -1)
        {
            _errorMessage.SetActive(true);
        }

        // 음식을 안 들고 있을 경우이거나 전투모드일 경우
        if (count == 0
        && (StageManager.Instance.TycoonInfo.HoldingPlateId == -1
        || StageManager.Instance.CurMode == Mode.Combat))
        {
            var set = ignoredPerPlayer.ContainsKey(root)
            ? ignoredPerPlayer[root]
            : (ignoredPerPlayer[root] = new HashSet<Collider2D>());

            foreach (var c in root.GetComponentsInChildren<Collider2D>(true))
            {
                if (!c || !_block) continue;
                if (set.Add(c)) Physics2D.IgnoreCollision(_block, c, true);
            }
        }   
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"{collision} 떨어짐");

        PlayerController playerController = collision.transform.parent.GetComponent<PlayerController>();

        // 플레이어 아니면 return
        if (playerController == null) return;


        // =============== 플레이어일 경우 ===============
        Debug.Log("바리게이트에서 플레이어 떨어짐");

        GameObject root = playerController.gameObject;
        if (!insideCount.TryGetValue(root, out var count)) return;

        count -= 1;
        if (count > 0) { insideCount[root] = count; return; }

        insideCount.Remove(root);

        // 에러 메시지 끄기
        if (_errorMessage.activeSelf) _errorMessage.SetActive(false);


        if (ignoredPerPlayer.TryGetValue(root, out var set))
        {
            foreach (var c in set)
                if (c && _block) Physics2D.IgnoreCollision(_block, c, false);
            ignoredPerPlayer.Remove(root);
        }
    }
}
