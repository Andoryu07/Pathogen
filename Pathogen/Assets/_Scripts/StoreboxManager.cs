using UnityEngine;
using System.Collections.Generic;
/// Global singleton that owns the shared storebox item pool.
/// All Storebox interactables in the world point to this one list.
public class StoreboxManager : MonoBehaviour
{
    public static StoreboxManager Instance { get; private set; }

    // Items currently stored — kept as prefab references (inactive GameObjects)
    private List<Item> storedItems = new List<Item>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public List<Item> GetStoredItems() => storedItems;

    //Move an item from the player inventory into the storebox
    public bool StoreItem(Item item)
    {
        if (item == null) return false;
        InventoryGrid.Instance.RemoveItem(item);
        item.gameObject.SetActive(false);
        storedItems.Add(item);
        Debug.Log($"[Storebox] Stored: {item.GetItemName()}");
        return true;
    }
    /// Move an item from the storebox back into the player inventory
    /// Returns true on success, false if inventory is full
    public bool WithdrawItem(Item item)
    {
        if (item == null || !storedItems.Contains(item)) return false;
        bool added = InventoryGrid.Instance.TryAddItem(item);
        if (added)
        {
            storedItems.Remove(item);
            Debug.Log($"[Storebox] Withdrew: {item.GetItemName()}");
        }
        else
        {
            Debug.Log($"[Storebox] Cannot withdraw {item.GetItemName()} — inventory full.");
        }
        return added;
    }
}