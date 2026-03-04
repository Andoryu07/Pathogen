using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 6;
    [SerializeField] private int gridHeight = 7;
    [SerializeField] private GameObject inventoryPanel;

    private Item[,] grid;
    private List<Item> items = new List<Item>();

    public static InventoryGrid Instance { get; private set; }

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public bool TryAddItem(Item item)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CanPlaceItemAt(item, x, y))
                {
                    PlaceItemAt(item, x, y);
                    items.Add(item);
                    Debug.Log($"Added {item.GetItemName()} to position [{x},{y}]");
                    return true;
                }
            }
        }

        return false;
    }

    //Checks whether you can place the item at chosen position
    private bool CanPlaceItemAt(Item item, int startX, int startY)
    {
        ItemSize size = item.GetSize();
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

    private void PlaceItemAt(Item item, int startX, int startY)
    {
        ItemSize size = item.GetSize();

        for (int x = startX; x < startX + size.width; x++)
        {
            for (int y = startY; y < startY + size.height; y++)
            {
                grid[x, y] = item;
            }
        }

        //Later: UI representation
    }

    public void RemoveItem(Item item)
    {
        if (!items.Contains(item)) return;

        // Deletes all grids owned by this item
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == item)
                {
                    grid[x, y] = null;
                }
            }
        }

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

    private void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}