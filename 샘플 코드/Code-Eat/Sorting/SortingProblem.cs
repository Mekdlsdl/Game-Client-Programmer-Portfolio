using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SortingProblem : MonoBehaviour
{
    [SerializeField] private GameObject guide, divider, beforeDishes, afterDishes, option;
    [SerializeField] private List<GameObject> dishes;
    [SerializeField] private int problemNum;
    public TMP_Text beforeDishesText;
    public List<int> randomList;
    public static (int, int) answerDishes;
    public static List<GameObject> randomDishes;
    public static List<float> positionList; 
    public static int step = -1;
    public static int sortingNum;
    private bool breakStep = false;
    private FillSorting fillSorting;
    WaitForSeconds shortWait = new WaitForSeconds(1f);
    WaitForSeconds longWait = new WaitForSeconds(2f);

    /*
    
        problemNum
            0 : WhatSorting
            1 : FillSorting
            2 : HowManySteps

    */

    void OnEnable() {
        step = -1;
        GetPosition();
        RandomList();
        SetPosition();
        StartCoroutine(BeginProblem());
    }

    IEnumerator BeginProblem() {
        yield return longWait;
        guide.SetActive(true);
        yield return longWait;
        guide.SetActive(false);
        yield return shortWait;
        afterDishes.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        beforeDishes.SetActive(true);
        yield return shortWait;
        divider.SetActive(true);
        yield return longWait;
        DoSort();
    }

    void RandomList() {
        randomList = new List<int>();

        while (randomList.Count < 6) {
            int ranNum = UnityEngine.Random.Range(0,6);
        
            if (!randomList.Contains(ranNum)) {
                randomList.Add(ranNum);
            }
        }
        beforeDishesText.text = String.Format("{0}          {1}          {2}          {3}          {4}          {5}", randomList[0]+1, randomList[1]+1, randomList[2]+1, randomList[3]+1, randomList[4]+1, randomList[5]+1);
    }
    
    void GetPosition() {
        positionList = new List<float> ();

        foreach (GameObject dish in dishes) {
            RectTransform dishTransform = dish.GetComponent<RectTransform>();
            float dishPosition = dishTransform.anchoredPosition.x;
            positionList.Add(dishPosition);
        }
    }

    void SetPosition() {
        for (int i=0; i<6; i++) {
            RectTransform dishTransform = dishes[randomList[i]].GetComponent<RectTransform>();
            Vector2 modifyPosition = dishTransform.anchoredPosition;
            modifyPosition.x = positionList[i];
            dishTransform.anchoredPosition = modifyPosition;
        }
    }

    void AfterSort() {
        if (problemNum == 1) {
            fillSorting = option.GetComponent<FillSorting>();
            option.SetActive(true);
            fillSorting.SetOption();
        }
        else {
            option.SetActive(true);
        }
    }

    void DoSort() {
        sortingNum = UnityEngine.Random.Range(0,3);

        if (problemNum == 1) {
            step = UnityEngine.Random.Range(2,5);
        }
        else if (problemNum == 2) {
            step = UnityEngine.Random.Range(1,6);
        }

        switch (sortingNum) {
            case 0:
                StartCoroutine(BubbleSort(step));
                break;
            case 1:
                StartCoroutine(InsertionSort(step));
                break;
            case 2:
                StartCoroutine(SelectionSort(step));
                break;
            default:
                break;
        }
    }

    void AfterStep() {
        List<GameObject> copyDishes = new List<GameObject>(dishes);
        randomDishes = new List<GameObject>();

        int randomIndex = FillSorting.randomIndex;
        answerDishes = (randomList[randomIndex], randomList[randomIndex+1]);

        randomDishes.AddRange(copyDishes);
    }

    IEnumerator BubbleSort(int step = -1) {
        Debug.Log("bubble");

        int count = 0;
        
        if (problemNum == 1) {
            yield return shortWait;
        }

        for (int index=randomList.Count-1; index>0; index--) {
            // Debug.Log($"index : {index}");

            bool changed = false;
            for (int i=0; i<index; i++) {
                // Debug.Log($"i: {i}");

                if (randomList[i] > randomList[i+1]) {
                    // Debug.Log($"{randomList[i]} / {randomList[i+1]}");
                    RectTransform dishTransform1 = dishes[randomList[i]].GetComponent<RectTransform>();
                    float modifyPosition1 = dishTransform1.anchoredPosition.x;
                    RectTransform dishTransform2 = dishes[randomList[i+1]].GetComponent<RectTransform>();
                    float modifyPosition2 = dishTransform2.anchoredPosition.x;

                    // Debug.Log($"{positionList[randomList[i+1]]} / {positionList[randomList[i]]}");
                    
                    float duration = 0.2f; // 이동에 걸리는 시간

                    dishTransform1.DOLocalMoveX(modifyPosition2, duration);
                    dishTransform2.DOLocalMoveX(modifyPosition1, duration);

                    int temp = randomList[i];
                    randomList[i] = randomList[i+1];
                    randomList[i+1] = temp;

                    changed = true;
                    yield return shortWait;
                }
            }

            if (problemNum == 2) {
                RectTransform dishesTransform = afterDishes.GetComponent<RectTransform>();
                yield return new WaitForSeconds(0.1f);
                dishesTransform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);
                dishesTransform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);

                count++;
            }

            if (changed == false) {
                break;
            }

            if (step != -1 && randomList.Count - index == step) {
                if (problemNum == 1) {
                    AfterStep();
                }
                else if (problemNum == 2) {
                    breakStep = true;
                }

                break;
            }
        }
        if (!breakStep) {
            if (problemNum == 1) {
                AfterStep();
            }
            else if (problemNum == 2) {
                step = count;
            } 
        }
        AfterSort();
    }

    IEnumerator InsertionSort(int step = -1) {
        Debug.Log("insertion");

        int count = 0;
        float duration = 0.1f; // 이동에 걸리는 시간

        int index = randomList.Count;
        // Debug.Log($"index : {index}");

        for (int i=1; i<index; i++) {
            // Debug.Log($"i: {i}");
            int key = randomList[i];
            RectTransform dishTransform = dishes[randomList[i]].GetComponent<RectTransform>();
            float modifyPosition = dishTransform.anchoredPosition.y;
            dishTransform.DOLocalMoveY(modifyPosition - 200, duration);
            yield return new WaitForSeconds(0.3f);

            int j = i - 1;

            while (j >= 0 && randomList[j] > key) {
                // Debug.Log($"{randomList[i]} / {randomList[i+1]}");
                RectTransform dishTransform1 = dishes[randomList[j]].GetComponent<RectTransform>();
                float modifyPosition1 = dishTransform1.anchoredPosition.x;
                RectTransform dishTransform2 = dishes[randomList[j+1]].GetComponent<RectTransform>();
                float modifyPosition2 = dishTransform2.anchoredPosition.x;

                // Debug.Log($"{positionList[randomList[i+1]]} / {positionList[randomList[i]]}");

                dishTransform1.DOLocalMoveX(modifyPosition2, duration);
                dishTransform2.DOLocalMoveX(modifyPosition1, duration);


                int temp = randomList[j];
                randomList[j] = randomList[j+1];
                randomList[j+1] = temp;
                j--;

                yield return new WaitForSeconds(0.2f);
            }
            randomList[j+1] = key;
            RectTransform dishTransform3 = dishes[randomList[j+1]].GetComponent<RectTransform>();
            Vector2 modifyPosition3 = dishTransform3.anchoredPosition;
            dishTransform.DOLocalMove(new Vector3(modifyPosition3.x, modifyPosition, 0), duration);

            if (problemNum == 2) {
                RectTransform dishesTransform = afterDishes.GetComponent<RectTransform>();
                yield return new WaitForSeconds(0.1f);
                dishesTransform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);
                dishesTransform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.1f);

                count++;
            }

            if (step != -1 && i == step) {
                if (problemNum == 1) {
                    AfterStep();
                }
                else if (problemNum == 2) {
                    breakStep = true;
                }
                break;
            }

            yield return shortWait;
        }

        if (!breakStep) {
            if (problemNum == 1) {
                AfterStep();
            }
            else if (problemNum == 2) {
                step = count;
            } 
        }

        AfterSort();
    }


    IEnumerator SelectionSort(int step = -1) {
        int index = randomList.Count;
        float duration = 0.4f; // 이동에 걸리는 시간
        int count = 0;

        for (int i=0; i<index; i++) {
            // Debug.Log($"i: {i}");
            int least = i;

            for (int j=i+1; j<index; j++) {
                GameObject dish = dishes[randomList[j]].transform.GetChild(0).gameObject;
                Image dishImage = dish.GetComponent<Image>();
                dishImage.color = Color.grey;

                yield return new WaitForSeconds(0.2f);

                dishImage.color = Color.white;

                if (randomList[j] < randomList[least]) {
                    least = j;
                    RectTransform dishTransform = dishes[randomList[least]].GetComponent<RectTransform>();
                    float modifyPosition = dishTransform.anchoredPosition.y;
                    dishTransform.DOLocalMoveY(modifyPosition - 50, duration);
                    yield return new WaitForSeconds(0.1f);
                    dishTransform.DOLocalMoveY(modifyPosition, duration);
                }
            }
            
            // Debug.Log($"{randomList[i]} / {randomList[i+1]}");
            RectTransform dishTransform1 = dishes[randomList[i]].GetComponent<RectTransform>();
            float modifyPosition1 = dishTransform1.anchoredPosition.x;
            RectTransform dishTransform2 = dishes[randomList[least]].GetComponent<RectTransform>();
            float modifyPosition2 = dishTransform2.anchoredPosition.x;

            // Debug.Log($"{positionList[randomList[i+1]]} / {positionList[randomList[i]]}");

            dishTransform1.DOLocalMoveX(modifyPosition2, duration);
            dishTransform2.DOLocalMoveX(modifyPosition1, duration);

            int temp = randomList[i];
            randomList[i] = randomList[least];
            randomList[least] = temp;

            if (problemNum == 2) {
                RectTransform dishesTransform = afterDishes.GetComponent<RectTransform>();
                yield return new WaitForSeconds(0.2f);
                dishesTransform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);
                dishesTransform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);

                count++;
            }

            if (step != -1 && i + 1 == step) {
                if (problemNum == 1) {
                    AfterStep();
                }
                else if (problemNum == 2) {
                    breakStep = true;
                }

                break;
            }

            yield return shortWait;
        }
        
        if (!breakStep) {
            if (problemNum == 1) {
                AfterStep();
            }
            else if (problemNum == 2) {
                step = count;
            } 
        }
        AfterSort();
    }
}
