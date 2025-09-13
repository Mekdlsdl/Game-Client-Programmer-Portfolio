using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using Unity.VisualScripting;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private GameObject _panel;

    [Header("AbleRecipe UI")]
    [SerializeField] private List<GameObject> _ableRecipeBoxes;
    [SerializeField] private List<Image> _ableRecipeImages;
    [SerializeField] private List<Image> _ableMarkImages;
    [SerializeField] private List<TMP_Text> _ableRecipeCounts;
    [SerializeField] private Sprite[] _ableBoxes;


    [Header("Recipe Box")]
    [SerializeField] private GameObject _recipeBox;
    [SerializeField] private List<Image> _ingreIcons;
    [SerializeField] private List<TMP_Text> _ingreCounts;


    [Header("Inventory Popup")]
    [SerializeField] private GameObject _inventoryPopup;
    [SerializeField] private GameObject _invenBox;
    [SerializeField] private TMP_Text _bagSizeNCntText;

    [Header("확인 용도(인스펙터 창에서 안 넣어도 됨)")]
    [SerializeField] private List<Image> _invenImages;
    [SerializeField] private List<TMP_Text> _invenCounts;



    private List<int> _selectedRecipes = new();
    private List<(int, int)> _inventory = new List<(int, int)>();
    private int _totalIngre = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetAbleRecipe();
        SetInvenInfo();
    }

    public List<(int, int)> GetInventory()
    {
        return _inventory;
    }

    public int GetInvenCount()
    {
        return _totalIngre;
    }

    public void SetInventory(List<(int, int)> inventory, int totalIngre)
    {
        Debug.Log($"인벤으로 다시 돌림.\ninventory.Count = {inventory.Count}\n_inventory.Count = {_inventory.Count}");
        _inventory.Clear();

        for (int i = 0; i < inventory.Count; i++)
        {
            _inventory.Add(inventory[i]);
        }

        _totalIngre = totalIngre;
    }

    public void SetInvenInfo()
    {
        foreach (Transform child in _invenBox.transform)
        {
            _invenImages.Add(child.GetChild(0).GetComponent<Image>());
            _invenCounts.Add(child.GetChild(1).GetComponent<Transform>().GetChild(0).GetComponent<TMP_Text>());
        }
    }

    public void SetInvenButton()
    {
        for (int i = 0; i < _inventory.Count; i++)
        {
            _invenBox.transform.GetChild(i).GetComponent<Button>().enabled = true;
        }
    }

    // 몬스터 사냥 후 재료 얻을 시 인벤토리에 저장
    public bool CollectIngredients(int ingId, int cnt = 1)
    {
        Debug.Log($"{Managers.Data.Ingredients.GetByKey(ingId).DisplayName} {cnt}개 저장 시도");

        if (_totalIngre >= StageManager.Instance.TycoonInfo.InventorySize)
        {
            Debug.Log("가방이 꽉 찼습니다.");
            return false;
        }

        for (int i = 0; i < _inventory.Count; i++)
        {
            // 해당 칸의 재료와 수
            (int ingredient, int count) = _inventory[i];

            // 같은 재료를 채울 경우
            if (ingredient == ingId)
            {
                count += cnt;
                // 개수만 증가
                _inventory[i] = (ingId, count);
                ApplyInventory(cnt);
                return true;
            }
        }

        // return 되지 못한 경우 = 인벤이 비어있음 or 인벤에 같은 음식이 없음
        // Debug.Log("인벤이 비어있음 or 인벤에 같은 음식이 없음");
        _inventory.Add((ingId, cnt));
        ApplyInventory(cnt);
        return true;
    }

    public void ApplyInventory(int cnt)
    {
        _totalIngre += cnt;

        SyncInventory();

        if (!BinManager.Instance.IsOpen) SyncAbleRecipe(); 
    }

    public bool UseIngredient(int ingreId, int count)
    {
        Debug.Log($"UseIngredient / ingreId = {ingreId}, count = {count}");

        for (int i = 0; i < _inventory.Count; i++)
        {
            (int ingredient, int cnt) = _inventory[i];

            if (ingredient == ingreId)
            {
                int rest = cnt - count;

                if (rest < 0)
                {
                    Debug.Log("재료 부족!");
                    return false;
                }
                else if (rest == 0)
                {
                    _inventory.Remove((ingredient, cnt));
                }
                else
                {
                    _inventory[i] = (ingredient, rest);
                }

                _totalIngre -= count;

                SyncInventory();

                // 쓰레기통이 닫혀있을 때만
                if (!BinManager.Instance.IsOpen) SyncAbleRecipe();
                return true;
            }
        }

        Debug.Log("해당 재료가 없습니다.");
        return false;
    }

    // 몇개 만들 수 있는지 반환 (재료 하나씩만 확인)
    public int IsInInven(int ingId, int cnt)
    {
        for (int i = 0; i < _inventory.Count; i++)
        {
            (int ingredient, int count) = _inventory[i];

            // count - 인벤토리 / cnt - 필요한 재료 수
            if (ingredient == ingId && count >= cnt)
            {
                return count/cnt;
            }
        }
        return 0;
    }

    public void SetAbleRecipe()
    {
        _selectedRecipes = Managers.Data.User.SelectRecipes;

        for (int i = 0; i < _selectedRecipes.Count; i++)
        {
            if (_selectedRecipes[i] == -1)
            {
                if (_ableRecipeBoxes[i].activeSelf) _ableRecipeBoxes[i].SetActive(false);
                continue;
            }

            Image ableRecipeImage = _ableRecipeImages[i];
            ableRecipeImage.sprite = Utils.LoadIconSprite(Managers.Data.Recipes.GetByKey(_selectedRecipes[i]).Icon);

            Color recipeColor = ableRecipeImage.color;
            recipeColor.a = 1f;
            ableRecipeImage.color = recipeColor;

            _ableRecipeCounts[i].text = "0";

            Image ableMarkImage = _ableMarkImages[i];
            ableMarkImage.sprite = _ableBoxes[0];

            Color color = ableMarkImage.color;
            color.a = 1f;
            ableMarkImage.color = color;

            ButtonInfo buttonInfo = ableMarkImage.transform.parent.GetComponent<ButtonInfo>();
            buttonInfo.Id = _selectedRecipes[i];
            
            if (!_ableRecipeBoxes[i].activeSelf) _ableRecipeBoxes[i].SetActive(true);
        }
    }

    public void SyncAbleRecipe()
    {
        for (int i = 0; i < _selectedRecipes.Count; i++)
        {
            _ableRecipeCounts[i].text = "0";
            int canCook = 0;

            if (_selectedRecipes[i] != -1)
            { 
                canCook = CookingHandler.Instance.CanCookCheck(_selectedRecipes[i]);
            }

            int ableNum = canCook > 0 ? 1 : 0;

            Image ableMarkImage = _ableMarkImages[i];
            ableMarkImage.sprite = _ableBoxes[ableNum];

            _ableRecipeCounts[i].text = canCook.ToString("N0");
        }
    }

    public void ShowInventory()
    {
        // 쓰레기통이 닫혀있는데 인벤토리가 열려 있을 경우
        if (_inventoryPopup.activeSelf && !BinManager.Instance.IsOpen)
        {
            HideInventory();
            return;
        }

        SyncInventory();

        Debug.Log($"Bin 상태 = {BinManager.Instance.IsOpen}");
        _inventoryPopup.SetActive(true);
    }

    public void SyncInventory()
    {
        _inventory.RemoveAll(item => item.Item2 == 0);

        for (int i = 0; i < _invenImages.Count; i++)
        {
            // 인벤토리에 지금 있는 양 채우기
            if (i < _inventory.Count)
            {
                // 해당 칸의 재료와 수
                (int ingredient, int count) = _inventory[i];

                // 스프라이트 불러오기
                Sprite ingreSprite = Utils.LoadIconSprite(Managers.Data.Ingredients.GetByKey(ingredient).Icon);

                Image invIcon = _invenImages[i];

                // 인벤토리 UI 채우기
                invIcon.sprite = ingreSprite;

                // 투명도 조절
                Color color = invIcon.color;
                color.a = 1f;
                invIcon.color = color;

                // 개수
                _invenCounts[i].text = count.ToString("N0");

                // ButtonInfo 채우기
                var buttonInfo = _invenImages[i].transform.parent.GetComponent<ButtonInfo>();
                buttonInfo.Id = ingredient;
                buttonInfo.Count = count;

                // 쓰레기통 열려 있으면 버튼 활성화
                if (BinManager.Instance.IsOpen)
                {
                    _invenImages[i].transform.parent.GetComponent<Button>().interactable = true;
                }
                else
                {
                    _invenImages[i].transform.parent.GetComponent<Button>().interactable = false;
                }
            }
            // 나머지는 비우기
            else
            {
                Image invIcon = _invenImages[i];

                // 인벤토리 UI 비우기
                invIcon.sprite = null;

                // 투명도 조절
                Color color = invIcon.color;
                color.a = 0f;
                invIcon.color = color;

                // 개수
                _invenCounts[i].text = "";

                // ButtonInfo 리셋
                var buttonInfo = _invenImages[i].transform.parent.GetComponent<ButtonInfo>();
                buttonInfo.Id = -1;
                buttonInfo.Count = 0;

                // 비어있으면 항상 비활성화 걸어두기
                _invenImages[i].transform.parent.GetComponent<Button>().interactable = false;
            }
        }

        _bagSizeNCntText.text = $"{_totalIngre:N0} / {StageManager.Instance.TycoonInfo.InventorySize:N0}";
    }

    public void HideInventory()
    {
        ResetInventory();
        _inventoryPopup.SetActive(false);
    }

    public void ResetInventory()
    {
        for (int i = 0; i < _invenImages.Count; i++)
        {
            _invenImages[i].sprite = null;

            Color invenImgCol = _invenImages[i].color;
            invenImgCol.a = 0f;
            _invenImages[i].color = invenImgCol;
            
            _invenCounts[i].text = "";

            // ButtonInfo 리셋
            var buttonInfo = _invenImages[i].transform.parent.GetComponent<ButtonInfo>();
            buttonInfo.Id = -1;
            buttonInfo.Count = 0;
        }

        _bagSizeNCntText.text = "";

    }

    public IEnumerator ActivateRecipeBox(ButtonInfo buttonInfo)
    {
        var data = Managers.Data.Recipes.GetByKey(buttonInfo.Id);
        List<int> ingredients = data.Ingredients;
        List<int> ingreCnts = data.Counts;

        for (int i = 0; i < ingredients.Count; i++)
        {
            _ingreIcons[i].sprite = Utils.LoadIconSprite(Managers.Data.Ingredients.GetByKey(ingredients[i]).Icon);

            Color ingreCol = _ingreIcons[i].color;
            ingreCol.a = 1f;
            _ingreIcons[i].color = ingreCol;

            _ingreCounts[i].text = ingreCnts[i].ToString();
        }
        _recipeBox.SetActive(true);
        _panel.SetActive(true);

        yield return new WaitForSeconds(5f);
        _recipeBox.SetActive(false);
        _panel.SetActive(false);
    }

    public void InactivateRecipeBox()
    {
        if (!_recipeBox.activeSelf) return;

        for (int i = 0; i < _ingreIcons.Count; i++)
        {
            _ingreIcons[i].sprite = null;

            Color ingreCol = _ingreIcons[i].color;
            ingreCol.a = 0f;
            _ingreIcons[i].color = ingreCol;

            _invenCounts[i].text = "0";
        }
        _recipeBox.SetActive(false);
        _panel.SetActive(false);
    }


    // =================== 테스트용 ===================
    int _testId = -1;
    int _testEndId = -1;
    int _testCnt = -1;

    public void GetId(TMP_InputField ingreId)
    {
        if (ingreId.text != "")
        {

            if (ingreId.text.Contains("~"))
            {
                (_testId, _testEndId) = (int.Parse(ingreId.text.Trim().Split("~")[0]), int.Parse(ingreId.text.Trim().Split("~")[1]));
            }
            else
            {
                _testId = int.Parse(ingreId.text.Trim());
                Debug.Log(_testId);
            }

            ingreId.text = "";
        }
    }
    public void GetCnt(TMP_InputField ingreCnt)
    {
        if (ingreCnt.text != "")
        {
            Debug.Log(ingreCnt.text);
            _testCnt = int.Parse(ingreCnt.text.Trim());
            ingreCnt.text = "";
        }
    }
    public void AddInventory()
    {
        if (_testId != -1 && _testCnt != -1)
        {
            if (_testEndId == -1)
            {
                _testEndId = _testId;
            }

            for (int j = _testId; j <= _testEndId; j++)
            {               
                CollectIngredients(j, _testCnt);
            }
        }

        _testCnt = -1; _testEndId = -1; _testId = -1;
    }
}
