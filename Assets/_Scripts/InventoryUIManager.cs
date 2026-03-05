using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Tabs")]
    [SerializeField] private Button inventoryTabButton;
    [SerializeField] private Button documentsTabButton;
    [SerializeField] private GameObject inventoryGridPanel;
    [SerializeField] private GameObject documentsPanel;

    [Header("Grid Settings")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;

    [Header("Documents")]
    [SerializeField] private Transform documentsParent;
    [SerializeField] private GameObject documentEntryPrefab;

    [Header("Selected Item")]
    [SerializeField] private GameObject selectedItemPanel;
    [SerializeField] private TextMeshProUGUI selectedItemNameText;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;


    private InventorySlotUI[,] slots;
    private Item selectedItem;
    private int selectedX, selectedY;
    private bool isInventoryView = true;
    private InventoryGrid inventoryGrid;

    void Start()
    {
        inventoryGrid = InventoryGrid.Instance;
        InitializeGrid();
        SetupTabs();
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void InitializeGrid()
    {
        int gridWidth = 6; 
        int gridHeight = 7;

        slots = new InventorySlotUI[gridWidth, gridHeight];
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridParent);
                InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
                slot.Initialize(x, y, this);
                slots[x, y] = slot;
            }
        }
    }

    private void SetupTabs()
    {
        inventoryTabButton.onClick.AddListener(() => SwitchView(true));
        documentsTabButton.onClick.AddListener(() => SwitchView(false));
    }

    private void SwitchView(bool showInventory)
    {
        isInventoryView = showInventory;
        inventoryGridPanel.SetActive(showInventory);
        documentsPanel.SetActive(!showInventory);

        if (!showInventory)
        {
            RefreshDocumentsList();
        }
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        if (inventoryPanel.activeSelf)
        {
            RefreshInventoryGrid();
            RefreshDocumentsList();
            SwitchView(isInventoryView);
        }
    }

    public void RefreshInventoryGrid()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
        List<Item> items = inventoryGrid.GetAllItems();
        foreach (var item in items)
        {
            Vector2Int pos = inventoryGrid.GetItemPosition(item);
            if (pos.x >= 0 && pos.y >= 0 && pos.x < slots.GetLength(0) && pos.y < slots.GetLength(1))
            {
                slots[pos.x, pos.y].SetItem(item);
            }
        }
    }

    private void RefreshDocumentsList()
    {
        foreach (Transform child in documentsParent)
        {
            Destroy(child.gameObject);
        }
        List<Item> documents = ReadableManager.Instance.GetAllReadables();
        foreach (var doc in documents)
        {
            GameObject entry = Instantiate(documentEntryPrefab, documentsParent);
            TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = doc.GetItemName();
            }
            Button button = entry.GetComponent<Button>();
            if (button != null)
            {
                Item capturedDoc = doc;
                button.onClick.AddListener(() => ShowDocument(capturedDoc));
            }
        }
    }

    private void ShowDocument(Item document)
    {   //Later: A Panel for reading individual documents
        Debug.Log($"=== {document.GetItemName()} ===\n{document.GetReadableText()}\n================");
    }

    public void SelectItem(Item item, int x, int y)
    {
        if (selectedItem != null)
        {
            slots[selectedX, selectedY].SetSelected(false);
        }
        selectedItem = item;
        selectedX = x;
        selectedY = y;
        slots[x, y].SetSelected(true);
        selectedItemPanel.SetActive(true);
        selectedItemNameText.text = item.GetItemName();
    }

    public void OnUseButton()
    {
        if (selectedItem != null)
        {
            //Later: Using items logic
            Debug.Log($"Using item: {selectedItem.GetItemName()}");
        }
    }

    public void OnDropButton()
    {
        if (selectedItem != null)
        {
            Debug.Log($"Dropping item: {selectedItem.GetItemName()}");
            inventoryGrid.RemoveItem(selectedItem);
            slots[selectedX, selectedY].ClearSlot();
            selectedItem = null;
            selectedItemPanel.SetActive(false);
            RefreshInventoryGrid();
        }
    }
}