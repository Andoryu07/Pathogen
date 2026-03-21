using UnityEngine;
using System.Collections.Generic;
/// Maps item names to their prefabs so SaveManager can restore inventory
[CreateAssetMenu(fileName = "ItemRegistry", menuName = "Pathogen/Item Registry")]
public class ItemRegistry : ScriptableObject
{
    public static ItemRegistry Instance { get; private set; }
    [Tooltip("Add every item prefab in the game here.")]
    public List<GameObject> allItemPrefabs = new List<GameObject>();
    private Dictionary<string, GameObject> lookup;

    public static void Register(ItemRegistry registry)
    {
        Instance = registry;
        Instance.BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, GameObject>();
        foreach (var prefab in allItemPrefabs)
        {
            if (prefab == null) continue;
            Item item = prefab.GetComponent<Item>();
            if (item == null) continue;
            string name = item.GetItemName();
            if (!lookup.ContainsKey(name))
                lookup[name] = prefab;
            else
                Debug.LogWarning("[ItemRegistry] Duplicate item name: " + name);
        }
        Debug.Log("[ItemRegistry] Built lookup with " + lookup.Count + " items.");
    }

    public GameObject GetPrefab(string itemName)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(itemName, out var prefab) ? prefab : null;
    }
}