using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.30f, 0.30f, 0.30f, 1f);
    [SerializeField] private Color ghostValidColor = new Color(0.20f, 0.80f, 0.20f, 0.45f);
    [SerializeField] private Color ghostInvalidColor = new Color(0.80f, 0.20f, 0.20f, 0.45f);
    [SerializeField] private Color hoverHighlightColor = new Color(0.30f, 0.30f, 0.30f, 1f);

    private Item currentItem;
    private int slotX;
    private int slotY;
    private bool isEmpty = true;
    private InventoryUIManager uiManager;
    private bool pointerHeld = false;
    private float holdTimer = 0f;
    private const float HoldThreshold = 0.8f;//Seconds before drag starts

    public void Initialize(int x, int y, InventoryUIManager manager)
    {
        slotX = x;
        slotY = y;
        uiManager = manager;
        ClearSlot();
        ResetColor();
    }

    public void SetItem(Item item, bool rotated = false)
    {
        currentItem = item;
        isEmpty = (item == null);

        if (!isEmpty && item.GetIcon() != null)
        {
            iconImage.sprite = item.GetIcon();
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }
        else
        {
            ClearVisuals();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        isEmpty = true;
        ClearVisuals();
    }

    private void ClearVisuals()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public void SetGhostState(bool valid)
    {
        SetBg(valid ? ghostValidColor : ghostInvalidColor);
    }

    public void ResetColor()
    {
        SetBg(normalColor);
    }

    private void SetBg(Color c)
    {
        if (backgroundImage != null)
            backgroundImage.color = c;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiManager.IsDragging)
        {
            uiManager.OnDragHoverSlot(slotX, slotY);
            return;
        }

        if (!isEmpty)
        {
            SetBg(hoverColor);
            uiManager.OnSlotHoverEnter(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager.IsDragging)
        {
            // When mouse leaves a slot during drag, clears all highlights
            uiManager.ClearAllGhostHighlights();
            return;
        }

        ResetColor();
        if (!isEmpty)
            uiManager.OnSlotHoverExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!isEmpty && !uiManager.IsDragging)
        {
            pointerHeld = true;
            holdTimer = 0f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerHeld = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (uiManager.IsDragging)
                uiManager.CancelDrag();
            return;
        }
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (uiManager.IsDragging)
        {
            uiManager.TryDropDraggedItem(slotX, slotY);
            return;
        }

        if (!isEmpty)
            uiManager.OnSlotClicked(currentItem, slotX, slotY, eventData.position);
    }

    void Update()
    {
        if (pointerHeld && !isEmpty && !uiManager.IsDragging)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= HoldThreshold)
            {
                pointerHeld = false;
                uiManager.StartDragging(currentItem);
            }
        }
    }
    public void SetHoverHighlight()
    {
        SetBg(hoverHighlightColor);
    }

    public bool IsEmpty() => isEmpty;
    public Item GetItem() => currentItem;
    public (int x, int y) GetSlotPosition() => (slotX, slotY);
}