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
        // Tries normal orientation first, then rotated
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
                        Debug.Log($"Added {item.GetItemName()} at [{x},{y}] rotated={rotated}");
                        return true;
                    }
                }
            }
        }
        Debug.Log($"No space found for {item.GetItemName()} (size {item.GetSize().width}x{item.GetSize().height})");
        return false;
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
        RemoveItemFromGrid(item);
        PlaceItemAt(item, newX, newY, rotated);
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
    }

    public void RemoveItem(Item item)
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

    public List<Item> GetAllItems() => items;
}