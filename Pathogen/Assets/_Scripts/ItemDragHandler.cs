using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;

    private InventoryUIManager uiManager;
    private InventoryGrid inventoryGrid;
    private Item draggedItem;
    private Vector2Int originalPosition;
    private Vector2Int itemSize;
    private bool isRotated = false;

    // Drag preview
    private GameObject dragPreviewObject;
    private RectTransform dragPreviewRect;
    private Image dragPreviewImage;

    void Awake()
    {
        uiManager = GetComponentInParent<InventoryUIManager>();
        inventoryGrid = InventoryGrid.Instance;

        // Najdeme canvas
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        // Vytvoření preview objektu
        CreateDragPreview();
    }

    void CreateDragPreview()
    {
        dragPreviewObject = new GameObject("DragPreview");
        dragPreviewObject.transform.SetParent(canvas.transform, false);

        dragPreviewRect = dragPreviewObject.AddComponent<RectTransform>();
        dragPreviewImage = dragPreviewObject.AddComponent<Image>();
        dragPreviewImage.color = new Color(1, 1, 1, 0.7f);
        dragPreviewImage.raycastTarget = false; // Aby neblokovalo raycasty

        dragPreviewObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Zjistíme, jestli klikáme na slot s předmětem
        InventorySlotUI slot = GetComponent<InventorySlotUI>();
        if (slot != null && !slot.IsEmpty())
        {
            draggedItem = slot.GetItem();
            originalPosition = slot.GetSlotPosition();
            itemSize = draggedItem.GetSize();
            isRotated = false;

            // Nastavení preview
            dragPreviewImage.sprite = draggedItem.GetIcon();
            UpdatePreviewSize();
            dragPreviewRect.position = eventData.position;
            dragPreviewObject.SetActive(true);

            // Dočasně schováme původní ikonu
            slot.HideIcon();

            eventData.Use();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            // Pohyb preview s myší
            dragPreviewRect.position = eventData.position;

            // Rotace na klávesu R
            if (Input.GetKeyDown(KeyCode.R))
            {
                isRotated = !isRotated;
                UpdatePreviewSize();
            }

            // Zobrazení možné pozice
            Vector2Int hoveredSlot = GetHoveredSlot();
            if (hoveredSlot.x >= 0 && hoveredSlot.y >= 0)
            {
                bool canPlace = inventoryGrid.CanPlaceItemAt(
                    draggedItem,
                    hoveredSlot.x,
                    hoveredSlot.y,
                    isRotated
                );

                // Změna barvy preview podle možnosti umístění
                dragPreviewImage.color = canPlace ?
                    new Color(0, 1, 0, 0.5f) :
                    new Color(1, 0, 0, 0.5f);
            }

            eventData.Use();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            Vector2Int targetSlot = GetHoveredSlot();

            // Obnovíme ikonu v původním slotu
            InventorySlotUI originalSlot = GetComponent<InventorySlotUI>();
            if (originalSlot != null)
            {
                originalSlot.ShowIcon();
            }

            if (targetSlot.x >= 0 && targetSlot.y >= 0)
            {
                // Pokus o přesun
                bool success = inventoryGrid.MoveItem(
                    draggedItem,
                    originalPosition.x, originalPosition.y,
                    targetSlot.x, targetSlot.y,
                    isRotated
                );

                if (success)
                {
                    Debug.Log($"Předmět přesunut na [{targetSlot.x}, {targetSlot.y}]");
                }
                else
                {
                    Debug.Log("Nelze umístit předmět na tuto pozici");
                }

                // Obnovíme celý grid
                uiManager.RefreshInventoryGrid();
            }

            // Ukončení dragování
            draggedItem = null;
            dragPreviewObject.SetActive(false);
        }

        eventData.Use();
    }

    private void UpdatePreviewSize()
    {
        Vector2Int currentSize = isRotated ?
            new Vector2Int(itemSize.y, itemSize.x) :
            itemSize;

        float tileSize = uiManager.GetTileSize();
        dragPreviewRect.sizeDelta = new Vector2(
            currentSize.x * tileSize,
            currentSize.y * tileSize
        );
    }

    private Vector2Int GetHoveredSlot()
    {
        // Zjistí, nad kterým slotem je myš
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            InventorySlotUI slot = result.gameObject.GetComponent<InventorySlotUI>();
            if (slot != null)
            {
                return slot.GetSlotPosition();
            }
        }

        return new Vector2Int(-1, -1);
    }
}