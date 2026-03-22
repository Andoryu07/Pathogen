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

    [Header("Combat")]
    [Tooltip("Maximum range in world units. Bullet does nothing beyond this distance.")]
    public float range = 15f;
    [Tooltip("Damage dealt per hit.")]
    public float damage = 25f;
    [Tooltip("Seconds between shots (0 = unlimited click speed).")]
    public float fireRate = 0.25f;
    [Tooltip("Reload speed multiplier. 1.0 = base speed, 0.5 = twice as fast.")]
    public float reloadSpeedMultiplier = 1.0f;
    [Tooltip("Ranged damage multiplier from talisman rewards. 1.0 = base damage.")]
    public float rangedDamageMultiplier = 1.0f;
}