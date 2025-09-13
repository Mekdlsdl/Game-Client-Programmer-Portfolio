using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerPool : MonoBehaviour
{
    [SerializeField] Customer prefeb;
    [SerializeField] int initialSize = 8;

    readonly Queue<Customer> _q = new();


    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var c = Instantiate(prefeb, transform);
            c.gameObject.SetActive(false);
            _q.Enqueue(c);
        }
    }

    public Customer Get()
    {
        var c = _q.Count > 0 ? _q.Dequeue() : Instantiate(prefeb, transform);
        c.gameObject.SetActive(true);
        c.ResetForReuse();
        return c;
    }

    public void Return(Customer c)
    {
        c.gameObject.SetActive(false);
        _q.Enqueue(c);
    }
}
