using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    public string AD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917";
    public bool IsCompleted => _isCompleted;
    public bool IsAdStarted => _isAdStarted;

    private bool _isCompleted = false;
    private bool _isAdStarted = false;
    private Stopwatch stopwatch = new Stopwatch();

    void Awake()
    {
        Instance = this;

        // 구글 모바일 광고 SDK Initialize
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            UnityEngine.Debug.Log("MobileAds SDK is initialized.");
        });
    }
    void Start()
    {
        // 처음에 미리 한번 로드
        InitLoad();
    }

    public void InitLoad()
    {
        var adRequest = new AdRequest(); // 요청

        // 광고 로드 요청
        RewardedAd.Load(AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                UnityEngine.Debug.Log("광고 로드 실패");
                DestroyAd(ad);
                return;
            }
            UnityEngine.Debug.Log("광고 로드 성공");
        });
    }
    
    public void LoadAds()
    {
        if (_isAdStarted) return;

        _isAdStarted = true;
        var adRequest = new AdRequest(); // 요청

        // 광고 로드 요청
        RewardedAd.Load(AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                UnityEngine.Debug.Log("광고 로드 실패");
                DestroyAd(ad);
                return;
            }
            UnityEngine.Debug.Log("광고 로드 성공");
            ShowAd(ad);
        });
    }

    public IEnumerator WaitUntilAdCompleted()
    {
        yield return new WaitUntil(() => _isCompleted);
    }

    public void SetIsCompleted(bool b)
    {
        _isCompleted = b;
    }

    void ShowAd(RewardedAd rewardedAd)
    {
        stopwatch.Reset();
        
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            stopwatch.Start();


            rewardedAd.Show((Reward reward) =>
            {
                stopwatch.Stop();
                double elapsed = stopwatch.Elapsed.TotalSeconds;

                _isCompleted = true;
                _isAdStarted = false;
        
                // 광고 나옴, 유저 보상 받음
                UnityEngine.Debug.Log("광고 보상");
                UnityEngine.Debug.Log($"광고 재생 시간: {elapsed:F2}초");

                // 메모리 누수 방지
                DestroyAd(rewardedAd);

                // 다음 광고 준비
                ReloadAd(rewardedAd);
            });
        }
    }

    void ListenToAdEvents(RewardedAd rewardedAd)
    {
        rewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            // 광고에서 수익 창출 추정
        };
        rewardedAd.OnAdImpressionRecorded += () =>
        {
            // 광고 노출 기록
        };
        rewardedAd.OnAdClicked += () =>
        {
            // 광고에서 컨텐츠를 열었을 때
        };
        rewardedAd.OnAdFullScreenContentOpened += () =>
        {
            // 광고가 전체 화면 콘텐츠를 열었을 때
        };
        rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            // 광고가 전체 화면 콘텐츠를 닫았을 때
        };
        rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            // 광고가 전체 화면 콘텐츠를 열지 못했을 때
        };
    }

    void DestroyAd(RewardedAd rewardedAd)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            UnityEngine.Debug.Log("Destroy 완료");
        }
    }

    void ReloadAd(RewardedAd rewardedAd)
    {
        rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            // 다음 광고 미리 로드
            var adRequest = new AdRequest();
            RewardedAd.Load(AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                UnityEngine.Debug.Log("다음 광고 준비 중");

                if (error != null)
                {
                    UnityEngine.Debug.Log("광고 로드 실패");
                    DestroyAd(ad);
                    return;
                }
                UnityEngine.Debug.Log("광고 준비 완료");
            });
        };
    }
}