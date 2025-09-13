using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreeWhatOrder : TreeProblem
{
    [SerializeField] private GameObject guide, tree, option, question;
    [SerializeField] private List<GameObject> treeOptions;
    [SerializeField] private List<Sprite> optionContent;
    private List<int> optionIndex;
    public List<TNode> getNode;
    private List<GameObject> orderResult;
    private int answerIndex, orderNum;
    System.Random random = new System.Random();
    WaitForSeconds shortWait = new WaitForSeconds(1f);
    WaitForSeconds longWait = new WaitForSeconds(2f);

    /*
        OrderOption() : 선택지 랜덤 생성
        ShowResult() : 순회 결과 출력 및 트리에 표시
        OrderMain() : 랜덤 종류의 순회 실행

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

    void OrderOption() {
        // optionContent 순서 : "전위순회", "중위순회", "후위순회", "레벨순회"
        optionIndex = new List<int>();

        while (true) {
            int num = random.Next(optionContent.Count);

            if (!optionIndex.Contains(num)) {
                optionIndex.Add(num);
            }

            if (optionIndex.Count == optionContent.Count) {
                break;
            }
        }

        string[] orderNames = { "Preorder", "Inorder", "Postorder", "Levelorder" };

        int j = 0;
        foreach (Sprite content in optionContent) {
            Image optionImage = treeOptions[optionIndex[j]].GetComponent<Image>();
            //optionImage.sprite = content;
            optionImage.sprite = LocalizationManager.instance.ReturnTranslatedImage(orderNames[j]);
            j++;
            if (j >= optionContent.Count) {
                break;
            }
        }

    }

    IEnumerator ShowResult() {
        for (int i=0; i<orderResult.Count; i++) {
            GameObject res = question.transform.GetChild(i).gameObject;
            Image resImage = res.GetComponent<Image>();
            Image orderImage = orderResult[i].GetComponent<Image>();
            Color ordColor = orderImage.color;
            ordColor.a = 0.5f;

            resImage.sprite = orderImage.sprite;
            res.SetActive(true);
            orderImage.color = ordColor;

            yield return new WaitForSeconds(0.4f);
        }
        yield return new WaitForSeconds(0.7f);
        option.SetActive(true);
        yield return shortWait;
        AnswerManager.instance.SetProblemAnswer(answerIndex);
    }

    void OrderMain() {
        TreeProblem tpScript = tree.GetComponent<TreeProblem>();
        orderResult = new List<GameObject>();
        getNode = tpScript.node;
        // Debug.Log(getNode.Count);

        int last = getNode.Count - 1;
        TNode root = getNode[last];

        //if (tpScript.generateCount < 3) {
        //    orderNum = UnityEngine.Random.Range(1,3);
        //}
        //else {
        orderNum = random.Next(4);
        //}
        // int orderNum = 3; //테스트용

        switch(orderNum) {
            case 0:
                Debug.Log("PreOrder");
                PreOrder(root);
                break;

            case 1:
                Debug.Log("InOrder");
                InOrder(root);
                break;
            
            case 2:
                Debug.Log("PostOrder");
                PostOrder(root);
                break;

            case 3:
                Debug.Log("LevelOrder");
                LevelOrder(getNode, root);
                break;
        }

        answerIndex = optionIndex[orderNum];
        Debug.Log($"정답 인덱스 : {(AnswerButton) answerIndex}");

        // Debug.Log(orderResult.Count);
        StartCoroutine(ShowResult());
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

        // Debug.Log("getN" + getN.Count);
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
