/// <summary>
/// 
/// CustomerSpawner 스크립트
/// 
/// - 손님 자동 스폰 담당
/// 
/// - 주요 함수:
///     SpawnInLane() : 레인에 손님 스폰
///     HandleArrived() : 카운터에 도착했을 때 기능 관리
///     HandleLeft() : 카운터에서 떠날 때 기능 관리
///     ServeLane() : 
///     Shuffle() : 레인 섞기
/// 
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner Instance { get; private set; }

    [Header("Pool & Points (size = 4)")]
    // [SerializeField] int size = 4;
    [SerializeField] CustomerPool pool;
    [SerializeField] Transform startPoint;
    [SerializeField] Transform doorPoint;
    [SerializeField] Transform[] counterPoints;
    [SerializeField] Transform exitPoint;
    


    [Header("Times")]
    [SerializeField] float timeIn = 1.5f;
    [SerializeField] float timeWait = 1.5f;
    [SerializeField] float timeOut = 1.5f;

    float spawnInterval = 0;


    readonly Customer[] _laneCustomer = new Customer[4];
    readonly System.Random _random = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitialSpawnRoutine());
    }

    IEnumerator InitialSpawnRoutine()
    {
        var lanes = new List<int> { 0, 1, 2, 3 };
        Shuffle(lanes);
        foreach (var lane in lanes)
        {
            SpawnInLane(lane);
            spawnInterval = _random.Next(1, 3);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnInLane(int laneIndex)
    {
        var c = pool.Get();
        _laneCustomer[laneIndex] = c;

        c.OnArrivedAtCounter += HandleArrived;
        c.OnLeftLane += HandleLeft;

        c.Begin(
            laneIndex,
            startPoint.position,
            doorPoint.position,
            counterPoints[laneIndex].position,
            exitPoint.position,
            timeIn, timeWait, timeOut
            );
    }

    void HandleArrived(Customer c, int laneIndex)
    {
        c.Order(laneIndex);
    }

    void HandleLeft(Customer c, int laneIndex)
    {
        c.OnArrivedAtCounter -= HandleArrived;
        c.OnLeftLane -= HandleLeft;

        _laneCustomer[laneIndex] = null;
        pool.Return(c);

        SpawnInLane(laneIndex);
    }

    public void ServeLane(int laneIndex, PlaceTrigger place)
    {
        var c = _laneCustomer[laneIndex];
        if (c != null) StartCoroutine(c.Serve(place));
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
