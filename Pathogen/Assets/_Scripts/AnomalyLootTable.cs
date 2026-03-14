using UnityEngine;

/// Reusable loot drop component attached to all anomaly prefabs.
public class AnomalyLootTable : MonoBehaviour
{
    [Header("Patheos (always drops)")]
    [SerializeField] private GameObject patheosPrefab;
    [SerializeField] private int patheosMin = 500;
    [SerializeField] private int patheosMax = 3000;
    [Header("Guaranteed Rare Drop (pick one per anomaly type)")]
    [SerializeField] private GameObject[] rareDropPrefabs;   // one is chosen at random
    [Header("Bonus Ammo Drop (optional)")]
    [SerializeField] private GameObject ammoDropPrefab;
    [SerializeField]
    [Range(0f, 1f)]
    private float ammoDropChance = 0.60f;
    [SerializeField] private int ammoDropMin = 5;
    [SerializeField] private int ammoDropMax = 15;

    public void DropAll(Vector2 position)
    {
        Vector2 centre = position;
        // Patheos
        if (patheosPrefab != null)
        {
            int amount = Random.Range(patheosMin, patheosMax + 1);
            var go = Instantiate(patheosPrefab,
                                 centre + Random.insideUnitCircle * 0.3f,
                                 Quaternion.identity);
            go.SetActive(true);
            go.GetComponent<PatheosCurrency>()?.SetAmount(amount);
        }
        // Guaranteed rare item
        if (rareDropPrefabs != null && rareDropPrefabs.Length > 0)
        {
            GameObject pick = rareDropPrefabs[Random.Range(0, rareDropPrefabs.Length)];
            if (pick != null)
            {
                var go = Instantiate(pick,
                             centre + Random.insideUnitCircle * 0.4f,
                             Quaternion.identity);
                go.SetActive(true);
            }
        }
        // Bonus ammo
        if (ammoDropPrefab != null && Random.value <= ammoDropChance)
        {
            int count = Random.Range(ammoDropMin, ammoDropMax + 1);
            var go = Instantiate(ammoDropPrefab,
                             centre + Random.insideUnitCircle * 0.35f,
                             Quaternion.identity);
            go.SetActive(true);
            go.GetComponent<Item>()?.SetWorldStackCount(count);
        }
    }
}