using UnityEngine;
using System.Collections.Generic;
/// Singleton that tracks purchased upgrade levels for all weapons.
/// Persists across scenes.
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    ///Returns the current upgrade level (0 = not upgraded)
    public int GetLevel(string weaponName, string upgradeType)
    {
        string key = Key(weaponName, upgradeType);
        return upgradeLevels.TryGetValue(key, out int lvl) ? lvl : 0;
    }

    ///Sets the upgrade level. Called after a successful purchase
    public void SetLevel(string weaponName, string upgradeType, int level)
    {
        upgradeLevels[Key(weaponName, upgradeType)] = level;

        // Re-apply upgrades to equipped weapon if it matches
        ApplyToEquippedWeapon(weaponName);
    }
    /// Applies all current upgrade levels to the equipped weapon if it matches weaponName
    public void ApplyToEquippedWeapon(string weaponName)
    {
        PlayerController player = PlayerController.LocalInstance;
        if (player?.EquippedWeapon == null) return;

        WeaponItem wi = player.EquippedWeapon.GetComponent<WeaponItem>();
        if (wi == null || wi.data == null || wi.data.weaponName != weaponName) return;

        ApplyUpgrades(wi);
    }

    ///Applies all stored upgrades to a WeaponItem directly
    public void ApplyUpgrades(WeaponItem wi)
    {
        if (wi == null || wi.data == null) return;
        string wn = wi.data.weaponName;

        // Find the matching WeaponUpgradeData — search all registered upgrade data
        // WeaponUpgradeData provides the value arrays; levels come from this manager
        // SilasWeaponUpgrades passes upgrade data via ApplyUpgradesFromData
    }
    /// Applies upgrades to a WeaponItem using provided WeaponUpgradeData
    public void ApplyUpgradesFromData(WeaponItem wi, WeaponUpgradeData data)
    {
        if (wi == null || wi.data == null || data == null) return;
        string wn = data.weaponName;
        // Damage
        int dmgLvl = GetLevel(wn, "Damage");
        if (dmgLvl > 0 && dmgLvl < data.damageValues.Length)
            wi.data.damage = data.damageValues[dmgLvl];
        else
            wi.data.damage = data.damageValues[0];

        if (!data.isMelee)
        {
            // Mag size
            int magLvl = GetLevel(wn, "MagSize");
            if (magLvl > 0 && magLvl < data.magValues.Length)
                wi.data.magSize = data.magValues[magLvl];
            else
                wi.data.magSize = data.magValues[0];
            // Fire rate
            int frLvl = GetLevel(wn, "FireRate");
            if (frLvl > 0 && frLvl < data.fireRateValues.Length)
                wi.data.fireRate = data.fireRateValues[frLvl];
            else
                wi.data.fireRate = data.fireRateValues[0];
            // Reload speed
            int rlLvl = GetLevel(wn, "Reload");
            if (rlLvl > 0 && rlLvl < data.reloadMultipliers.Length)
                wi.data.reloadSpeedMultiplier = data.reloadMultipliers[rlLvl];
            else
                wi.data.reloadSpeedMultiplier = data.reloadMultipliers[0];
        }
        // Refresh HUD ammo display since mag size may have changed
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    public List<SavedUpgrade> GetAllLevels()
    {
        var list = new List<SavedUpgrade>();
        foreach (var kvp in upgradeLevels)
            list.Add(new SavedUpgrade { key = kvp.Key, level = kvp.Value });
        return list;
    }
    public void LoadAllLevels(List<SavedUpgrade> saved)
    {
        upgradeLevels.Clear();
        if (saved == null) return;
        foreach (var s in saved)
            upgradeLevels[s.key] = s.level;
    }
    private static string Key(string weaponName, string upgradeType) => $"{weaponName}_{upgradeType}";
}