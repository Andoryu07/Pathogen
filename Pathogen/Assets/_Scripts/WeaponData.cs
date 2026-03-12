using UnityEngine;
/// ScriptableObject that holds all stats for a weapon
[CreateAssetMenu(fileName = "WeaponData", menuName = "Pathogen/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Weapon";

    [Header("Magazine")]
    [Tooltip("Maximum rounds in one magazine. Set to 0 for melee weapons.")]
    public int magSize = 15;

    [Header("Ammo")]
    [Tooltip("Must match Item.GetItemName() exactly for the ammo prefab.")]
    public string ammoItemName = "Pistol Rounds";
}