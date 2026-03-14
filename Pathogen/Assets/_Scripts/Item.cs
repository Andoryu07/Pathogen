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
    [Header("World Drop")]
    [Tooltip("When dropped in the world, how many are in this pickup. Set at runtime by loot spawner.")]
    [SerializeField] private int worldStackCount = 1;
    [Tooltip("Show the item icon as a small sprite on the ground when dropped.")]
    [SerializeField] private bool showWorldSprite = true;
    [SerializeField] private float worldSpriteScale = 0.4f;
    [Header("Readable")]
    [SerializeField]
    [TextArea(3, 10)]
    private string readableText;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdatePromptMessage();
    }

    void OnEnable()
    {
        // Applies sprite immediately when activated (e.g. loot drop SetActive(true))
        ApplyWorldSprite(scaleApplied: false);
    }

    void Start()
    {
        // Applies scale, overrides any prefab-saved scale correctly
        ApplyWorldSprite(scaleApplied: true);
    }

    private void ApplyWorldSprite(bool scaleApplied)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (showWorldSprite && itemIcon != null)
        {
            spriteRenderer.sprite = itemIcon;
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 0; // render below player (player should be order 1+)
            if (scaleApplied)
                transform.localScale = new Vector3(worldSpriteScale, worldSpriteScale, 1f);
        }
        else if (!showWorldSprite)
        {
            spriteRenderer.enabled = false;
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
            //Use TryAddItemAmount for stacked world drops (e.g 7x Pistol Rounds as one pickup)
            int toAdd = Mathf.Max(1, worldStackCount);
            int leftover = InventoryGrid.Instance.TryAddItemAmount(this, toAdd);
            if (leftover < toAdd)
            {
                //At least some were added
                WeaponHUD.Instance?.RefreshAmmoText();
                if (leftover > 0)
                    HUDFeedback.Instance?.ShowWarning(
                        $"Picked up {toAdd - leftover}/{toAdd} — inventory full!");
                gameObject.SetActive(false);
            }
            else
            {
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
    private void UpdatePromptMessage()
    {
        promptMessage = worldStackCount > 1
            ? $"E - Pick up {itemName} x{worldStackCount}"
            : $"E - Pick up {itemName}";
    }
    public void SetWorldStackCount(int count)
    {
        worldStackCount = Mathf.Max(1, count);
        UpdatePromptMessage();
    }
    public int GetWorldStackCount() => worldStackCount;
}