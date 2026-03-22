using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// Manages the storebox panel UI
public class StoreboxUIManager : MonoBehaviour
{
    public static StoreboxUIManager Instance { get; private set; }
    public bool IsOpen => storeboxPanel != null && storeboxPanel.activeSelf;
    [Header("Panel")]
    [SerializeField] private GameObject storeboxPanel;
    [Header("List Parents (ScrollView Content objects)")]
    [SerializeField] private Transform boxListParent;
    [SerializeField] private Transform inventoryListParent;
    [Header("Action Bar")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button allButton;
    [SerializeField] private TextMeshProUGUI allButtonText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button closeButton;
    [Header("Row Prefab")]
    [Tooltip("Prefab with Image bg + HorizontalLayoutGroup + StoreboxRowUI script")]
    [SerializeField] private GameObject rowPrefab;

    private Item selectedItem = null;
    private bool selectedIsInBox = false;
    // Colours
    private static readonly Color ColSelected = new Color(0.25f, 0.55f, 0.90f, 0.85f);
    private static readonly Color ColNormal = new Color(0.18f, 0.18f, 0.18f, 0.85f);
    private static readonly Color ColHover = new Color(0.28f, 0.28f, 0.28f, 0.85f);
    // Tracks spawned row buttons for colour refresh
    private List<(Item item, Image bg, bool inBox)> rows = new List<(Item, Image, bool)>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        storeboxPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(CloseStorebox);
        if (actionButton != null) actionButton.onClick.AddListener(OnActionClicked);
        if (allButton != null) allButton.onClick.AddListener(OnAllClicked);
        ClearAction();
    }

    void Update()
    {
        if (storeboxPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            CloseStorebox();
    }

    public void OpenStorebox()
    {
        storeboxPanel.SetActive(true);
        selectedItem = null;
        ClearAction();
        Refresh();
        WeaponHUD.Instance?.Hide();
    }

    public void CloseStorebox()
    {
        storeboxPanel.SetActive(false);
        selectedItem = null;
        WeaponHUD.Instance?.Show();
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    private void Refresh()
    {
        BuildList(boxListParent, StoreboxManager.Instance.GetStoredItems(), inBox: true);
        BuildList(inventoryListParent, InventoryGrid.Instance.GetAllItems(), inBox: false);
        SetFeedback("");
    }

    private void BuildList(Transform parent, IEnumerable<Item> items, bool inBox)
    {
        rows.RemoveAll(r => r.inBox == inBox);
        foreach (Transform c in parent) Destroy(c.gameObject);
        foreach (var item in items)
        {
            Item cap = item;
            bool mine = inBox;
            // Instantiate row from prefab — all visual structure set in Inspector
            GameObject rowGO = rowPrefab != null
                ? Instantiate(rowPrefab, parent)
                : new GameObject(item.GetItemName(), typeof(RectTransform));
            rowGO.transform.SetParent(parent, false);
            rowGO.name = item.GetItemName();

            Image bg = rowGO.GetComponent<Image>();
            if (bg != null) bg.color = (selectedItem == item) ? ColSelected : ColNormal;

            // Populate via StoreboxRowUI if present, otherwise fallback
            StoreboxRowUI rowUI = rowGO.GetComponent<StoreboxRowUI>();
            int stackCount = inBox
                ? StoreboxManager.Instance.GetStackCount(item)
                : InventoryGrid.Instance.GetStackCount(item);
            string labelText = stackCount > 1
                ? $"{item.GetItemName()}  <color=#aaaaaa>x{stackCount}</color>"
                : item.GetItemName();

            if (rowUI != null)
                rowUI.Populate(item.GetIcon(), labelText);

            Button btn = rowGO.GetComponent<Button>();
            if (btn != null)
            {
                btn.targetGraphic = bg;
                ColorBlock cb = btn.colors;
                cb.normalColor = ColNormal;
                cb.highlightedColor = ColHover;
                cb.selectedColor = ColSelected;
                cb.pressedColor = ColSelected;
                btn.colors = cb;
                btn.onClick.AddListener(() => OnRowClicked(cap, mine, bg));
            }

            rows.Add((item, bg, inBox));
        }
    }

    private void OnRowClicked(Item item, bool inBox, Image bg)
    {
        // Deselect all rows
        foreach (var r in rows)
            r.bg.color = ColNormal;

        if (selectedItem == item)
        {
            // Second click = deselect
            selectedItem = null;
            ClearAction();
            return;
        }
        selectedItem = item;
        selectedIsInBox = inBox;
        bg.color = ColSelected;
        SetFeedback("");

        if (inBox)
        {
            if (actionButton != null) actionButton.gameObject.SetActive(true);
            if (actionButtonText != null) actionButtonText.text = "Withdraw";
            if (allButton != null) allButton.gameObject.SetActive(true);
            if (allButtonText != null) allButtonText.text = "Withdraw All";
        }
        else
        {
            if (actionButton != null) actionButton.gameObject.SetActive(true);
            if (actionButtonText != null) actionButtonText.text = "Store";
            if (allButton != null) allButton.gameObject.SetActive(true);
            if (allButtonText != null) allButtonText.text = "Store All";
        }
    }
    private void OnActionClicked()
    {
        if (selectedItem == null) return;

        if (selectedIsInBox)
        {
            // Withdraw
            bool ok = StoreboxManager.Instance.WithdrawItem(selectedItem);
            if (!ok)
                SetFeedback("Inventory is full — reorganise or discard items first.");
            else
                SetFeedback($"Withdrew: {selectedItem.GetItemName()}");
        }
        else
        {
            // Store
            StoreboxManager.Instance.StoreItem(selectedItem);
            SetFeedback($"Stored: {selectedItem.GetItemName()}");
        }
        selectedItem = null;
        ClearAction();
        Refresh();
        //refreshes the main inventory grid if it's open
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RefreshInventoryGrid();
    }

    private void OnAllClicked()
    {
        if (selectedItem == null) return;
        Item item = selectedItem;

        if (selectedIsInBox)
        {
            int count = StoreboxManager.Instance.GetStackCount(item);
            int withdrawn = 0;
            for (int i = 0; i < count; i++)
            {
                bool ok = StoreboxManager.Instance.WithdrawItem(item);
                if (!ok) break;
                withdrawn++;
            }
            SetFeedback(withdrawn == count ? $"Withdrew all {count}x {item.GetItemName()}" : $"Withdrew {withdrawn}/{count} — inventory full.");
        }
        else
        {
            int count = InventoryGrid.Instance.GetStackCount(item);
            int stored = 0;
            for (int i = 0; i < count; i++)
            {
                StoreboxManager.Instance.StoreItem(item);
                stored++;
            }
            SetFeedback($"Stored all {stored}x {item.GetItemName()}");
        }
        selectedItem = null;
        ClearAction();
        Refresh();

        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RefreshInventoryGrid();
    }

    private void ClearAction()
    {
        if (actionButton != null) actionButton.gameObject.SetActive(false);
        if (allButton != null) allButton.gameObject.SetActive(false);
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = msg.Contains("full") ? new Color(0.9f, 0.3f, 0.3f, 1f) : new Color(0.7f, 0.9f, 0.7f, 1f);
        }
    }

}