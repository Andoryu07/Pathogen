using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private Item currentItem;
    private int slotX;
    private int slotY;
    private bool isEmpty = true;
    private bool isSelected = false;
    private InventoryUIManager uiManager;

    public void Initialize(int x, int y, InventoryUIManager manager)
    {
        slotX = x;
        slotY = y;
        uiManager = manager;
        ClearSlot();
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        isEmpty = (item == null);

        if (!isEmpty && item.GetIcon() != null)
        {
            iconImage.sprite = item.GetIcon();
            iconImage.enabled = true;
            iconImage.color = normalColor;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        isEmpty = true;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        backgroundImage.color = selected ? selectedColor : normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEmpty)
        {
            uiManager.SelectItem(currentItem, slotX, slotY);
        }
    }

    public bool IsEmpty() => isEmpty;
    public Item GetItem() => currentItem;
    public (int x, int y) GetSlotPosition() => (slotX, slotY);

}