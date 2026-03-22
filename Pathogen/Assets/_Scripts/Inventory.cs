using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 7;
    [SerializeField] private int gridHeight = 6;

    private Item[,] grid;
    private List<Item> items = new List<Item>();
    private Dictionary<Item, Vector2Int> itemPositions = new Dictionary<Item, Vector2Int>();
    private Dictionary<Item, bool> itemRotations = new Dictionary<Item, bool>();
    private Dictionary<Item, int> stackCounts = new Dictionary<Item, int>();
    public static InventoryGrid Instance { get; private set; }
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGrid();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGrid()
    {
        grid = new Item[gridWidth, gridHeight];
    }

    public bool TryAddItem(Item item)
    {
        int maxStack = item.GetMaxStackSize();

        //Try to fill an existing partial stack of the same item name
        if (maxStack > 1)
        {
            foreach (Item existing in items)
            {
                if (existing.GetItemName() == item.GetItemName() &&
                    stackCounts.TryGetValue(existing, out int count) &&
                    count < maxStack)
                {
                    stackCounts[existing] = count + 1;
                    Debug.Log($"[Inventory] Stacked {item.GetItemName()} onto existing stack ({count + 1}/{maxStack})");
                    QuestManager.Instance?.ReportItemObtained(item.GetItemName());
                    return true;
                }
            }
        }
        //No existing stack with room — place as a new stack
        for (int rotPass = 0; rotPass < 2; rotPass++)
        {
            bool rotated = (rotPass == 1);
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (CanPlaceItemAt(item, x, y, rotated))
                    {
                        PlaceItemAt(item, x, y, rotated);
                        items.Add(item);
                        stackCounts[item] = 1;
                        Debug.Log($"[Inventory] Added {item.GetItemName()} at [{x},{y}] rotated={rotated} (1/{maxStack})");
                        QuestManager.Instance?.ReportItemObtained(item.GetItemName());
                        return true;
                    }
                }
            }
        }
        Debug.Log($"[Inventory] No space for {item.GetItemName()} ({item.GetSize().width}x{item.GetSize().height})");
        return false;
    }
    /// Add multiple instances of the same item at once
    public int TryAddItemAmount(Item itemPrefab, int amount)
    {
        int remaining = amount;
        while (remaining > 0)
        {
            bool placed = TryAddItem(itemPrefab);
            if (!placed) break;
            remaining--;
        }
        return remaining;
    }

    public bool CanPlaceItemAt(Item item, int startX, int startY, bool rotated)
    {
        ItemSize size = GetEffectiveSize(item, rotated);
        if (startX + size.width > gridWidth || startY + size.height > gridHeight)
            return false;
        for (int x = startX; x < startX + size.width; x++)
        {
            for (int y = startY; y < startY + size.height; y++)
            {
                if (grid[x, y] != null)
                    return false;
            }
        }
        return true;
    }

    // Same as above but ignores the item itself (used during move/drag)
    public bool CanPlaceItemAtIgnoringSelf(Item item, int startX, int startY, bool rotated)
    {
        ItemSize size = GetEffectiveSize(item, rotated);
        if (startX + size.width > gridWidth || startY + size.height > gridHeight)
            return false;
        for (int x = startX; x < startX + size.width; x++)
        {
            for (int y = startY; y < startY + size.height; y++)
            {
                if (grid[x, y] != null && grid[x, y] != item)
                    return false;
            }
        }
        return true;
    }

    public ItemSize GetEffectiveSize(Item item, bool rotated)
    {
        ItemSize size = item.GetSize();
        if (rotated)
            return new ItemSize(size.height, size.width);
        return size;
    }

    private void PlaceItemAt(Item item, int startX, int startY, bool rotated)
    {
        itemPositions[item] = new Vector2Int(startX, startY);
        itemRotations[item] = rotated;
        ItemSize size = GetEffectiveSize(item, rotated);

        for (int x = startX; x < startX + size.width; x++)
        {
            for (int y = startY; y < startY + size.height; y++)
            {
                grid[x, y] = item;
            }
        }
    }

    public void MoveItem(Item item, int newX, int newY, bool rotated)
    {
        // Preserve stack count across the move — RemoveItemFromGrid would wipe it
        stackCounts.TryGetValue(item, out int savedCount);
        RemoveItemFromGrid(item);
        PlaceItemAt(item, newX, newY, rotated);
        if (savedCount > 0) stackCounts[item] = savedCount;
    }

    public bool IsItemRotated(Item item)
    {
        if (itemRotations.ContainsKey(item))
            return itemRotations[item];
        return false;
    }

    public Item GetItemAtPosition(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        return null;
    }

    public Vector2Int GetItemPosition(Item item)
    {
        if (itemPositions.ContainsKey(item))
            return itemPositions[item];
        return new Vector2Int(-1, -1);
    }

    private void RemoveItemFromGrid(Item item)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == item)
                    grid[x, y] = null;
            }
        }
        itemPositions.Remove(item);
        itemRotations.Remove(item);
        stackCounts.Remove(item);
    }

    /// Removes one instance from the item's stack
    public void RemoveItem(Item item)
    {
        if (!items.Contains(item)) return;
        if (stackCounts.TryGetValue(item, out int count) && count > 1)
        {
            stackCounts[item] = count - 1;
            Debug.Log($"[Inventory] Consumed one {item.GetItemName()} — {count - 1} remaining in stack.");
            return;
        }
        //Stack empty — remove the slot entirely
        RemoveItemFromGrid(item);
        items.Remove(item);
    }
    ///Removes the entire stack regardless of count (e.g. discard)
    public void RemoveItemStack(Item item)
    {
        if (!items.Contains(item)) return;
        RemoveItemFromGrid(item);
        items.Remove(item);
    }

    public bool HasItem(string itemName)
    {
        foreach (var item in items)
        {
            if (item != null && item.GetItemName() == itemName)
                return true;
        }
        return false;
    }

    public Item GetItem(string itemName)
    {
        return items.Find(item => item != null && item.GetItemName() == itemName);
    }

    public Item GetItemAt(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        return null;
    }

    public Dictionary<Vector2Int, Item> GetAllOccupiedPositions()
    {
        var positions = new Dictionary<Vector2Int, Item>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                    positions[new Vector2Int(x, y)] = grid[x, y];
            }
        }
        return positions;
    }

    public int GetStackCount(Item item)
    {
        if (stackCounts.TryGetValue(item, out int count)) return count;
        return 1;
    }
    ///Returns true if the item can be placed or stacked in the current grid
    public bool HasSpaceForItem(Item item)
    {
        if (item == null) return false;
        // If stackable, check for an existing partial stack first
        if (item.GetMaxStackSize() > 1)
        {
            foreach (Item existing in items)
            {
                if (existing.GetItemName() == item.GetItemName() &&
                    stackCounts.TryGetValue(existing, out int count) &&
                    count < existing.GetMaxStackSize())
                    return true;
            }
        }
        // Otherwise check for a free grid position
        for (int rotPass = 0; rotPass < 2; rotPass++)
        {
            bool rotated = rotPass == 1;
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    if (CanPlaceItemAt(item, x, y, rotated))
                        return true;
        }
        return false;
    }
    ///Returns total count of an item across all stacks
    public int CountItem(string itemName)
    {
        int total = 0;
        foreach (var item in items)
            if (item != null && item.GetItemName() == itemName)
                total += GetStackCount(item);
        return total;
    }

    ///Removes one from a stack — same as RemoveItem but named explicitly
    public void ConsumeOneFromStack(Item item) => RemoveItem(item);

    /// Places an item at a specific position with a known stack count
    /// Used exclusively by SaveManager when restoring inventory
    public void PlaceItemAtPublic(Item item, int x, int y, bool rotated, int stackCount)
    {
        PlaceItemAt(item, x, y, rotated);
        items.Add(item);
        stackCounts[item] = Mathf.Max(1, stackCount);
    }

    public List<Item> GetAllItems() => items;
    /// Auto-sorts all items to occupy minimum space
    /// Uses greedy bin-packing: sorts by area descending, places each item at the first available position in both orientations
    /// Preserves all items and stack counts — only positions/rotations change
    public void AutoSort()
    {
        if (items.Count == 0) return;

        // Snapshot stack counts before clearing
        var savedStacks = new Dictionary<Item, int>();
        foreach (var item in items)
            savedStacks[item] = GetStackCount(item);
        // Sort items largest area first for best packing
        var sortedItems = new List<Item>(items);
        sortedItems.Sort((a, b) =>
        {
            int aArea = a.GetSize().width * a.GetSize().height;
            int bArea = b.GetSize().width * b.GetSize().height;
            return bArea.CompareTo(aArea);
        });
        // Clear grid (keep items list intact for now)
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                grid[x, y] = null;
        itemPositions.Clear();
        itemRotations.Clear();
        stackCounts.Clear();
        // Re-place each item at first available position
        var failed = new List<Item>();
        foreach (var item in sortedItems)
        {
            bool placed = false;
            for (int rotPass = 0; rotPass < 2 && !placed; rotPass++)
            {
                bool rotated = rotPass == 1;
                // Skip rotation if it produces the same shape
                if (rotated)
                {
                    var s = item.GetSize();
                    if (s.width == s.height) break;
                }
                for (int y = 0; y < gridHeight && !placed; y++)
                    for (int x = 0; x < gridWidth && !placed; x++)
                        if (CanPlaceItemAt(item, x, y, rotated))
                        {
                            PlaceItemAt(item, x, y, rotated);
                            stackCounts[item] = savedStacks.TryGetValue(item, out int sc) ? sc : 1;
                            placed = true;
                        }
            }
            if (!placed) failed.Add(item);
        }
        // Rebuild items list in new order
        items.Clear();
        foreach (var item in sortedItems)
            if (!failed.Contains(item)) items.Add(item);
        if (failed.Count > 0) Debug.LogWarning("[AutoSort] Could not place " + failed.Count + " item(s) — inventory may be overfull.");
        Debug.Log("[AutoSort] Complete. " + items.Count + " items sorted.");
    }
    /// Expands the grid by one row (called by Hip Pouch pickup)
    /// Preserves all existing items
    public void ExpandGrid()
    {
        int newWidth = gridWidth + 1;
        Item[,] newGrid = new Item[newWidth, gridHeight];

        // Copy existing grid contents
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                newGrid[x, y] = grid[x, y];

        grid = newGrid;
        gridWidth = newWidth;
        Debug.Log("[Inventory] Grid expanded to " + gridWidth + "x" + gridHeight);
    }
}