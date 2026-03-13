using UnityEngine;

public enum ItemType
{
    KeyItem,
    Weapon,
    Consumable,
    Readable,
    Material,
    Ammo
}

[System.Serializable]
public struct ItemSize
{
    public int width;
    public int height;
    public ItemSize(int w, int h) { width = w; height = h; }
}

public class Item : InteractableBase
{
    [Header("Item Properties")]
    [SerializeField] private string itemName = "Item";
    [SerializeField]
    [TextArea(2, 4)]
    private string itemDescription = "No description.";
    [SerializeField] private ItemType itemType = ItemType.KeyItem;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private ItemSize size = new ItemSize(1, 1);
    [Header("Stacking")]
    [Tooltip("Max items per stack. Set to 1 to disable stacking.")]
    [SerializeField] private int maxStackSize = 1;
    [Header("Flags")]
    [SerializeField] private bool isStarterItem = false;
    [Header("Readable")]
    [SerializeField]
    [TextArea(3, 10)]
    private string readableText;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        promptMessage = $"E - Pick up {itemName}";

        // Show the inventory icon as the world sprite if no sprite is set manually
        if (spriteRenderer != null && spriteRenderer.sprite == null && itemIcon != null)
        {
            spriteRenderer.sprite = itemIcon;
            // Scale down to a small ground pickup size
            transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        }
    }

    public override void Interact()
    {
        if (itemType == ItemType.Readable)
        {
            bool added = ReadableManager.Instance.AddReadable(this);
            if (added)
            {
                Debug.Log($"Added document: {itemName}");
                ShowReadableText();
                gameObject.SetActive(false);
            }
        }
        else
        {
            bool pickedUp = InventoryGrid.Instance.TryAddItem(this);
            if (pickedUp)
            {
                // Refresh weapon HUD in case picked-up item is ammo for equipped weapon
                WeaponHUD.Instance?.RefreshAmmoText();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("No space in inventory! Reorganize items or discard them to make space.");
                HUDFeedback.Instance?.ShowWarning("No space in inventory!");
            }
        }
    }
    public void ShowReadableText() => Debug.Log($"=== {itemName} ===\n{readableText}\n================");
    public string GetItemName() => itemName;
    public string GetDescription() => itemDescription;
    public string GetReadableText() => readableText;
    public ItemType GetItemType() => itemType;
    public Sprite GetIcon() => itemIcon;
    public ItemSize GetSize() => size;
    public int GetMaxStackSize() => maxStackSize;
    public bool IsStarterItem() => isStarterItem;
}