using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreeFillOrder : TreeProblem
{
    [SerializeField] private GameObject guide, tree, option, question;
    [SerializeField] private List<GameObject> treeOptions;
    private List<int> optionIndex;
    public List<TNode> getNode;
    private List<GameObject> orderResult, answerResult;
    public TMP_Text orderType;
    private int answerIndex;
    System.Random random = new System.Random();
    WaitForSeconds shortWait = new WaitForSeconds(1f);
    WaitForSeconds longWait = new WaitForSeconds(2f);

    /*
        OrderOption() : 선택지 인덱스 랜덤 세팅
        ShowResult() : 순회 결과 출력 및 트리에 표시
        OrderMain() : 랜덤 종류의 순회 실행
        GenerateOptions() : 선택지 생성

            PreOrder() : 전위순회
            InOrder() : 중위순회
            PostOrder() : 후위순회
            LevelOrder() : 레벨순회
    
    */

    void OnEnable()
    {
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
        OrderOption();
        OrderMain();
        question.SetActive(true);
    }

    void GenerateOptions() {
        int [][] indexs = {
            new int [] {0,2,1},
            new int [] {1,0,2},
            new int [] {1,2,0},
            new int [] {2,0,1},
            new int [] {2,1,0}
        };

        // 정답 먼저 넣기
        answerIndex = optionIndex[0];
        // Debug.Log("answerResult.Count : " + answerResult.Count);
        for (int i=0; i<answerResult.Count; i++) {
            GameObject op = treeOptions[answerIndex].transform.GetChild(i).gameObject;
            Image opImage = op.GetComponent<Image>();
            Image ansImage = answerResult[i].GetComponent<Image>();

            opImage.sprite = ansImage.sprite;
        }

        // 랜덤 리스트를 생성하여 나머지 선택지 채우기
        List<int> selectedRan = new List<int>();

        while (selectedRan.Count < 3) {
            int randomAns = random.Next(5);
            if (!selectedRan.Contains(randomAns)) {
                selectedRan.Add(randomAns);
            }
        }
        // Debug.Log("selectedRan.Count : " + selectedRan.Count);
        // Debug.Log("selectedRan : " + selectedRan[0] + selectedRan[1] + selectedRan[2]);

        for (int o=1; o<optionIndex.Count; o++) {
            // Debug.Log("optionIndex : " + o);
            int[] resSeq = indexs[o];
            // Debug.Log("resSeq : " + resSeq[0] + resSeq[1] + resSeq[2]);
            for (int j=0; j<3; j++) {
                GameObject op = treeOptions[optionIndex[o]].transform.GetChild(j).gameObject;
                Image opImage = op.GetComponent<Image>();
                Image ansImage = answerResult[resSeq[j]].GetComponent<Image>();
                // Debug.Log("GetChild : " + j + ", answerResult : " + resSeq[j]);

                opImage.sprite = ansImage.sprite;
            }
        }    
    }

    void OrderOption() {
        optionIndex = new List<int>();

        while (true) {
            int num = random.Next(treeOptions.Count);

            if (!optionIndex.Contains(num)) {
                optionIndex.Add(num);
            }

            if (optionIndex.Count == treeOptions.Count) {
                break;
            }
        }
    }

    IEnumerator ShowResult() {
        int blankIndex = random.Next(orderResult.Count - 3);
        answerResult = new List<GameObject>();

        yield return new WaitForSeconds(0.4f);

        for (int i=0; i<orderResult.Count; i++) {
            GameObject res = question.transform.GetChild(i).gameObject;
            Image resImage = res.GetComponent<Image>();
            Image orderImage = orderResult[i].GetComponent<Image>();
            Color ordColor = orderImage.color;
            ordColor.a = 0.5f;

            if (i == blankIndex || i == (blankIndex + 1) || i == (blankIndex + 2)) {
                answerResult.Add(orderResult[i]);
            }
            else {
                RectTransform rectTrans = res.GetComponent<RectTransform>();
                rectTrans.sizeDelta = new Vector2(55, 100);

                Vector2 currentPosition = rectTrans.anchoredPosition;
                currentPosition.y = currentPosition.y + 10;
                // Vector2 newPosition = new Vector2(transform.position.x, transform.position.y + 10);
                rectTrans.anchoredPosition = currentPosition;

                resImage.sprite = orderImage.sprite;
                orderImage.color = ordColor;
                
            }
            res.SetActive(true);

            yield return new WaitForSeconds(0.4f);
        }
        yield return new WaitForSeconds(0.7f);
        GenerateOptions();
        option.SetActive(true);
        // yield return shortWait;
        AnswerManager.instance.SetProblemAnswer(answerIndex);
        Debug.Log($"정답 인덱스 : {(AnswerButton) answerIndex}");
    }

    void OrderMain() {
        TreeProblem tpScript = tree.GetComponent<TreeProblem>();
        orderResult = new List<GameObject>();
        getNode = tpScript.node;
        // Debug.Log("getNode.Count : " + getNode.Count);

        int last = getNode.Count - 1;
        TNode root = getNode[last];

        int orderNum = random.Next(4);
        // int orderNum = 3; //테스트용

        switch(orderNum) {
            case 0:
                Debug.Log("PreOrder");
                SetText("FillOrder_Preorder");
                PreOrder(root);
                break;

            case 1:
                Debug.Log("InOrder");
                SetText("FillOrder_Inorder");
                InOrder(root);
                break;
            
            case 2:
                Debug.Log("PostOrder");
                SetText("FillOrder_Postorder");
                PostOrder(root);
                break;

            case 3:
                Debug.Log("LevelOrder");
                SetText("FillOrder_LevelOrder");
                LevelOrder(getNode, root);
                break;
        }

        // Debug.Log("orderResult.Count : " + orderResult.Count);
        StartCoroutine(ShowResult());
    }

    private void SetText(string tag)
    {
        string[] texts = LocalizationManager.instance.ReturnTranslatedText(tag).Split('@');
        orderType.text = texts[0];
        if (texts.Length > 1) orderType.fontSize = int.Parse(texts[1]);
    }

    void PostOrder(TNode n) {
        // Left - Right - Now

        if (n.Left is not null) {
            PostOrder(n.Left);
        }
        if (n.Right is not null) {
            PostOrder(n.Right);
        }
        if (n.Now is not null) {
            orderResult.Add(n.Now);
        }
    }

    void InOrder(TNode n) {
        // Left - Now - Right

        if (n.Left is not null) {
            InOrder(n.Left);
        }
        if (n.Now is not null) {
            orderResult.Add(n.Now);
        }
        if (n.Right is not null) {
            InOrder(n.Right);
        }
    }

    void PreOrder(TNode n) {
        // Now - Left - Right

        if (n.Now is not null) {
            orderResult.Add(n.Now);
        }
        if (n.Left is not null) {
            PreOrder(n.Left);
        }
        if (n.Right is not null) {
            PreOrder(n.Right);
        }
    }

    void LevelOrder(List<TNode> getN, TNode n) {
        // node 차례대로 출력

        // Debug.Log("getN.Count : " + getN.Count);
        List<TNode> getNodeL = getN;
        // Debug.Log(getNodeL.Count);

        int last = getNodeL.Count - 1;
        for (int k=last; k>-1; k--) {
            if (getNodeL[k] is not null) {
                orderResult.Add(getNodeL[k].Now);
            }
        }
    }
}