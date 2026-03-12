using UnityEngine;
using System.Collections.Generic;

/// Global singleton that owns the shared storebox item pool
/// All Storebox interactables in the world point to this one list
public class StoreboxManager : MonoBehaviour
{
    public static StoreboxManager Instance { get; private set; }

    // Items currently stored — kept as prefab references (inactive GameObjects)
    private List<Item> storedItems = new List<Item>();
    private Dictionary<Item, int> stackCounts = new Dictionary<Item, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }
    public List<Item> GetStoredItems() => storedItems;
    ///Move an item from the player inventory into the storebox
    public bool StoreItem(Item item)
    {
        if (item == null) return false;
        // Decrement from inventory (handles stack logic on the inventory side)
        InventoryGrid.Instance.RemoveItem(item);
        // Try to stack onto an existing stored stack of the same item name
        if (item.GetMaxStackSize() > 1)
        {
            foreach (Item stored in storedItems)
            {
                if (stored.GetItemName() == item.GetItemName() &&
                    stackCounts.TryGetValue(stored, out int count) &&
                    count < stored.GetMaxStackSize())
                {
                    stackCounts[stored] = count + 1;
                    Debug.Log($"[Storebox] Stacked {item.GetItemName()} ({count + 1}/{stored.GetMaxStackSize()})");
                    CheckUnequipWeapon(item);
                    return true;
                }
            }
        }
        // No existing stack — add as new entry
        item.gameObject.SetActive(false);
        storedItems.Add(item);
        stackCounts[item] = 1;
        Debug.Log($"[Storebox] Stored: {item.GetItemName()}");

        CheckUnequipWeapon(item);
        return true;
    }

    private static void CheckUnequipWeapon(Item item)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null || player.EquippedWeapon != item) return;
        player.EquipWeapon(null);
        if (WeaponHUD.Instance != null) WeaponHUD.Instance.Refresh(null);
        Debug.Log("[Storebox] Equipped weapon was stored — slot cleared.");
    }

    /// Move one instance from the storebox back into the player inventory,returns true on success, false if inventory is full
    public bool WithdrawItem(Item item)
    {
        if (item == null || !storedItems.Contains(item)) return false;

        bool added = InventoryGrid.Instance.TryAddItem(item);
        if (!added)
        {
            Debug.Log($"[Storebox] Cannot withdraw {item.GetItemName()} — inventory full.");
            return false;
        }
        // Decrement stack; remove slot entirely when empty
        if (stackCounts.TryGetValue(item, out int count) && count > 1)
        {
            stackCounts[item] = count - 1;
            Debug.Log($"[Storebox] Withdrew one {item.GetItemName()} — {count - 1} remaining.");
        }
        else
        {
            storedItems.Remove(item);
            stackCounts.Remove(item);
            Debug.Log($"[Storebox] Withdrew: {item.GetItemName()} (stack empty)");
        }
        return true;
    }
    ///Returns the current stack count for a stored item
    public int GetStackCount(Item item)
    {
        if (stackCounts.TryGetValue(item, out int count)) return count;
        return 1;
    }
}