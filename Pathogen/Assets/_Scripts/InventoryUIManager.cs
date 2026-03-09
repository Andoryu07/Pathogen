using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }
    [Header("Main Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [Header("Tabs")]
    [SerializeField] private Button inventoryTabButton;
    [SerializeField] private Button documentsTabButton;
    [SerializeField] private Button tutorialsTabButton;
    [SerializeField] private Button collectiblesTabButton;
    [SerializeField] private Button craftingTabButton;
    [SerializeField] private GameObject inventoryGridPanel;
    [SerializeField] private GameObject documentsPanel;
    [SerializeField] private GameObject tutorialsPanel;
    [SerializeField] private GameObject collectiblesPanel;
    [SerializeField] private GameObject craftingPanel;
    [Header("Crafting")]
    [SerializeField] private Transform craftingListParent;
    [SerializeField] private GameObject craftingRecipePrefab;
    [SerializeField] private GameObject craftingSlotPrefab;
    [SerializeField] private GameObject craftingSeparatorPrefab;
    [Header("Grid Settings")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private float gridMargin = 0f;
    private const int GRID_WIDTH = 7;
    private const int GRID_HEIGHT = 6;
    private float tileSize = 64f;
    [Header("Hover Panel")]
    [SerializeField] private GameObject hoverPanel;
    [SerializeField] private TextMeshProUGUI hoverItemNameText;
    [SerializeField] private TextMeshProUGUI hoverItemDescText;
    [Header("Click Panel")]
    [SerializeField] private GameObject clickPanel;
    [SerializeField] private Button useEquipButton;
    [SerializeField] private TextMeshProUGUI useEquipButtonText;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button replaceButton;
    [SerializeField] private RectTransform clickPanelRect;
    [Header("Documents")]
    [SerializeField] private Transform documentsListParent;
    [SerializeField] private GameObject documentButtonPrefab;
    [SerializeField] private GameObject documentReaderPanel;
    [SerializeField] private TextMeshProUGUI documentTitleText;
    [SerializeField] private TextMeshProUGUI documentBodyText;
    [SerializeField] private Button documentCloseButton;
    [Header("Tutorials")]
    [SerializeField] private Transform tutorialsListParent;
    [SerializeField] private GameObject tutorialButtonPrefab;
    [SerializeField] private TextMeshProUGUI tutorialTitleText;
    [SerializeField] private TextMeshProUGUI tutorialBodyText;
    [Header("Collectibles")]
    [SerializeField] private TextMeshProUGUI collectiblesCountText;
    [SerializeField] private Transform collectibleRewardsParent;
    [SerializeField] private GameObject collectibleRewardPrefab;
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
    private enum ActiveTab { Inventory, Documents, Tutorials, Collectibles, Crafting }
    private ActiveTab activeTab = ActiveTab.Inventory;
    private Item clickedItem;
    private Item draggedItem;
    private bool draggedItemRotated;
    private bool isDragging = false;
    public bool IsDragging => isDragging;
    private int ghostHoverX = -1;
    private int ghostHoverY = -1;
    private string selectedTutorialTitle = "";
    private string selectedTutorialBody = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        inventoryGrid = InventoryGrid.Instance;
    }

    void Start()
    {
        if (inventoryGrid == null) inventoryGrid = InventoryGrid.Instance;
        SetupGrid();
        SetupTabs();
        SetupClickPanel();
        SetupDocumentReader();
        if (rotateButton != null) rotateButton.onClick.AddListener(RotateDraggedItem);
        hoverPanel.SetActive(false);
        clickPanel.SetActive(false);
        dragGhostPanel.SetActive(false);
        documentReaderPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        if (rotateButtonObject != null) rotateButtonObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ToggleInventory();
        if (isDragging)
        {
            UpdateDragGhostPosition();
            if (Input.GetKeyDown(KeyCode.R)) RotateDraggedItem();
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) CancelDrag();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (documentReaderPanel.activeSelf) CloseDocumentReader();
                else if (clickPanel.activeSelf) clickPanel.SetActive(false);
            }
        }
    }

    private void SetupGrid()
    {
        Canvas.ForceUpdateCanvases();
        RectTransform cr = gridParent.GetComponent<RectTransform>();
        float tileW = Mathf.Floor((cr.rect.width - gridMargin * 2) / GRID_WIDTH);
        float tileH = Mathf.Floor((cr.rect.height - gridMargin * 2) / GRID_HEIGHT);
        tileSize = Mathf.Min(tileW, tileH);

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

        slots = new InventorySlotUI[GRID_WIDTH, GRID_HEIGHT];
        for (int y = 0; y < GRID_HEIGHT; y++)
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                GameObject go = Instantiate(slotPrefab, gridParent);
                InventorySlotUI slot = go.GetComponent<InventorySlotUI>();
                slot.Initialize(x, y, this);
                slots[x, y] = slot;
            }
    }

    private void SetupTabs()
    {
        inventoryTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Inventory));
        documentsTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Documents));
        if (tutorialsTabButton != null) tutorialsTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Tutorials));
        if (collectiblesTabButton != null) collectiblesTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Collectibles));
        if (craftingTabButton != null) craftingTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Crafting));
    }

    private void SwitchTab(ActiveTab tab)
    {
        activeTab = tab;
        inventoryGridPanel.SetActive(tab == ActiveTab.Inventory);
        documentsPanel.SetActive(tab == ActiveTab.Documents);
        if (tutorialsPanel != null) tutorialsPanel.SetActive(tab == ActiveTab.Tutorials);
        if (collectiblesPanel != null) collectiblesPanel.SetActive(tab == ActiveTab.Collectibles);
        if (craftingPanel != null) craftingPanel.SetActive(tab == ActiveTab.Crafting);
        clickPanel.SetActive(false);
        hoverPanel.SetActive(false);
        switch (tab)
        {
            case ActiveTab.Documents: RefreshDocumentsList(); break;
            case ActiveTab.Tutorials: RefreshTutorialsList(); break;
            case ActiveTab.Collectibles: RefreshCollectibles(); break;
            case ActiveTab.Crafting: RefreshCraftingList(); break;
        }
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        if (inventoryPanel.activeSelf) { RefreshInventoryGrid(); SwitchTab(activeTab); }
        else { clickPanel.SetActive(false); hoverPanel.SetActive(false); CancelDrag(); }
    }

    public void RefreshInventoryGrid()
    {
        if (inventoryGrid == null)
        {
            inventoryGrid = InventoryGrid.Instance;
            if (inventoryGrid == null) { Debug.LogError("[InventoryUI] InventoryGrid missing!"); return; }
        }

        foreach (var slot in slots) slot.ClearSlot();

        foreach (var item in inventoryGrid.GetAllItems())
        {
            Vector2Int pos = inventoryGrid.GetItemPosition(item);
            bool rotated = inventoryGrid.IsItemRotated(item);
            if (pos.x < 0) continue;
            ItemSize size = inventoryGrid.GetEffectiveSize(item, rotated);

            for (int dx = 0; dx < size.width; dx++)
                for (int dy = 0; dy < size.height; dy++)
                {
                    int sx = pos.x + dx, sy = pos.y + dy;
                    if (sx >= GRID_WIDTH || sy >= GRID_HEIGHT) continue;

                    bool isTopLeft = (dx == 0 && dy == 0);
                    bool eTop = (dy == 0);
                    bool eBottom = (dy == size.height - 1);
                    bool eLeft = (dx == 0);
                    bool eRight = (dx == size.width - 1);

                    slots[sx, sy].SetItem(item, isTopLeft, eTop, eBottom, eLeft, eRight);

                    if (isTopLeft)
                        slots[sx, sy].SetIconSize(size.width * tileSize, size.height * tileSize);
                }
        }
    }

    public void OnSlotHoverEnter(Item item)
    {
        if (isDragging) return;
        hoverItemNameText.text = item.GetItemName();
        hoverItemDescText.text = item.GetDescription();
        hoverPanel.SetActive(true);

        Vector2Int pos = inventoryGrid.GetItemPosition(item);
        bool rotated = inventoryGrid.IsItemRotated(item);
        ItemSize size = inventoryGrid.GetEffectiveSize(item, rotated);
        if (pos.x < 0) return;

        for (int dx = 0; dx < size.width; dx++)
            for (int dy = 0; dy < size.height; dy++)
            {
                int sx = pos.x + dx, sy = pos.y + dy;
                if (sx < GRID_WIDTH && sy < GRID_HEIGHT)
                    slots[sx, sy].SetHoverHighlight();
            }
    }

    public void OnSlotHoverExit()
    {
        if (isDragging) return;
        hoverPanel.SetActive(false);
        ClearAllGhostHighlights();
    }

    private void SetupClickPanel()
    {
        useEquipButton.onClick.AddListener(OnUseEquipClicked);
        discardButton.onClick.AddListener(OnDiscardClicked);
        replaceButton.onClick.AddListener(OnReplaceClicked);
    }

    public void OnSlotClicked(Item item, int x, int y, Vector2 screenPos)
    {
        if (clickPanel.activeSelf && clickedItem == item) { clickPanel.SetActive(false); return; }
        clickedItem = item;
        ItemType type = item.GetItemType();

        // Use/Equip: only Weapons and Consumables have actions — everything else is greyed out
        bool canUseEquip = (type == ItemType.Weapon || type == ItemType.Consumable);
        useEquipButton.interactable = canUseEquip;
        if (type == ItemType.Weapon) useEquipButtonText.text = "Equip";
        else if (type == ItemType.Material) useEquipButtonText.text = "Craft Only";
        else if (type == ItemType.KeyItem) useEquipButtonText.text = "Key Item";
        else useEquipButtonText.text = "Use";

        // Discard: blocked for key items and the starter weapon (Crowbar)
        discardButton.interactable = !(type == ItemType.KeyItem || item.IsStarterItem());
        clickPanel.SetActive(true);
        PositionClickPanel(screenPos);
        hoverPanel.SetActive(false);
    }

    private void PositionClickPanel(Vector2 screenPos)
    {
        if (parentCanvas == null) return;
        float scale = parentCanvas.scaleFactor;
        Vector2 lp = new Vector2(screenPos.x / scale, screenPos.y / scale) + new Vector2(2f, -2f);
        Vector2 ps = clickPanelRect.sizeDelta;
        RectTransform crt = parentCanvas.GetComponent<RectTransform>();
        lp.x = Mathf.Clamp(lp.x, 0f, crt.rect.width - ps.x);
        lp.y = Mathf.Clamp(lp.y, ps.y, crt.rect.height);
        clickPanelRect.anchoredPosition = lp;
    }

    private void OnUseEquipClicked()
    {
        if (clickedItem == null) return;
        Item item = clickedItem;
        clickPanel.SetActive(false);
        clickedItem = null;
        hoverPanel.SetActive(false);

        if (item.GetItemType() == ItemType.Weapon)
        {
            // Equip — store on PlayerController and refresh HUD
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null) player.EquipWeapon(item);
            if (WeaponHUD.Instance != null) WeaponHUD.Instance.Refresh(item);
        }
        else if (item.GetItemType() == ItemType.Consumable)
        {
            if (UseItemHandler.Instance == null)
            {
                Debug.LogWarning("[Inventory] UseItemHandler not found in scene!");
                return;
            }
            bool consumed = UseItemHandler.Instance.TryUseItem(item);
            if (consumed)
            {
                inventoryGrid.RemoveItem(item);
                RefreshInventoryGrid();
            }
        }
        else
        {
            Debug.Log($"[Inventory] {item.GetItemName()} cannot be used directly.");
        }
    }

    private void OnDiscardClicked()
    {
        if (clickedItem == null) return;
        if (clickedItem.GetItemType() == ItemType.KeyItem || clickedItem.IsStarterItem()) return;

        // If discarding the equipped weapon, clear the weapon slot
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.EquippedWeapon == clickedItem)
        {
            player.EquipWeapon(null);
            if (WeaponHUD.Instance != null) WeaponHUD.Instance.Refresh(null);
        }

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
        clickPanel.SetActive(false); clickedItem = null;
        StartDragging(toMove);
    }

    public void StartDragging(Item item)
    {
        isDragging = true;
        draggedItem = item;
        draggedItemRotated = inventoryGrid.IsItemRotated(item);
        hoverPanel.SetActive(false); clickPanel.SetActive(false);
        if (item.GetIcon() != null) dragGhostImage.sprite = item.GetIcon();
        dragGhostImage.color = new Color(1f, 1f, 1f, 0.75f);
        dragGhostPanel.SetActive(true);
        dragGhostRect.anchoredPosition = new Vector2(-9999f, -9999f);
        if (rotateButtonObject != null) rotateButtonObject.SetActive(true);
        UpdateDragGhostSize();
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
        float scale = parentCanvas.scaleFactor;
        Vector2 lp = new Vector2(Input.mousePosition.x / scale, Input.mousePosition.y / scale);
        ItemSize size = inventoryGrid.GetEffectiveSize(draggedItem, draggedItemRotated);
        lp -= new Vector2(size.width * tileSize * 0.5f, -size.height * tileSize * 0.5f);
        dragGhostRect.anchoredPosition = lp;
        UpdateGhostHighlightFromMouse();
    }

    // Drives ghost highlighting purely from mouse position in grid space — avoids all pointer-event edge cases that caused out-of-bounds issues.
    private void UpdateGhostHighlightFromMouse()
    {
        RectTransform gridRT = gridParent.GetComponent<RectTransform>();
        Camera uiCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                           ? null : parentCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRT, Input.mousePosition, uiCam, out Vector2 local))
        {
            if (ghostHoverX >= 0) ClearAllGhostHighlights();
            return;
        }

        float gridPixelW = GRID_WIDTH * tileSize;
        float gridPixelH = GRID_HEIGHT * tileSize;
        float fromLeft = local.x + gridPixelW * 0.5f - gridMargin;
        float fromTop = gridPixelH * 0.5f - local.y - gridMargin;

        int cellX = Mathf.FloorToInt(fromLeft / tileSize);
        int cellY = Mathf.FloorToInt(fromTop / tileSize);

        if (cellX < 0 || cellX >= GRID_WIDTH || cellY < 0 || cellY >= GRID_HEIGHT)
        {
            if (ghostHoverX >= 0) ClearAllGhostHighlights();
            return;
        }

        OnDragHoverSlot(cellX, cellY);
    }

    public void OnDragHoverSlot(int x, int y)
    {
        if (draggedItem == null) return;
        ItemSize size = inventoryGrid.GetEffectiveSize(draggedItem, draggedItemRotated);
        x = Mathf.Clamp(x, 0, GRID_WIDTH - size.width);
        y = Mathf.Clamp(y, 0, GRID_HEIGHT - size.height);
        if (ghostHoverX == x && ghostHoverY == y) return;
        ghostHoverX = x; ghostHoverY = y;
        ClearAllGhostHighlights();
        bool canPlace = inventoryGrid.CanPlaceItemAtIgnoringSelf(draggedItem, x, y, draggedItemRotated);
        for (int dx = 0; dx < size.width; dx++)
            for (int dy = 0; dy < size.height; dy++)
                slots[x + dx, y + dy].SetGhostState(canPlace);
    }

    public void ClearAllGhostHighlights()
    {
        ghostHoverX = -1; ghostHoverY = -1;
        if (slots == null) return;
        foreach (var slot in slots) slot.ResetColor();
    }

    public void TryDropDraggedItem(int x, int y)
    {
        if (!isDragging || draggedItem == null) return;
        ItemSize size = inventoryGrid.GetEffectiveSize(draggedItem, draggedItemRotated);
        x = Mathf.Clamp(x, 0, GRID_WIDTH - size.width);
        y = Mathf.Clamp(y, 0, GRID_HEIGHT - size.height);
        if (inventoryGrid.CanPlaceItemAtIgnoringSelf(draggedItem, x, y, draggedItemRotated))
            inventoryGrid.MoveItem(draggedItem, x, y, draggedItemRotated);
        else
            Debug.Log("[Inventory] Cannot place item there.");
        EndDrag();
    }

    public void CancelDrag() => EndDrag();

    private void EndDrag()
    {
        isDragging = false; draggedItem = null;
        ghostHoverX = -1; ghostHoverY = -1;
        dragGhostPanel.SetActive(false);
        if (rotateButtonObject != null) rotateButtonObject.SetActive(false);
        ClearAllGhostHighlights();
        RefreshInventoryGrid();
    }

    private void RotateDraggedItem()
    {
        if (!isDragging) return;
        draggedItemRotated = !draggedItemRotated;
        UpdateDragGhostSize();
        if (ghostHoverX >= 0) OnDragHoverSlot(ghostHoverX, ghostHoverY);
    }

    private void SetupDocumentReader()
    {
        if (documentCloseButton != null) documentCloseButton.onClick.AddListener(CloseDocumentReader);
    }

    private void RefreshDocumentsList()
    {
        foreach (Transform c in documentsListParent) Destroy(c.gameObject);
        EnsureVerticalList(documentsListParent, 40f);
        foreach (var doc in ReadableManager.Instance.GetAllReadables())
        {
            GameObject go = Instantiate(documentButtonPrefab, documentsListParent);
            ForceFullWidth(go, 40f);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = doc.GetItemName();
            var btn = go.GetComponent<Button>();
            if (btn != null) { Item cap = doc; btn.onClick.AddListener(() => OpenDocumentReader(cap)); }
        }
    }

    private void OpenDocumentReader(Item doc)
    {
        documentReaderPanel.SetActive(true);
        documentTitleText.text = doc.GetItemName();
        documentBodyText.text = string.IsNullOrEmpty(doc.GetReadableText())
            ? "[No text available yet]" : doc.GetReadableText();
    }

    private void CloseDocumentReader() => documentReaderPanel.SetActive(false);

    private void RefreshTutorialsList()
    {
        if (tutorialsListParent == null) return;
        foreach (Transform c in tutorialsListParent) Destroy(c.gameObject);
        EnsureVerticalList(tutorialsListParent, 44f);

        var tutorials = TutorialManager.Instance.GetAllTutorials();
        foreach (var tut in tutorials)
        {
            GameObject go = Instantiate(tutorialButtonPrefab, tutorialsListParent);
            ForceFullWidth(go, 44f);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = tut.title;
            var btn = go.GetComponent<Button>();
            if (btn != null) { TutorialEntry cap = tut; btn.onClick.AddListener(() => ShowTutorial(cap)); }
        }

        if (!string.IsNullOrEmpty(selectedTutorialTitle))
        {
            if (tutorialTitleText != null) tutorialTitleText.text = selectedTutorialTitle;
            if (tutorialBodyText != null) tutorialBodyText.text = selectedTutorialBody;
        }
        else if (tutorials.Count > 0) ShowTutorial(tutorials[0]);
    }

    private void ShowTutorial(TutorialEntry entry)
    {
        selectedTutorialTitle = entry.title;
        selectedTutorialBody = entry.body;
        if (tutorialTitleText != null) tutorialTitleText.text = entry.title;
        if (tutorialBodyText != null) tutorialBodyText.text = entry.body;
    }

    public void AddTutorial(string title, string body)
    {
        TutorialManager.Instance.AddTutorial(title, body);
        if (activeTab == ActiveTab.Tutorials) RefreshTutorialsList();
    }

    private void RefreshCollectibles()
    {
        if (collectiblesCountText == null) return;
        int collected = TalismanManager.Instance.CollectedCount;
        int total = TalismanManager.Instance.TotalCount;
        collectiblesCountText.text = $"Talismans:  {collected} / {total}";

        if (collectibleRewardsParent == null || collectibleRewardPrefab == null) return;

        List<Transform> old = new List<Transform>();
        foreach (Transform c in collectibleRewardsParent) old.Add(c);
        foreach (Transform c in old) DestroyImmediate(c.gameObject);

        EnsureVerticalList(collectibleRewardsParent, 56f);

        var rewards = new (int n, string lbl, string fx)[]
        {
            (1,  "1 Talisman",   "+10% Max Stamina"),
            (5,  "5 Talismans",  "+20% Reload Speed"),
            (10, "10 Talismans", "+30% Ranged Damage"),
        };

        foreach (var r in rewards)
        {
            bool unlocked = collected >= r.n;
            GameObject go = Instantiate(collectibleRewardPrefab, collectibleRewardsParent);
            ForceFullWidth(go, 56f);

            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2) { texts[0].text = r.lbl; texts[1].text = r.fx; }
            else if (texts.Length == 1) texts[0].text = $"{r.lbl}  —  {r.fx}";

            var img = go.GetComponent<Image>();
            if (img != null) img.color = unlocked
                ? new Color(0.15f, 0.55f, 0.15f, 0.65f)
                : new Color(0.22f, 0.22f, 0.22f, 0.65f);

            Color tc = unlocked ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
            foreach (var t in texts) t.color = tc;
        }
    }

    public void RefreshCraftingList()
    {
        if (craftingListParent == null || craftingRecipePrefab == null) return;

        if (CraftingManager.Instance == null)
        {
            Debug.LogWarning("[InventoryUI] CraftingManager not found in scene!");
            return;
        }
        foreach (Transform c in craftingListParent) Destroy(c.gameObject);
        EnsureVerticalList(craftingListParent, CraftingRecipeUI.ROW_HEIGHT);

        List<CraftingRecipe> visible = CraftingManager.Instance.GetVisibleRecipes();
        foreach (var recipe in visible)
        {
            GameObject go = Instantiate(craftingRecipePrefab, craftingListParent);
            ForceFullWidth(go, CraftingRecipeUI.ROW_HEIGHT);

            if (go.GetComponent<Image>() == null)
                go.AddComponent<Image>();

            var recipeUI = go.GetComponent<CraftingRecipeUI>();
            if (recipeUI != null)
                recipeUI.Setup(recipe);
        }
    }

    // Ensures Content has a VerticalLayoutGroup + ContentSizeFitter and forces the Content RectTransform to stretch horizontally so children can fill the full scroll area width
    private static void EnsureVerticalList(Transform content, float itemHeight)
    {
        // Force Content to stretch to full width of the ScrollView viewport
        RectTransform crt = content.GetComponent<RectTransform>();
        if (crt != null)
        {
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = new Vector2(0f, crt.offsetMin.y);
            crt.offsetMax = new Vector2(0f, crt.offsetMax.y);
        }
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false;   // must be false — height comes from LayoutElement
        vlg.childForceExpandHeight = false;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        // ContentSizeFitter grows Content vertically to fit all children
        var csf = content.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = content.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    // Forces a prefab instance to fill the list width with a fixed height, the LayoutElement.preferredHeight is what VerticalLayoutGroup reads when childControlHeight = false
    private static void ForceFullWidth(GameObject go, float height)
    {
        // LayoutElement tells the parent VerticalLayoutGroup the desired size
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
        le.flexibleWidth = 1f;
        //RectTransform as a fallback
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, height);
    }
}