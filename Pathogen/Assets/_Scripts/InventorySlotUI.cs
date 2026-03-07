using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// PREFAB SETUP:
///   Slot (this script + Image = Background, RaycastTarget ON)
///     ├── Icon        (Image, anchor TopLeft, pivot 0,1, RaycastTarget OFF)
///     ├── BorderTop    (Image, anchor top-stretch h=2,   RaycastTarget OFF)
///     ├── BorderBottom (Image, anchor bottom-stretch h=2, RaycastTarget OFF)
///     ├── BorderLeft   (Image, anchor left-stretch w=2,   RaycastTarget OFF)
///     └── BorderRight  (Image, anchor right-stretch w=2,  RaycastTarget OFF)
/// </summary>
public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderTop;
    [SerializeField] private Image borderBottom;
    [SerializeField] private Image borderLeft;
    [SerializeField] private Image borderRight;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color ghostValidColor = new Color(0.20f, 0.75f, 0.20f, 0.45f);
    [SerializeField] private Color ghostInvalidColor = new Color(0.80f, 0.20f, 0.20f, 0.45f);
    [SerializeField] private Color borderColor = new Color(0.80f, 0.80f, 0.80f, 1f);

    private Item currentItem;
    private int slotX, slotY;
    private bool isEmpty = true;
    private bool isTopLeft = false;
    private InventoryUIManager uiManager;

    private bool pointerHeld = false;
    private float holdTimer = 0f;
    private const float HoldThreshold = 0.8f;

    // ---------------------------------------------------------------
    public void Initialize(int x, int y, InventoryUIManager manager)
    {
        slotX = x;
        slotY = y;
        uiManager = manager;
        ClearSlot();
    }

    // ---------------------------------------------------------------
    //  Set / Clear
    // ---------------------------------------------------------------
    public void SetItem(Item item, bool topLeft,
                        bool eTop, bool eBottom, bool eLeft, bool eRight)
    {
        currentItem = item;
        isEmpty = false;
        isTopLeft = topLeft;

        SetBg(normalColor);
        ShowBorder(borderTop, eTop);
        ShowBorder(borderBottom, eBottom);
        ShowBorder(borderLeft, eLeft);
        ShowBorder(borderRight, eRight);

        if (isTopLeft && item.GetIcon() != null)
        {
            iconImage.sprite = item.GetIcon();
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }
        else
        {
            ClearIcon();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        isEmpty = true;
        isTopLeft = false;
        ClearIcon();
        SetBg(normalColor);
        HideAllBorders();
    }

    private void ClearIcon()
    {
        if (iconImage == null) return;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    // Stretches icon across the full item footprint — called only on top-left tile
    public void SetIconSize(float pixelWidth, float pixelHeight)
    {
        if (iconImage == null) return;
        RectTransform rt = iconImage.GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = new Vector2(pixelWidth, pixelHeight);
    }

    // ---------------------------------------------------------------
    //  Borders
    // ---------------------------------------------------------------
    private void ShowBorder(Image img, bool show)
    {
        if (img == null) return;
        img.enabled = show;
        img.color = borderColor;
    }

    private void HideAllBorders()
    {
        if (borderTop != null) borderTop.enabled = false;
        if (borderBottom != null) borderBottom.enabled = false;
        if (borderLeft != null) borderLeft.enabled = false;
        if (borderRight != null) borderRight.enabled = false;
    }

    // ---------------------------------------------------------------
    //  Colors
    // ---------------------------------------------------------------
    public void SetGhostState(bool valid) => SetBg(valid ? ghostValidColor : ghostInvalidColor);
    public void SetHoverHighlight() => SetBg(hoverColor);
    public void ResetColor() => SetBg(normalColor);

    private void SetBg(Color c)
    {
        if (backgroundImage != null)
            backgroundImage.color = c;
    }

    // ---------------------------------------------------------------
    //  Pointer events
    // ---------------------------------------------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiManager.IsDragging) { uiManager.OnDragHoverSlot(slotX, slotY); return; }
        if (!isEmpty) uiManager.OnSlotHoverEnter(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager.IsDragging) { uiManager.ClearAllGhostHighlights(); return; }
        ResetColor();
        if (!isEmpty) uiManager.OnSlotHoverExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!isEmpty && !uiManager.IsDragging) { pointerHeld = true; holdTimer = 0f; }
    }

    public void OnPointerUp(PointerEventData eventData) => pointerHeld = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        { if (uiManager.IsDragging) uiManager.CancelDrag(); return; }

        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (uiManager.IsDragging) { uiManager.TryDropDraggedItem(slotX, slotY); return; }
        if (!isEmpty) uiManager.OnSlotClicked(currentItem, slotX, slotY, eventData.position);
    }

    void Update()
    {
        if (pointerHeld && !isEmpty && !uiManager.IsDragging)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= HoldThreshold) { pointerHeld = false; uiManager.StartDragging(currentItem); }
        }
    }

    public bool IsEmpty() => isEmpty;
    public Item GetItem() => currentItem;
    public (int x, int y) GetSlotPosition() => (slotX, slotY);
}