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
    [SerializeField] private float tileSize = 80f;// pixels per tile  (increase for bigger grid)
    [SerializeField] private float gridMargin = 12f;    // padding inside container

    // Grid dimensions
    private const int GRID_WIDTH = 7;
    private const int GRID_HEIGHT = 6;

    [Header("Hover Panel  (below grid)")]
    [SerializeField] private GameObject hoverPanel;
    [SerializeField] private TextMeshProUGUI hoverItemNameText;
    [SerializeField] private TextMeshProUGUI hoverItemDescText;

    [Header("Click Panel  (context menu near cursor)")]
    [SerializeField] private GameObject clickPanel;
    [SerializeField] private Button useEquipButton;
    [SerializeField] private TextMeshProUGUI useEquipButtonText;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button replaceButton;
    [SerializeField] private RectTransform clickPanelRect;

    [Header("Documents")]
    [SerializeField] private Transform documentsListParent;//Scrollview content
    [SerializeField] private GameObject documentButtonPrefab;
    [SerializeField] private GameObject documentReaderPanel;
    [SerializeField] private TextMeshProUGUI documentTitleText;
    [SerializeField] private TextMeshProUGUI documentBodyText;
    [SerializeField] private Button documentCloseButton;

    [Header("Drag Ghost")]
    [SerializeField] private GameObject dragGhostPanel;
    [SerializeField] private Image dragGhostImage;
    [SerializeField] private RectTransform dragGhostRect;
    [SerializeField] private Button rotateButton;
    [SerializeField] private GameObject rotateButtonObject;

    [Header("Canvas")]
    [SerializeField] private Canvas parentCanvas;
    private InventorySlotUI[,] slots;
    private InventoryGrid inventoryGrid;
    private bool isInventoryView = true;

    // Click-panel state
    private Item clickedItem;
    private int clickedX, clickedY;
    // Drag state
    private Item draggedItem;
    private bool draggedItemRotated;
    private bool isDragging = false;
    public bool IsDragging => isDragging;
    // Ghost hover tracking
    private int ghostHoverX = -1;
    private int ghostHoverY = -1;

    void Awake()
    {
        inventoryGrid = InventoryGrid.Instance;
    }
    void Start()
    {
        if (inventoryGrid == null)
            inventoryGrid = InventoryGrid.Instance;
        if (rotateButton != null)
            rotateButton.onClick.AddListener(RotateDraggedItem);
        SetupGrid();
        SetupTabs();
        SetupClickPanel();
        SetupDocumentReader();

        hoverPanel.SetActive(false);
        clickPanel.SetActive(false);
        dragGhostPanel.SetActive(false);
        documentReaderPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();

        if (isDragging)
        {
            UpdateDragGhostPosition();

            // R rotates the dragged item
            if (Input.GetKeyDown(KeyCode.R))
                RotateDraggedItem();

            // Right-click or Escape cancels drag
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                CancelDrag();
        }
        else
        {
            // Escape also closes click panel / document reader
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (documentReaderPanel.activeSelf)
                    CloseDocumentReader();
                else if (clickPanel.activeSelf)
                    clickPanel.SetActive(false);
            }
        }
    }

    private void SetupGrid()
    {
        // Get the container's actual rect size
        RectTransform containerRect = gridParent.GetComponent<RectTransform>();

        // Force the layout to update so rect size is accurate
        Canvas.ForceUpdateCanvases();

        float availableWidth = containerRect.rect.width - gridMargin * 2;
        float availableHeight = containerRect.rect.height - gridMargin * 2;

        // Calculate tile size to fill the container, keeping tiles square
        float tileW = Mathf.Floor(availableWidth / GRID_WIDTH);
        float tileH = Mathf.Floor(availableHeight / GRID_HEIGHT);
        tileSize = Mathf.Min(tileW, tileH);   // square tiles, fit the smaller axis

        GridLayoutGroup layout = gridParent.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            layout.cellSize = new Vector2(tileSize, tileSize);
            layout.spacing = Vector2.zero;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = GRID_WIDTH;
            layout.padding = new RectOffset(
                (int)gridMargin, (int)gridMargin,
                (int)gridMargin, (int)gridMargin);
            layout.childAlignment = TextAnchor.MiddleCenter;
        }

        // Instantiate slots
        slots = new InventorySlotUI[GRID_WIDTH, GRID_HEIGHT];
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                GameObject go = Instantiate(slotPrefab, gridParent);
                InventorySlotUI slot = go.GetComponent<InventorySlotUI>();
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

        clickPanel.SetActive(false);
        hoverPanel.SetActive(false);

        if (!showInventory)
            RefreshDocumentsList();
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
        else
        {
            clickPanel.SetActive(false);
            hoverPanel.SetActive(false);
            CancelDrag();
        }
    }


    public void RefreshInventoryGrid()
    {
        // Clear all slots first
        foreach (var slot in slots)
            slot.ClearSlot();

        List<Item> items = inventoryGrid.GetAllItems();
        foreach (var item in items)
        {
            Vector2Int pos = inventoryGrid.GetItemPosition(item);
            bool rotated = inventoryGrid.IsItemRotated(item);

            if (pos.x < 0) continue;

            ItemSize size = inventoryGrid.GetEffectiveSize(item, rotated);

            for (int dx = 0; dx < size.width; dx++)
            {
                for (int dy = 0; dy < size.height; dy++)
                {
                    int sx = pos.x + dx;
                    int sy = pos.y + dy;
                    if (sx < GRID_WIDTH && sy < GRID_HEIGHT)
                    {
                        // All cells of a multi-tile item get the item reference
                        // Only the top-left cell renders the icon (handled in SetItem).
                        if (dx == 0 && dy == 0)
                            slots[sx, sy].SetItem(item, rotated);
                        else
                            slots[sx, sy].SetItem(item, rotated);
                    }
                }
            }
        }
    }

    public void OnSlotHoverEnter(Item item)
    {
        if (isDragging) return;

        hoverItemNameText.text = item.GetItemName();
        hoverItemDescText.text = item.GetDescription();
        hoverPanel.SetActive(true);

        // Highlight all tiles this item occupies
        Vector2Int pos = inventoryGrid.GetItemPosition(item);
        bool rotated = inventoryGrid.IsItemRotated(item);
        ItemSize size = inventoryGrid.GetEffectiveSize(item, rotated);

        if (pos.x < 0) return;

        for (int dx = 0; dx < size.width; dx++)
        {
            for (int dy = 0; dy < size.height; dy++)
            {
                int sx = pos.x + dx;
                int sy = pos.y + dy;
                if (sx < GRID_WIDTH && sy < GRID_HEIGHT)
                    slots[sx, sy].SetHoverHighlight();
            }
        }
    }

    public void OnSlotHoverExit()
    {
        if (isDragging) return;
        hoverPanel.SetActive(false);
        ClearAllGhostHighlights(); // resets all slot colors back to normal
    }

    private void SetupClickPanel()
    {
        useEquipButton.onClick.AddListener(OnUseEquipClicked);
        discardButton.onClick.AddListener(OnDiscardClicked);
        replaceButton.onClick.AddListener(OnReplaceClicked);
    }

    public void OnSlotClicked(Item item, int x, int y, Vector2 screenPos)
    {
        // Toggles off if clicking the same item again
        if (clickPanel.activeSelf && clickedItem == item)
        {
            clickPanel.SetActive(false);
            return;
        }

        clickedItem = item;
        clickedX = x;
        clickedY = y;

        // Configures button labels and visibility
        bool isKeyItem = item.GetItemType() == ItemType.KeyItem;
        bool isWeapon = item.GetItemType() == ItemType.Weapon;
        bool isStarterItem = item.IsStarterItem();
        discardButton.interactable = (!isKeyItem && !isStarterItem);
        useEquipButtonText.text = isWeapon ? "Equip" : "Use";

        clickPanel.SetActive(true);
        PositionClickPanel(screenPos);

        // Hides hover panel while click panel is open
        hoverPanel.SetActive(false);
    }

    private void PositionClickPanel(Vector2 screenPos)
    {
        if (parentCanvas == null) return;

        // Converts screen position to canvas local position directly
        Vector2 localPos;
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // Scales screen position to canvas space
        float scaleFactor = parentCanvas.scaleFactor;
        localPos = new Vector2(screenPos.x / scaleFactor, screenPos.y / scaleFactor);

        // Offsets slightly so panel doesn't sit directly under cursor
        localPos += new Vector2(2f, -2f);

        // Clamps so panel stays fully on screen
        Vector2 panelSize = clickPanelRect.sizeDelta;
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;

        localPos.x = Mathf.Clamp(localPos.x, 0f, canvasW - panelSize.x);
        localPos.y = Mathf.Clamp(localPos.y, panelSize.y, canvasH);

        clickPanelRect.anchoredPosition = localPos;
    }

    private void OnUseEquipClicked()
    {
        if (clickedItem == null) return;
        Debug.Log($"[Inventory] Use/Equip: {clickedItem.GetItemName()}");
        // TODO: hook up actual use/equip logic
        clickPanel.SetActive(false);
        clickedItem = null;
    }

    private void OnDiscardClicked()
    {
        if (clickedItem == null) return;
        if (clickedItem.GetItemType() == ItemType.KeyItem) return;   // safety

        Debug.Log($"[Inventory] Discarding: {clickedItem.GetItemName()}");
        inventoryGrid.RemoveItem(clickedItem);
        clickedItem = null;
        clickPanel.SetActive(false);
        hoverPanel.SetActive(false);
        RefreshInventoryGrid();
    }

    private void OnReplaceClicked()
    {
        if (clickedItem == null) return;
        Item toMove = clickedItem;
        clickPanel.SetActive(false);
        clickedItem = null;
        StartDragging(toMove);
    }
    public void StartDragging(Item item)
    {
        isDragging = true;
        draggedItem = item;
        draggedItemRotated = inventoryGrid.IsItemRotated(item);

        hoverPanel.SetActive(false);
        clickPanel.SetActive(false);

        if (item.GetIcon() != null)
            dragGhostImage.sprite = item.GetIcon();

        dragGhostPanel.SetActive(true);
        dragGhostRect.anchoredPosition = new Vector2(-9999f, -9999f);//To prevent the ghost from appearing on screen
        UpdateDragGhostSize();
        if (rotateButtonObject != null)
            rotateButtonObject.SetActive(true);      
    }

    private void UpdateDragGhostSize()
    {
        if (draggedItem == null) return;
        ItemSize size = inventoryGrid.GetEffectiveSize(draggedItem, draggedItemRotated);
        dragGhostRect.sizeDelta = new Vector2(size.width * tileSize, size.height * tileSize);
    }

    private void UpdateDragGhostPosition()
    {
        if (parentCanvas == null || !isDragging) return;
        float scaleFactor = parentCanvas.scaleFactor;
        Vector2 localPos = new Vector2(
            Input.mousePosition.x / scaleFactor,
            Input.mousePosition.y / scaleFactor
        );

        dragGhostRect.anchoredPosition = localPos;
    }

    private void RotateDraggedItem()
    {
        draggedItemRotated = !draggedItemRotated;
        UpdateDragGhostSize();
        if (ghostHoverX >= 0)
            OnDragHoverSlot(ghostHoverX, ghostHoverY);
    }

    public void OnDragHoverSlot(int x, int y)
    {
        if (draggedItem == null) return;

        ItemSize size = inventoryGrid.GetEffectiveSize(draggedItem, draggedItemRotated);
        x = Mathf.Clamp(x, 0, GRID_WIDTH - size.width);
        y = Mathf.Clamp(y, 0, GRID_HEIGHT - size.height);

        ghostHoverX = x;
        ghostHoverY = y;
        ClearAllGhostHighlights();
        bool canPlace = inventoryGrid.CanPlaceItemAtIgnoringSelf(draggedItem, x, y, draggedItemRotated);
        for (int dx = 0; dx < size.width; dx++)
        {
            for (int dy = 0; dy < size.height; dy++)
            {
                slots[x + dx, y + dy].SetGhostState(canPlace);
            }
        }
    }

    public void ClearAllGhostHighlights()
    {
        ghostHoverX = -1;
        ghostHoverY = -1;
        foreach (var slot in slots)
            slot.ResetColor();
    }

    public void TryDropDraggedItem(int x, int y)
    {
        if (!isDragging || draggedItem == null) return;
        ItemSize size = inventoryGrid.GetEffectiveSize(draggedItem, draggedItemRotated);
        x = Mathf.Clamp(x, 0, GRID_WIDTH - size.width);
        y = Mathf.Clamp(y, 0, GRID_HEIGHT - size.height);
        if (inventoryGrid.CanPlaceItemAtIgnoringSelf(draggedItem, x, y, draggedItemRotated))
        {
            inventoryGrid.MoveItem(draggedItem, x, y, draggedItemRotated);
            Debug.Log($"[Inventory] Moved {draggedItem.GetItemName()} to [{x},{y}] rotated={draggedItemRotated}");
        }
        else
        {
            Debug.Log("[Inventory] Cannot place item there — space not free.");
        }

        EndDrag();
    }

    public void CancelDrag()
    {
        EndDrag();
    }

    private void EndDrag()
    {
        isDragging = false;
        draggedItem = null;
        ghostHoverX = -1;
        ghostHoverY = -1;
        dragGhostPanel.SetActive(false);
        if (rotateButtonObject != null)
            rotateButtonObject.SetActive(false);
        ClearAllGhostHighlights();
        RefreshInventoryGrid();
    }

    private void RefreshDocumentsList()
    {
        // Destroys old buttons
        foreach (Transform child in documentsListParent)
            Destroy(child.gameObject);

        List<Item> documents = ReadableManager.Instance.GetAllReadables();
        foreach (var doc in documents)
        {
            GameObject go = Instantiate(documentButtonPrefab, documentsListParent);

            TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = doc.GetItemName();

            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                Item captured = doc;
                btn.onClick.AddListener(() => OpenDocumentReader(captured));
            }
        }
    }

    private void SetupDocumentReader()
    {
        if (documentCloseButton != null)
            documentCloseButton.onClick.AddListener(CloseDocumentReader);
    }

    private void OpenDocumentReader(Item document)
    {
        documentReaderPanel.SetActive(true);
        documentTitleText.text = document.GetItemName();
        documentBodyText.text = string.IsNullOrEmpty(document.GetReadableText())
            ? "[No text available yet]"
            : document.GetReadableText();
    }

    private void CloseDocumentReader()
    {
        documentReaderPanel.SetActive(false);
    }
}