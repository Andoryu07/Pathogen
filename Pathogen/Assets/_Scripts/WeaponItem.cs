using UnityEngine;

/// Holds the weapon's WeaponData reference and tracks the current magazine count at runtime
public class WeaponItem : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] public WeaponData data;
    private int currentMag = -1;   // -1 = uninitialised
    /// Returns the current rounds in the magazine
    /// Auto-fills to max on first access (simulates weapon spawning loaded)
    public int CurrentMag
    {
        get
        {
            if (currentMag < 0) currentMag = MagSize;
            return currentMag;
        }
    }
    public int MagSize => data != null ? data.magSize : 0;
    public string AmmoItemName => data != null ? data.ammoItemName : "";
    public bool IsMelee => MagSize == 0;
    ///Consume one round from the magazine. Returns false if empty
    public bool ConsumeRound()
    {
        if (IsMelee) return false;
        if (CurrentMag <= 0) return false;
        currentMag--;
        return true;
    }
    /// Top-up reload: pulls only the rounds needed to fill the mag from inventory
    /// Returns rounds loaded. Actual reload uses reloadSpeedMultiplier for timing
    public int Reload()
    {
        if (IsMelee) return 0;
        int needed = MagSize - CurrentMag;
        if (needed <= 0) return 0;
        int loaded = 0;
        for (int i = 0; i < needed; i++)
        {
            Item ammoItem = InventoryGrid.Instance.GetItem(AmmoItemName);
            if (ammoItem == null) break;
            InventoryGrid.Instance.RemoveItem(ammoItem);
            loaded++;
        }
        currentMag += loaded;
        return loaded;
    }

    /// Base reload duration in seconds — affected by reloadSpeedMultiplier
    /// Lower multiplier = faster reload. e.g. 0.5 = twice as fast
    public float GetReloadDuration()
    {
        float baseReload = data != null ? data.baseReloadTime : 2f;
        float multiplier = data != null ? data.reloadSpeedMultiplier : 1f;
        // Clamp multiplier so it can't go below 0.1 (avoid instant/zero reload)
        return baseReload * Mathf.Max(0.1f, multiplier);
    }

    /// Apply all current upgrades from UpgradeManager to this weapon instance
    public void ApplyUpgrades(WeaponUpgradeData upgradeData)
    {
        UpgradeManager.Instance?.ApplyUpgradesFromData(this, upgradeData);
    }
    /// Total ammo available in inventory for this weapon (excludes current mag)
    public int AmmoInInventory()
    {
        if (IsMelee) return 0;
        int total = 0;
        foreach (Item item in InventoryGrid.Instance.GetAllItems())
        {
            if (item.GetItemName() == AmmoItemName)
                total += InventoryGrid.Instance.GetStackCount(item);
        }
        return total;
    }
}