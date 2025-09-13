using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FillSorting : MonoBehaviour
{
    private ProblemManager pm;
    [SerializeField] private GameObject blank;
    [SerializeField] public List<GameObject> options;
    public static int randomIndex;
    private List<int> optionIndex;
    private List<GameObject> instantiatedDishes;
    private bool isTweening = false;

    void OnEnable() {
        pm = ProblemManager.instance;
        instantiatedDishes = new List<GameObject>();
        StartCoroutine(BeginProblem());
    }

    void Update() {
        if (pm.isShowingAnswer && !isTweening) {
            RectTransform blankTransform = blank.GetComponent<RectTransform>();
            blankTransform.DOScale(0f, 0.4f)
                .SetEase(Ease.OutBack)
                .OnStart(() => isTweening = true)
                .OnComplete(() =>
                {
                    blank.SetActive(false);
                    isTweening = false;
                });
        }
    }

    void OnDisable() {
        foreach (GameObject insDish in instantiatedDishes) {
            Destroy(insDish);
        }
    }

    IEnumerator BeginProblem() {
        yield return new WaitForSeconds(8f);
        SetBlank();
        blank.SetActive(true);
    }
    void SetBlank()
    {
        randomIndex = UnityEngine.Random.Range(0,5);
        List<float> dishPositions = SortingProblem.positionList;

        RectTransform blankTransform = blank.GetComponent<RectTransform>();
        Vector2 modifyPosition = new Vector2(dishPositions[randomIndex] + 72.44f, blankTransform.anchoredPosition.y);
        blankTransform.anchoredPosition = modifyPosition;
    }

    List<int> RandomList(int len, int start, int end) {
        List<int> randomList = new List<int>();

        while (randomList.Count<len) {
            int randomNum = UnityEngine.Random.Range(start,end);

            if (!randomList.Contains(randomNum)) {
                randomList.Add(randomNum);
            }
        }

        return randomList;
    }
    
    void SetInstantiate(int index, GameObject dish1, GameObject dish2) {
        Vector2 position1 = new Vector2(-75, 0);
        Vector2 position2 = new Vector2(75, 0);
        Vector3 defaltPosition = new Vector3(0.9f,0.9f,0);
        Transform parent = options[index].GetComponent<Transform>();

        GameObject instantiatedDish1 = Instantiate(dish1, defaltPosition, Quaternion.identity, parent);
        GameObject instantiatedDish2 = Instantiate(dish2, defaltPosition, Quaternion.identity, parent);
        
        RectTransform rectInsDish1 = instantiatedDish1.GetComponent<RectTransform>();
        RectTransform rectInsDish2 = instantiatedDish2.GetComponent<RectTransform>();
        rectInsDish1.anchoredPosition = position1;
        rectInsDish2.anchoredPosition = position2;

        rectInsDish1.localScale = defaltPosition;
        rectInsDish2.localScale = defaltPosition;
        rectInsDish1.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        rectInsDish2.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        instantiatedDishes.Add(instantiatedDish1);
        instantiatedDishes.Add(instantiatedDish2);
    }

    public bool FindTuple(List<(int, int)> tupleList, (int, int) targetTuple)
    {
        foreach (var tuple in tupleList)
        {
            if (tuple.Item1 == targetTuple.Item1 && tuple.Item2 == targetTuple.Item2)
            {
                return true;
            }
        }

        return false;
    }

    public void SetOption() {
        optionIndex = RandomList(4, 0, 4);

        List<GameObject> optionDishes = new List<GameObject>(SortingProblem.randomDishes);
        (int, int) answerDishes = SortingProblem.answerDishes;

        int answerIndex = optionIndex[0];
        SetInstantiate(answerIndex, optionDishes[answerDishes.Item1], optionDishes[answerDishes.Item2]);

        // 만약에 답에서 순서만 바꾼 선택지를 만들고 싶다면 주석 해제
        // answerIndex = optionIndex[1];
        // SetInstantiate(answerIndex, optionDishes[1], optionDishes[0]);

        List<(int,int)> fillOptions = new List<(int, int)>();
        fillOptions.Add(answerDishes);
        while (fillOptions.Count < 4) {    
            
            List<int> listToTuple = RandomList(2, 0, optionDishes.Count);
            (int, int) tupleIndex = (listToTuple[0], listToTuple[1]);

            bool foundTuple = FindTuple(fillOptions, tupleIndex);
            if (!foundTuple) {
                fillOptions.Add(tupleIndex);
            }
        }

        for (int i=1; i<4; i++) {
            int odIndex1 = fillOptions[i].Item1;
            int odIndex2 = fillOptions[i].Item2;
            int index = optionIndex[i];
            SetInstantiate(index, optionDishes[odIndex1], optionDishes[odIndex2]);
        }

        AnswerManager.instance.SetProblemAnswer(answerIndex);
        Debug.Log($"정답 인덱스 : {(AnswerButton) answerIndex}");
    }
}
