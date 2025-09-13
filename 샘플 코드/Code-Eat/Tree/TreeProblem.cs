using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TreeProblem : MonoBehaviour
{
    public List<bool> generateNode;
    public List<GameObject> treeNode, treeName;
    public List<TNode> node = new List<TNode>();
    public int generateMin, generateCount;
    System.Random random = new System.Random();

    void OnEnable() {
        RandomTree();
        node = Tree();
        Debug.Log("node.Count : " + node.Count);
    }

    void RandomTree() {
        generateCount = 0;
        List<bool> bools = new List<bool> {true, false};
        generateNode = new List<bool>();

        for (int i=0; i<treeNode.Count; i++) {
            int ranBool = random.Next(2);
            if (ranBool == 0) {
                generateCount ++;
                Debug.Log("generateCount: " + generateCount);
            }
            generateNode.Add(bools[ranBool]);
        }

        // 최소 노드 개수 설정값에 미치지 않으면 다시 생성
        if (generateCount < generateMin) {
            RandomTree();
        }
        else {
            for (int j=0; j<treeNode.Count; j++) {
            treeNode[j].SetActive(generateNode[j]);
            treeName[j+3].SetActive(generateNode[j]);
            }
        }
    }


    /*
        A -> B,C
        B -> D,E
        C -> F,G

        tNode = {Now, Left, Right}
    */

    public class TNode {
        public GameObject Now { get; set; }
        public TNode Left { get; set; }
        public TNode Right { get; set; }

        public TNode(GameObject now, TNode left, TNode right) {
            Now = now;
            Left = left;
            Right = right;
        }
    }

    /*

       A
     B   C
    D E F G

    */

    [SerializeField] public List<TNode> Tree() {
        // treeName = new List<GameObject>();
        // 역순으로 삽입
        Debug.Log("treeName.Count : " + treeName.Count);

        if (treeName.Count == 7) {
            // D, E, F, G
            for (int j=6; j>2; j--) {
                if (treeName[j].activeSelf) {
                    TNode tNode = new TNode(treeName[j], null, null);
                    node.Add(tNode);
                } else {
                    node.Add(null);
                    // node = {G, F, E, D}인 상태
                }
            }

            // C
            TNode tNodeC = new TNode(treeName[2], node[1], node[0]);
            node.Add(tNodeC);
            // node = {G, F, E, D, C}인 상태

            // B
            TNode tNodeB = new TNode(treeName[1], node[3], node[2]);
            node.Add(tNodeB);
            // node = {G, F, E, D, C, B}인 상태

            // A
            TNode tNodeA = new TNode(treeName[0], node[5], node[4]);
            node.Add(tNodeA);
            // node = {G, F, E, D, C, B}인 상태
        }

        return node;
    }
}
