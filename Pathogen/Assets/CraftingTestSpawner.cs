using UnityEngine;
using System.Collections.Generic;


public class CraftingTestSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TestItem
    {
        public Item itemPrefab;
        public int amount = 1;
    }

    [Header("Test Items to Spawn")]
    [SerializeField] private List<TestItem> testItems = new List<TestItem>();

    [Header("Settings")]
    [SerializeField] private KeyCode spawnKey = KeyCode.F5;
    [SerializeField] private KeyCode clearKey = KeyCode.F6;
    [SerializeField] private bool showOnScreen = true;

    private bool spawned = false;

    void Update()
    {
        if (Input.GetKeyDown(spawnKey)) SpawnAll();
        if (Input.GetKeyDown(clearKey)) ClearAll();
    }

    void OnGUI()
    {
        if (!showOnScreen) return;
        GUI.Label(new Rect(10, 10, 300, 20), $"[F5] Spawn test items   [F6] Clear inventory");
        if (spawned)
            GUI.Label(new Rect(10, 28, 300, 20), "✓ Test items spawned — open inventory to craft");
    }

    private void SpawnAll()
    {
        if (InventoryGrid.Instance == null) { Debug.LogWarning("[TestSpawner] InventoryGrid not found!"); return; }

        int added = 0;
        foreach (var entry in testItems)
        {
            if (entry.itemPrefab == null) continue;
            for (int i = 0; i < entry.amount; i++)
            {
                Item instance = Instantiate(entry.itemPrefab);
                instance.gameObject.SetActive(false);   // keep it out of the world
                bool ok = InventoryGrid.Instance.TryAddItem(instance);
                if (ok) added++;
                else
                {
                    Destroy(instance.gameObject);
                    Debug.Log($"[TestSpawner] Inventory full — could not add {entry.itemPrefab.GetItemName()}");
                }
            }
        }

        spawned = true;
        Debug.Log($"[TestSpawner] Spawned {added} test items into inventory.");

        // Refresh UI if inventory is open
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RefreshInventoryGrid();
            InventoryUIManager.Instance.RefreshCraftingList();
        }
    }

    private void ClearAll()
    {
        if (InventoryGrid.Instance == null) return;

        var items = new List<Item>(InventoryGrid.Instance.GetAllItems());
        foreach (var item in items)
            InventoryGrid.Instance.RemoveItem(item);

        spawned = false;
        Debug.Log("[TestSpawner] Inventory cleared.");

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RefreshInventoryGrid();
            InventoryUIManager.Instance.RefreshCraftingList();
        }
    }
}