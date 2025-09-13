using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HowManySteps : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> contents;
    private List<int> randomSteps, randomOptions;
    private int step;
    
    void OnEnable() {
        RandomIndex();
        SetStep();
        SetOption();
    }

    void SetStep() {
        randomSteps = new List<int>();

        step = SortingProblem.step;
        randomSteps.Add(step);

        while (randomSteps.Count < 4) {
            int ranNum = UnityEngine.Random.Range(1,7);

            if (!randomSteps.Contains(ranNum)) {
                randomSteps.Add(ranNum);
            }
        }
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
        int answerIndex = randomOptions[0];
        contents[answerIndex].text = randomSteps[0].ToString();
        
        for (int i=1; i<randomOptions.Count; i++) {
            contents[randomOptions[i]].text = randomSteps[i].ToString();
        }
        
        AnswerManager.instance.SetProblemAnswer(answerIndex);
        Debug.Log($"정답 인덱스 : {(AnswerButton) answerIndex}");
    }
}
