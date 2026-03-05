using UnityEngine;

public enum ItemType
{
    KeyItem, 
    Weapon,    
    Consumable, 
    Readable   
}

[System.Serializable]
public struct ItemSize
{
    public int width;
    public int height;

    public ItemSize(int w, int h)
    {
        width = w;
        height = h;
    }
}

public class Item : InteractableBase
{
    [Header("Item Properties")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private ItemType itemType = ItemType.KeyItem;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private ItemSize size = new ItemSize(1, 1);

    [Header("Readable")]
    [SerializeField][TextArea(3, 10)] private string readableText;

    private SpriteRenderer spriteRenderer;

    public string GetReadableText()
    {
        return readableText;
    }
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        promptMessage = $"E - Pick up {itemName}";
    }

    public override void Interact()
    {
        if (itemType == ItemType.Readable)
        {
            //Readables are not stored in the inventory
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
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("No space in inventory! Reorganize items or discard them to make space");
                //Later: Automatically opens inventory, challenging player to reorganize inventory/get rid of items
            }
        }
    }

    public void ShowReadableText()
    {
        Debug.Log($"=== {itemName} ===\n{readableText}\n================");
        //Later: UI panel for reading
    }

    public string GetItemName() => itemName;
    public ItemType GetItemType() => itemType;
    public Sprite GetIcon() => itemIcon;
    public ItemSize GetSize() => size;
}