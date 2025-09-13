using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSortingOrder : MonoBehaviour
{
    [SerializeField] private int _playerLayer = 15;
    private SetSortingOrder _sso;

    void Awake()
    {
        _sso = GetComponent<SetSortingOrder>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"{other} 부딪힘");
        // 플레이어가 부딪혔고, 레이어를 조정해야 할 때
        if (other.transform.parent.GetComponent<PlayerController>() && _sso)
        {
            for (int i = 0; i < 2; i++) // 접시까지만
            {
                other.transform.parent.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = _sso.layer - 1;
            }
        }

            
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.parent.GetComponent<PlayerController>() && _sso)
        {
            for (int i = 0; i < 2; i++) // 접시까지만
            {
                other.transform.parent.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = _playerLayer + i; // 원래 값 (접시는 더 위로)
            }
        }
    }
}
