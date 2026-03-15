using UnityEngine;
using System.Collections.Generic;
/// Populates the Item Shop panel with ShopItemEntry rows
public class SilasItemShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entryListParent;
    [SerializeField] private GameObject entryPrefab;
    [Header("Shop Stock")]
    [SerializeField] private ShopItemData[] shopItems;

    private List<ShopItemEntry> spawnedEntries = new List<ShopItemEntry>();

    void OnEnable()
    {
        // Refresh every time the panel becomes visible
        Refresh();
    }
    public void Refresh()
    {
        if (entryListParent == null || entryPrefab == null) return;
        // Clear existing entries
        foreach (Transform child in entryListParent)
            Destroy(child.gameObject);
        spawnedEntries.Clear();
        if (shopItems == null) return;
        foreach (ShopItemData data in shopItems)
        {
            if (data == null) continue;

            GameObject go = Instantiate(entryPrefab, entryListParent);
            ShopItemEntry entry = go.GetComponent<ShopItemEntry>();
            if (entry == null) continue;

            entry.Configure(data);
            spawnedEntries.Add(entry);
        }
    }

    ///Refreshes all affordability states without rebuilding the list
    public void RefreshAffordability()
    {
        foreach (var entry in spawnedEntries)
            entry.RefreshAffordability();
    }
}