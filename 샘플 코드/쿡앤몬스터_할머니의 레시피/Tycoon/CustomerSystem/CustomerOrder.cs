using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    public static CustomerOrder Instance { get; private set; }

    private readonly float[][] _orderRate = new float[9][]
    {
        // 3개 선택한 경우
        new float[3] {   0.4f    ,   0.35f   ,   0.25f    },     // 일반 1, 고급 1, 전설 1
        new float[3] {   0.35f   ,   0.35f   ,   0.3f     },     // 일반 2, 고급 1
        new float[3] {   0.5f    ,   0.25f   ,   0.25f    },     // 일반 1, 고급 2
        new float[3] {   0.375f  ,   0.375f  ,   0.25f    },     // 일반 2, 전설 1
        new float[3] {   1f/3f   ,   1f/3f   ,   1f/3f    },     // 일반 3

        // 2개 선택한 경우
        new float[3] {   0.5f    ,   0.5f    ,   0f       },     // 일반 2
        new float[3] {   0.7f    ,   0.3f    ,   0f       },     // 일반 1, 고급 1
        new float[3] {   0.75f   ,   0.25f   ,   0f       },     // 일반 1, 전설 1

        // 1개 선택한 경우
        new float[3] {   1f      ,   0f      ,   0f       }
    };

    private List<int> _selectedRecipes;
    private List<int> sortedRecipes;


    private void Awake()
    {
        Instance = this;
    }


    public int GetOrderRecipe()
    {
        int idx = GetRecipeRates();
        int n = _orderRate[idx].Length;
        Debug.Log($"_orderRate = [{_orderRate[idx][0]}, {_orderRate[idx][1]}, {_orderRate[idx][2]}]");

        float rand = Random.value;
        Debug.Log($"CustomerOrder : rand = {rand}");
        float cumulative = 0f;

        for (int i = 0; i < n; i++)
        {
            cumulative += _orderRate[idx][i];
            if (rand <= cumulative) return sortedRecipes[i];
        }
        
        return sortedRecipes[n - 1];
    }


    public int GetRecipeRates()
    {
        _selectedRecipes = Managers.Data.User.SelectRecipes;

        // 등급별 초기 세팅
        int[] gradeCount = { 0, 0, 0 };
        sortedRecipes = new List<int>();

        for (int i = 0; i < _selectedRecipes.Count; i++)
        {
            int selectedId = _selectedRecipes[i];

            // 선택하지 않았으면 패스
            if (selectedId == -1) continue;

            int grade = (int)Managers.Data.Recipes.GetByKey(selectedId).Grade;
            gradeCount[grade]++;
            sortedRecipes.Add(selectedId);
        }

        // 등급 순으로 정렬
        sortedRecipes = sortedRecipes.OrderBy(x => (int?)(Managers.Data.Recipes.GetByKey(x)?.Grade) ?? int.MaxValue).ToList();

        /*
            일반 : gradeCount[0]
            고급 : gradeCount[1]
            전설 : gradeCount[2]
        */

        // 일반 1, 고급 1, 전설 1
        if (gradeCount[0] == 1 && gradeCount[1] == 1 && gradeCount[2] == 1)
        {
            return 0;
        }

        // 일반 2, 고급 1
        else if (gradeCount[0] == 2 && gradeCount[1] == 1)
        {
            return 1;
        }

        // 일반 1, 고급 2
        else if (gradeCount[0] == 1 && gradeCount[1] == 2)
        {
            return 2;
        }

        // 일반 2, 전설 1
        else if (gradeCount[0] == 2 && gradeCount[2] == 1)
        {
            return 3;
        }

        // 일반 3
        else if (gradeCount[0] == 3)
        {
            return 4;
        }

        // 일반 2
        else if (gradeCount[0] == 2)
        {
            return 5;
        }

        // 일반 1, 고급 1
        else if (gradeCount[0] == 1 && gradeCount[1] == 1)
        {
            return 6;
        }

        // 일반 1, 전설 1
        else if (gradeCount[0] == 1 && gradeCount[2] == 1)
        {
            return 7;
        }

        // 1개 선택
        else
        {
            return 8;
        }
    }
}
