using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * 아마도 SetUp 패널에 붙을 컨트롤러 스크립트
 */
public class SetUpController : MonoBehaviour
{
    public static SetUpController Instance { get; private set; }


    [Header("Defalt Guard Panel")]
    public GameObject GuardPanel;

    [Header("Cotroller")]
    [SerializeField] private UpgradeTabController _upgrade;
    [SerializeField] private RecipeTabController _recipe;
    [SerializeField] private MenuTabController _menu;

    [Header("CurStat Panel")]
    [SerializeField] private GameObject _curStatPanel;
    [SerializeField] private List<TMP_Text> _curStatFigures;

    [SerializeField] private Button _startButton;



    private void Awake()
    {
        Instance = this;
        // StartCoroutine(RefundUnlockRecipe());
    }

    // private void OnEnable()
    // {
    //     StartCoroutine(RefundUnlockRecipe());
    // }

    private void OnEnable()
    {
        Managers.Session.OnStageChaged -= RefreshStartBtn; // 치트용
        Managers.Session.OnStageChaged += RefreshStartBtn; // 치트용

        RefreshStartBtn();
        OpenRecipeNotice();

        StartCoroutine(RefundUnlockRecipe());
    }

    private void OnDisable()
    {
        Managers.Session.OnStageChaged -= RefreshStartBtn; // 치트용
    }


    /* 현재 스탯 보기 버튼 */
    public void ShowCurrentStat()
    {
        _curStatFigures[0].text = Managers.Data.User.MaxHp.ToString("N0");       // 생명력
        _curStatFigures[1].text = Managers.Data.User.Atk.ToString("N0");         // 탄환 데미지
        _curStatFigures[2].text = Managers.Data.User.MoveSpeed.ToString("N1");   // 이동 속도
        _curStatFigures[3].text = Managers.Data.User.AtkSpeed.ToString("N2");    // 공격 속도
        _curStatFigures[4].text = Managers.Data.User.BagSize.ToString("N0");     // 가방 크기
        _curStatFigures[5].text = $"{Managers.Data.User.Critical:P0}";          // 팁 증가 확률

        _curStatPanel.SetActive(true);
    }

    /* 게임 시작 버튼 */
    public void StageStart()
    {
        Debug.Log($"스테이지 시작 버튼 클릭");

        List<int> selectedRecipe = Managers.Data.User.SelectRecipes;

        // 환급 받은 적 없을 때만
        if (!Managers.Data.User.HadReceivedRefund[1])
        {
            bool hasExcluded = false;
            int refund = 0; // 환급금

            // 메뉴에서 뺄 게 있는지 확인
            for (int i = 0; i < selectedRecipe.Count; i++)
            {
                if (selectedRecipe[i] == -1) continue;

                if (ExcludeNotOpenedRecipe(i, selectedRecipe[i]))
                {
                    refund += Managers.Data.Recipes.GetByKey(selectedRecipe[i]).Cost / 2;

                    selectedRecipe[i] = -1;

                    if (Managers.Data.User.TryChangeMenu(i, -1))

                    hasExcluded = true;
                }
            }

            // 메뉴에서 빠진게 있으면 보상
            if (hasExcluded)
            {
                // LobbyManager.Instance.ActivateErrorPopup("아직 열리지 않은 레시피를\n메뉴판에서 발견했어요.");
                string message = "아직 열리지 않은 레시피를\n메뉴판에서 발견했어요.";
                StartCoroutine(PayRefund(1, refund, message));
                return;
            }
        }

        // 현재 메뉴 상태가 가능한 상태인지 확인
        _menu.SetSelectedRecipes();

        for (int i = 0; i < selectedRecipe.Count; i++)
        {
            if (!_menu.CanApply(selectedRecipe[i], i)) return;
        }

        Managers.Session.RecipeKeys = selectedRecipe;

        HashSet<int> monsters = new HashSet<int>();

        for (int i = 0; i < selectedRecipe.Count; i++)
        {
            int key = selectedRecipe[i];
            if (key == -1) continue;

            List<int> ingredients = Managers.Data.Recipes.GetByKey(key).Ingredients;

            for (int j = 0; j < ingredients.Count; j++)
            {
                int monsterId = Managers.Data.Ingredients.GetByKey(ingredients[j]).MonsterItem;
                monsters.Add(monsterId);
            }
        }

        Managers.Session.MonsterKeys = new List<int>(monsters);

        Managers.Session.hasVisitedGame = true;
        SceneManager.LoadScene("MonsterScene");
    }


