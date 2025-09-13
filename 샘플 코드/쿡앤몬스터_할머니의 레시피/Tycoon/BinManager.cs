using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BinManager : MonoBehaviour
{
    public static BinManager Instance { get; private set; }

    public bool CanOpenBin = true;
    public bool IsOpen = false;

    [SerializeField] private int _size = 9;

    [Header("Bin Object")]
    [SerializeField] private Sprite[] _binImages;
    [SerializeField] private SpriteRenderer _binObject;
    // [SerializeField] private GameObject _invenPopup;

    [Header("Bin Panel")]
    [SerializeField] private GameObject _binPanel;
    [SerializeField] private List<GameObject> _binButtons;
    [SerializeField] private List<TMP_Text> _binCounts;
    [SerializeField] private Button _binClearButton;

    [Header("Bin Count Popup")]
    [SerializeField] private GameObject _binCountPopup;
    [SerializeField] private TMP_Text _binCountIngreText;
    [SerializeField] private TMP_InputField _binCountInput;

    [Header("Bag Button")]
    [SerializeField] private Button _bagButton;

    [Header("확인 용도(인스펙터 창에서 안 넣어도 됨)")]
    [SerializeField] private List<GameObject> _invenButtons;


    private InventoryManager _inventoryManager;
    private List<(int, int)> _binIngreList = new();
    private List<(int, int)> _tempInventory = new();
    private int _tempTotalIngre = 0;

    private int _ingredientId = -1;
    private int _ingredientCount = 0;


    void Awake()
    {
        Instance = this;
        _inventoryManager = InventoryManager.Instance;
    }

    public bool CanOpenBinChenck()
    {
        if (!CanOpenBin) return false;
        if (_inventoryManager.GetInvenCount() <= 0) return false;

        return true;
    }

    public IEnumerator ActivateBin()
    {
        // 이미 켜져있는 경우 무시
        if (_binPanel.activeSelf) yield break;

        // 못 열도록 제한 걸려있을 때 무시
        if (!CanOpenBin) yield break;

        IsOpen = true;
        CanOpenBin = false;

        // 조이스틱 꺼주기
        UIManager.Instance.Joystick.SetCanUse(false);

        ChangeLidState();
        SetTempInventory(); // 기존 인벤 저장
        Debug.Log($"기존 인벤 저장, _tempInventory.Count = {_tempInventory.Count}");

        yield return new WaitForSeconds(0.5f);

        // 인벤토리 - 버튼으로 바꾸기
        _inventoryManager.SetInvenButton();

        // 인벤토리 열기
        _inventoryManager.ShowInventory();

        // 인벤토리 버튼 비활성화
        _bagButton.interactable = false;

        // 쓰레기통 패널 열기
        _binPanel.SetActive(true);
    }

    /*
        1) 닫기 버튼을 눌렀을 때
            - 목록이 있을 경우
                -> 쓰레기통 목록 초기화
                -> 원래 인벤토리로 복구
                -> 쓰레기통 창 닫힘
            - 목록이 없을 경우
                -> 그냥 쓰레기통 창 닫힘
            
            
        2) 비우기 버튼을 눌렀을 때
            - 목록이 있을 경우
                -> 쓰레기통 목록 초기화
                -> 비우기 버튼을 누를 시점의 인벤토리로 유지
                -> 쓰레기통 창 닫힘
            - 목록이 없을 경우
                -> ??
                
                
        3) 닫기/비우기 공통점
        - 목록이 있을 경우 쓰레기통 목록 초기화
        - 쓰레기통 창 닫힘
    */


    // 닫기 버튼 눌렀을 때
    public void ClosedBin()
    {
        // 하나 이상 버려서 인벤토리 변동이 생겼을 때 이전 인벤 복구
        if (_binIngreList.Count > 0)
        {
            _inventoryManager.SetInventory(_tempInventory, _tempTotalIngre);
        }

        StartCoroutine(InactivateBin());
    }

    // 비우기 버튼 눌렀을 때
    public IEnumerator InactivateBin()
    {
        // 쓰레기통 목록 초기화 및 AbleRecipe 동기화
        ResetBin();
        InventoryManager.Instance.SyncAbleRecipe();

        // 개수 창 열려있다면 닫음
        if(_binCountPopup.activeSelf) _binCountPopup.SetActive(false);

        // 쓰레기통 창 닫힘
        _binPanel.SetActive(false);
        _inventoryManager.HideInventory();

        IsOpen = false;
        ChangeLidState();

        // 조이스틱 다시 켜주기
        UIManager.Instance.Joystick.SetCanUse(true);

        Debug.Log("2초 아직 안 지나서 다시 열 수 없음");

        // 2초 뒤 다시 열 수 있음
        yield return new WaitForSeconds(2f);

        CanOpenBin = true;
        Debug.Log("2초 지나서 다시 열 수 있음");
    }

    // 쓰레기통 리셋
    public void ResetBin()
    {
        for (int i = 0; i < _binIngreList.Count; i++)
        {
            // ButtonInfo 리셋
            ButtonInfo buttonInfo = _binButtons[i].GetComponent<ButtonInfo>();
            buttonInfo.Id = -1;
            buttonInfo.Count = 0;

            // 버튼 비활성화
            _binButtons[i].GetComponent<Button>().interactable = false;

            // 이미지 리셋
            Image buttonImage = _binButtons[i].transform.GetChild(0).GetComponent<Image>();
            buttonImage.sprite = null;

            Color buttonImgCol = buttonImage.color;
            buttonImgCol.a = 0f;
            buttonImage.color = buttonImgCol;

            // 카운트 텍스트 리셋
            _binCounts[i].text = "";
        }

        _binIngreList.Clear();
        _tempInventory.Clear();

        // 가방 버튼 다시 활성화
        _bagButton.interactable = true;
    }

    // 쓰레기통 열림/닫힘
    public void ChangeLidState()
    {
        int binState = IsOpen ? 1 : 0;
        _binObject.sprite = _binImages[binState];
    }

    // 원본 저장
    public void SetTempInventory()
    {
        List<(int, int)> originalInven = _inventoryManager.GetInventory();

        for (int i = 0; i < originalInven.Count; i++)
        {
            (int ingre, int cnt) = originalInven[i];

            _tempInventory.Add((ingre, cnt));
        }

        _tempTotalIngre = _inventoryManager.GetInvenCount();
    }

    // 재료 버튼을 눌렀을 때
    public void SwapIngredient(GameObject button)
    {
        ButtonInfo buttonInfo = button.GetComponent<ButtonInfo>();
        int ingreId = buttonInfo.Id;
        int ingreCnt = buttonInfo.Count;

        // 쓰레기통에서 재료 눌렀을 경우 다시 인벤에 추가
        if (buttonInfo.Type == ButtonType.Bin)
        {
            _inventoryManager.CollectIngredients(ingreId, ingreCnt);
            _binIngreList.Remove((ingreId, ingreCnt));
            SyncBin();
        }

        // 인벤에서 재료 눌렀을 경우 개수 팝업 띄움
        else if (buttonInfo.Type == ButtonType.Inventory)
        {
            _binCountPopup.SetActive(true);
            SetCountPopup(ingreId, ingreCnt);
        }
    }

    // 개수 팝업 세팅
    public void SetCountPopup(int id, int cnt)
    {
        _ingredientId = id; _ingredientCount = cnt;
        _binCountIngreText.text = Managers.Data.Ingredients.GetByKey(id).DisplayName;
        _binCountInput.text = "";
    }

    public void SetMax()
    {
        _binCountInput.text = _ingredientCount.ToString("N0");
    }

    // 인벤 -> 쓰레기통 재료 넣음 (개수 팝업 확인 버튼에 연결)
    public void PutInBin()
    {
        bool IsPutIn = false;

        int id = _ingredientId;
        int cnt = int.Parse(_binCountInput.text);

        // 쓰레기통에 재료를 버릴 수 있을 때만
        if (!_inventoryManager.UseIngredient(id, cnt))
        {
            Debug.Log("입력값이 개수보다 많습니다.");
            return;
        }

        // 쓰레기통에 같은 재료가 이미 있을 때
        for (int i = 0; i < _binIngreList.Count; i++)
        {
            (int ingredient, int count) = _binIngreList[i];

            if (ingredient == id)
            {
                _binIngreList[i] = (ingredient, count + cnt);
                SyncBin();
                _binCountPopup.SetActive(false); // 정상적으로 버렸을 때만 닫기
                IsPutIn = true; // 재료 버림
            }
        }

        // 쓰레기통에서 재료를 찾지 못했다면
        if (!IsPutIn)
        {
            // 1) 쓰레기통 꽉참
            if (_binIngreList.Count > _size)
            {
                Debug.Log("쓰레기통이 꽉 찼습니다.");
            }

            // 2) 새로운 재료를 버림
            else
            {
                _binIngreList.Add((id, cnt));
                SyncBin();
                _binCountPopup.SetActive(false); // 정상적으로 버렸을 때만 닫기
            }
        }
    }

    // 쓰레기통 동기화
    public void SyncBin()
    {
        Debug.Log($"휴지통에 {_binIngreList.Count}개 있음");
        // 버리기 버튼 interactable 체크
        if (_binIngreList.Count == 0)
        {
            _binClearButton.interactable = false;
        }
        else
        {
            _binClearButton.interactable = true;
        }


        // 앞에서부터 당겨서 채우기
        for (int i = 0; i < _size; i++)
        {
            Image image = _binButtons[i].transform.GetChild(0).GetComponent<Image>();
            ButtonInfo buttonInfo = _binButtons[i].GetComponent<ButtonInfo>();

            if (i < _binIngreList.Count)
            {
                (int id, int cnt) = _binIngreList[i];

                string icon = Managers.Data.Ingredients.GetByKey(id).Icon;
                image.sprite = Utils.LoadIconSprite(icon);

                Color imageColor = image.color;
                imageColor.a = 1f;
                image.color = imageColor;

                _binCounts[i].text = cnt.ToString("N0");

                buttonInfo.Id = id;
                buttonInfo.Count = cnt;

                _binButtons[i].GetComponent<Button>().interactable = true;
            }
            else
            {
                image.sprite = null;

                Color imageColor = image.color;
                imageColor.a = 0f;
                image.color = imageColor;

                _binCounts[i].text = "";

                buttonInfo.Id = -1;
                buttonInfo.Count = 0;

                _binButtons[i].GetComponent<Button>().interactable = false;
            }
        }
    }
}
