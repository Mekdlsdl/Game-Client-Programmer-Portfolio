using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreeFamily : MonoBehaviour
{
    [SerializeField] private GameObject guide, tree, option, question;
    [SerializeField] private List<GameObject> treeOptions, treeNames;
    private List<int> ableNode, selectedProblem;
    private List<int> optionIndex = new List<int>();
    private List<int> [] answerNodes = new List<int>[3];
    private bool [] ableProblem = new bool[3];
    public TMP_Text problem;
    private int answerIndex, answerNode, nodeNum, problemNum;
    private Image ansImage;
    WaitForSeconds shortWait = new WaitForSeconds(1f);
    WaitForSeconds midWait = new WaitForSeconds(1.6f);
    WaitForSeconds longWait = new WaitForSeconds(2f);

    /*
        SetNodeNum() : 대상 노드 선택
        SetProblemNum() : 가능한 문제 선택 (부모, 자식, 형제 중)
        CalculateNode() : 노드 계산, 문제와 정답 세팅
        GenerateOptions() : 선택지 생성
        OrderOption() : 선택지 인덱스 랜덤 세팅
        ShowResult() : 문제 출력
    */

    /*
        - 부모노드
            따로 처리 불필요

        - 자식노드
            활성화 확인 (해당 노드의 자식노드가 있는지)

        - 형제노드
            활성화 확인 (해당 노드의 형제노드가 있는지)
            왼쪽 자식노드일 때는 +1
            오른쪽 자식노드일 때는 -1
    */

    void OnEnable()
    {
        // 최소 트리 노드 개수 세팅
        // 선택지에서 대상 노드와 정답 후보 노드는 제외해야 하므로 최소한 5개의 노드 필요
        // tpScript = tree.GetComponent<TreeProblem>();
        // tpScript.generateMin = 2;
        
        StartCoroutine(BeginProblem());
    }

    IEnumerator BeginProblem()
    {
        yield return longWait;
        guide.SetActive(true);
        yield return longWait;
        guide.SetActive(false);
        yield return shortWait;
        tree.SetActive(true);
        yield return shortWait;
        CalculateNode();
        OrderOption();
        ShowResult();
        question.SetActive(true);
        GenerateOptions();
        yield return midWait;
        option.SetActive(true);
        AnswerManager.instance.SetProblemAnswer(answerIndex);
        Debug.Log($"정답 인덱스 : {(AnswerButton) answerIndex}");
    }

    private int SetNodeNum() {
        nodeNum = -1;

        while (nodeNum == -1) {
            nodeNum = UnityEngine.Random.Range(0,7);
            
            if (!treeNames[nodeNum].activeSelf) {
                // Debug.Log("nodeNum ? : " + nodeNum);
                nodeNum = -1;
            }
        }
        // Debug.Log("nodeNum : " + nodeNum);
        return nodeNum;
    }

    private int SetProblemNum() {
        problemNum = -1;
        // problemNum = 1; //테스트용

        while (problemNum == -1) {
            problemNum = UnityEngine.Random.Range(0,3);

            if (!ableProblem[problemNum]) {
                problemNum = -1;
            }
        }
        // Debug.Log("problemNum : " + problemNum);
        return problemNum;
    }

    /*
    
    - nodeNum 기준
       1
     2   3
    4 5 6 7

    - 실제 인덱스
       0
     1   2
    3 4 5 6

    */

    void CalculateNode() {
        // tpScript = tree.GetComponent<TreeProblem>();
        // treeNames = tpScript.treeName;
        // treeNames = TreeProblem.treeName;

        ableProblem = new bool [3];

        nodeNum = SetNodeNum();
        // Debug.Log("nodeNum : " + nodeNum);
        // Debug.Log("treeNames.Count : " + treeNames.Count);
        nodeNum++;
        answerNodes = new List<int>[3];
        
        if (nodeNum > 1) {
            // 부모노드
            ableNode = new List<int>();

            ableNode.Add(nodeNum / 2);
            ableProblem[0] = true;

            answerNodes[0] = ableNode;


            // 형제노드
            ableNode = new List<int>();

            // 왼쪽 노드일 경우
            if (nodeNum % 2 == 0) {
                if (treeNames[nodeNum].activeSelf) {
                    ableNode.Add(nodeNum + 1);
                    ableProblem[2] = true;
                }
            }
            // 오른쪽 노드일 경우
            else {
                if (treeNames[nodeNum - 2].activeSelf) {
                    ableNode.Add(nodeNum - 1);
                    ableProblem[2] = true;
                }
            }
            answerNodes[2] = ableNode;
        }
        
        // 자식노드 (둘다 활성화되어있다면 아무거나 가능)
        if (nodeNum < 4) {
            ableNode = new List<int>();

            // 왼쪽 자식노드
            if (treeNames[(nodeNum * 2) - 1].activeSelf) {
                ableNode.Add(nodeNum * 2);
                ableProblem[1] = true;
            }
            // 오른쪽 자식노드
            if (treeNames[nodeNum * 2].activeSelf) {
                ableNode.Add((nodeNum * 2) + 1);
                ableProblem[1] = true;
            }

            // if (ableNode.Count == 2 && treeProblem.generateCount == 2) {
            //     // Debug.Log("tpScript.generateCount : " + tpScript.generateCount);
            //     ableProblem[1] = false;
            // }

            if (ableProblem[1]) {
                answerNodes[1] = ableNode;
            }
        }

        problemNum = SetProblemNum();
        // Debug.Log("problemNum : " + problemNum);

        selectedProblem = answerNodes[problemNum];
        int num = UnityEngine.Random.Range(0,selectedProblem.Count);
        answerNode = selectedProblem[num];
        // Debug.Log("answerNode : " + answerNode);
        nodeNum--;
    }

    void GenerateOptions() {
        // 정답 먼저 넣기

        answerIndex = optionIndex[0];
        GameObject op = treeOptions[answerIndex];
        Image opImage = op.GetComponent<Image>();
        ansImage = treeNames[answerNode - 1].GetComponent<Image>();

        opImage.sprite = ansImage.sprite;


        // 랜덤 리스트를 생성하여 나머지 선택지 채우기
        List<int> selectedRan = new List<int>();

        while (selectedRan.Count < 3) {
            int randomAns = UnityEngine.Random.Range(0,7);
            if (selectedRan.Contains(randomAns) || selectedProblem.Contains(randomAns + 1)) {
                continue;
            }
            else if (randomAns == nodeNum || !treeNames[randomAns].activeSelf) {
                continue;
            }
            else {
                selectedRan.Add(randomAns);
            }
        }
        // Debug.Log("selectedRan.Count : " + selectedRan.Count);
        // Debug.Log("selectedRan : " + selectedRan[0] + selectedRan[1] + selectedRan[2]);

        for (int o=1; o<optionIndex.Count; o++) {
            // Debug.Log("optionIndex : " + o);
            GameObject opT = treeOptions[optionIndex[o]];
            Image opTImage = opT.GetComponent<Image>();
            ansImage = treeNames[selectedRan[o-1]].GetComponent<Image>();
            // Debug.Log("answerResult : " + selectedRan[o-1]);

            opTImage.sprite = ansImage.sprite;
        }    
    }

    void OrderOption() {
        optionIndex = new List<int>();

        while (true) {
            int num = UnityEngine.Random.Range(0,treeOptions.Count);

            if (!optionIndex.Contains(num)) {
                optionIndex.Add(num);
            }

            if (optionIndex.Count == treeOptions.Count) {
                break;
            }
        }
    }

    void ShowResult() {
        // List<string> problems = new List<string> {"부모", "자식", "형제"};
        string[] problems = LocalizationManager.instance.ReturnTranslatedText("TreeFamily_3").Split(',');

        // Debug.Log("treeNames.Count : " + treeNames.Count);
        GameObject node = treeNames[nodeNum].gameObject;
        GameObject problemNode = question.transform.GetChild(1).gameObject;
        Image problemNodeImage = problemNode.GetComponent<Image>();
        Image nodeImage = node.GetComponent<Image>();

        // problem.text = String.Format("의 {0} 노드는?", problems[problemNum]);

        string[] texts = LocalizationManager.instance.ReturnTranslatedText("TreeFamily_2").Split('@');
        string problemText = texts[0];
        problem.text = Regex.Replace(problemText, "#", problems[problemNum]);
        
        if (texts.Length > 1)
            problem.fontSize = int.Parse(texts[1]);

        problemNodeImage.sprite = nodeImage.sprite;
    }
}