    // 비공개 테스트 한정 (안 열린 레시피 끼워뒀으면 빼기)
    public bool ExcludeNotOpenedRecipe(int menuIdx, int id)
    {
        if (id == -1) return false;

        int recipeOpenChapter = Managers.Data.Recipes.GetByKey(id).availableChapter;
        int currentChapter = Managers.Session.StageData.Chapter;

        if (recipeOpenChapter > currentChapter)
        {
            // if (Managers.Data.User.TryChangeMenu(menuIdx, -1))
            // {
            //     UserDataFile.Save(Managers.Data.User);
            // }
            return true;
        }

        return false;
    }

    private IEnumerator RefundUnlockRecipe()
    {
        // yield return new WaitForSeconds(5f);

        // 이미 환급 받은 적 있으면 break
        if (Managers.Data.User.HadReceivedRefund[0]) yield break;


        List<int> refundunlockedRecipes = new List<int>(Managers.Data.User.UnlockRecipes)
            .Where(r => Managers.Data.Recipes.GetByKey(r).availableChapter > Managers.Session.StageData.Chapter)
            .ToList();

        // List<int> refundunlockedRecipes = new List<int>();
        // int currentChapter = Managers.Session.StageData.Chapter;

        // foreach (int r in Managers.Data.User.UnlockRecipes)
        // {
        //     if (Managers.Data.Recipes.GetByKey(r).availableChapter > currentChapter)
        //     {
        //         refundunlockedRecipes.Add(r);
        //     }
        // }
        // yield break;

        // 환급 대상인 레시피가 없으면 break
        if (refundunlockedRecipes.Count <= 0) yield break;


        int refund = 0;

        // 환급 대상인 레시피가 있으면
        for (int i = 0; i < refundunlockedRecipes.Count; i++)
        {
            int id = refundunlockedRecipes[i];
            refund += Managers.Data.Recipes.GetByKey(id).Cost / 2;
        }

        // LobbyManager.Instance.ActivateErrorPopup("아직 열리지 않은 레시피를\n구매한 내역을 발견했어요.");
        string message = "아직 열리지 않은 레시피를\n구매한 내역을 발견했어요.";
        StartCoroutine(PayRefund(0, refund, message));
    }

    public IEnumerator PayRefund(int idx, int refund, string message)
    {
        // yield return new WaitForSeconds(2f);
        // LobbyManager.Instance.ActivateErrorPopup("열리지 않은 레시피는\n현재 사용 할 수 없어요.\n열리면 돌려 드릴게요!");
        // yield return new WaitForSeconds(2f);
        LobbyManager.Instance.ActivateErrorPopup($"죄송한 마음으로 보상금을 지급합니다.\n보상금 : {refund:N0}G\n(레시피 구매액의 50%)");
        LobbyManager.Instance.ActivateErrorPopup("열리지 않은 레시피는\n현재 사용 할 수 없어요.\n열리면 돌려 드릴게요!");
        LobbyManager.Instance.ActivateErrorPopup(message);

        yield return new WaitUntil(() => !LobbyManager.Instance._errorPanel.gameObject.activeSelf);
        // yield return new WaitForSeconds(2f);
        Managers.Data.User.TryAddCoin(refund);
        Managers.Data.User.TryChangeHadReceivedRefund(idx);
    }

    private void RefreshStartBtn()
    {
        _startButton.GetComponentInChildren<TMP_Text>().text = $"{Managers.Session.StageData.DisplayName}\n도전 시작";
    }
    
    private void OpenRecipeNotice()
    {
        int beforeStage = Managers.Session.BeforeStageLevel;

        // 게임을 이어서 하고 있는게 아니라면 return
        if (beforeStage == -1) return;

        int beforeChapter = Managers.Data.Stage.GetByKey(beforeStage).Chapter;
        int currentChapter = Managers.Session.StageData.Chapter;

        if (beforeChapter != currentChapter)
        {
            LobbyManager.Instance.ActivateErrorPopup("새로운 레시피를 얻었어요!\n'레시피' 탭에서 확인해보세요!");
        }
    }
}
