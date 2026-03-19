using UnityEngine;
using System.Collections.Generic;

/// Manages the weapon upgrades panel — weapon list on the left, upgrade entries on the right.
public class SilasWeaponUpgrades : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponListParent;
    [SerializeField] private Transform upgradeListParent;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private GameObject upgradeEntryPrefab;
    [Header("Weapon Data")]
    [SerializeField] private WeaponUpgradeData[] weapons;

    private List<WeaponButtonEntry> weaponButtons = new List<WeaponButtonEntry>();
    private List<WeaponUpgradeEntry> upgradeEntries = new List<WeaponUpgradeEntry>();
    private WeaponUpgradeData selectedWeapon = null;

    void OnEnable()
    {
        BuildWeaponList();
        // Auto-select first weapon that the player owns
        if (weapons != null)
        {
            foreach (var w in weapons)
            {
                if (w != null && PlayerOwnsWeapon(w.weaponName))
                {
                    SelectWeapon(w);
                    return;
                }
            }
        }
        // Nothing owned yet — clear upgrade list
        ClearUpgradeList();
    }

    private void BuildWeaponList()
    {
        foreach (Transform c in weaponListParent) Destroy(c.gameObject);
        weaponButtons.Clear();
        if (weapons == null) return;
        foreach (WeaponUpgradeData data in weapons)
        {
            if (data == null) continue;
            // Only show weapons the player owns
            if (!PlayerOwnsWeapon(data.weaponName)) continue;
            GameObject go = Instantiate(weaponButtonPrefab, weaponListParent);
            WeaponButtonEntry btn = go.GetComponent<WeaponButtonEntry>();
            if (btn == null) continue;
            btn.Configure(data, SelectWeapon);
            weaponButtons.Add(btn);
        }
    }

    private void SelectWeapon(WeaponUpgradeData data)
    {
        selectedWeapon = data;
        // Update selection highlight
        foreach (var btn in weaponButtons)
            btn.SetSelected(false);
        // Find and highlight the matching button by weapon name
        foreach (var btn in weaponButtons)
            if (btn.GetWeaponName() == data.weaponName)
                btn.SetSelected(true);

        BuildUpgradeList(data);
    }

    private void BuildUpgradeList(WeaponUpgradeData data)
    {
        ClearUpgradeList();
        if (data == null || upgradeEntryPrefab == null) return;
        // Damage — always shown for all weapons
        SpawnEntry(data, "Damage", "Strength");

        if (!data.isMelee)
        {
            SpawnEntry(data, "MagSize", "Magazine Size");
            SpawnEntry(data, "FireRate", "Fire Rate");
            SpawnEntry(data, "Reload", "Reload Speed");
        }
    }

    private void SpawnEntry(WeaponUpgradeData data, string type, string label)
    {
        GameObject go = Instantiate(upgradeEntryPrefab, upgradeListParent);
        WeaponUpgradeEntry entry = go.GetComponent<WeaponUpgradeEntry>();
        if (entry == null) return;
        entry.Configure(data, type, label);
        upgradeEntries.Add(entry);
    }

    private void ClearUpgradeList()
    {
        foreach (Transform c in upgradeListParent) Destroy(c.gameObject);
        upgradeEntries.Clear();
    }

    private static bool PlayerOwnsWeapon(string weaponName)
    {
        // Check inventory for the weapon
        if (InventoryGrid.Instance.HasItem(weaponName)) return true;
        // Also check if it's currently equipped
        PlayerController player = PlayerController.LocalInstance;
        if (player?.EquippedWeapon != null &&
            player.EquippedWeapon.GetItemName() == weaponName) return true;

        return false;
    }
}