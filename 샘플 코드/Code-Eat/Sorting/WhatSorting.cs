using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WhatSorting : MonoBehaviour
{
    [SerializeField] private GameObject people, afterDishes;
    [SerializeField] private List<TMP_Text> contents;
    [SerializeField] private List<int> randomOptions;
    private List<string> sortType;
    private int answerIndex;
    WaitForSeconds shortWait = new WaitForSeconds(1f);

    void OnEnable() {
        StartCoroutine(PeopleGetDishes());
        RandomIndex();
        SetOption();
    }

    void RandomIndex() {
        randomOptions = new List<int>();

        while (randomOptions.Count < 4) {
            int ranNum = UnityEngine.Random.Range(0,4);

            if (!randomOptions.Contains(ranNum)) {
                randomOptions.Add(ranNum);
            }
        }
    }

    void SetOption() {
        //sortType = new List<string>() {"버블\n정렬", "삽입\n정렬", "선택\n정렬"};
        //List<string> otherType = new List<string>() {"힙\n정렬", "합병\n정렬", "퀵\n정렬", "힙\n정렬", "쉘\n정렬", "기수\n정렬"};

        string[] sort_type = LocalizationManager.instance.ReturnTranslatedText("AnswerSort").Split('@');
        
        sortType = sort_type[0].Split(',').ToList();
        List<string> otherType = LocalizationManager.instance.ReturnTranslatedText("OtherSort").Split(',').ToList();

        int ranIdx = UnityEngine.Random.Range(0,otherType.Count);
        sortType.Add(otherType[ranIdx]);

        for (int i=0; i<randomOptions.Count; i++) {
            contents[i].text = sortType[randomOptions[i]];

            if (sort_type.Length > 1)
                contents[i].fontSize = int.Parse(sort_type[1]);
        }

        int answerNum = SortingProblem.sortingNum;
        Debug.Log($"answerNum : {answerNum}");
        answerIndex = randomOptions.IndexOf(answerNum);
    }

    IEnumerator PeopleGetDishes() {
        people.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        RectTransform dishTransform = afterDishes.GetComponent<RectTransform>();
        float modifyPosition = dishTransform.anchoredPosition.y;
        dishTransform.DOLocalMoveY(modifyPosition - 200, 0.8f);

        yield return shortWait;

        RectTransform peopleTransform = people.GetComponent<RectTransform>();
        float modifyPositionP = peopleTransform.anchoredPosition.y;
        peopleTransform.DOLocalMoveY(modifyPositionP - 50, 0.4f);
        dishTransform.DOLocalMoveY(modifyPosition - 250, 0.4f);

        AnswerManager.instance.SetProblemAnswer(answerIndex);
        Debug.Log($"정답 인덱스 : {(AnswerButton) answerIndex}");
        
        yield return new WaitForSeconds(0.5f);
        people.SetActive(false);
        afterDishes.SetActive(false);
    }
}
