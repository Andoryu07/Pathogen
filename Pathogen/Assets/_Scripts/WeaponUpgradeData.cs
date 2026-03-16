using UnityEngine;
/// Defines all upgrade stages for a single weapon
[CreateAssetMenu(fileName = "WeaponUpgradeData", menuName = "Pathogen/Weapon Upgrade Data")]
public class WeaponUpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Pistol";
    public Sprite weaponIcon;
    public bool isMelee = false;
    [Header("Damage (all weapons)")]
    public float[] damageValues = { 25f, 30f, 36f, 43f, 52f, 62f };
    public int[] damagePrices = { 0, 150, 250, 400, 600, 900 };
    [Header("Magazine Size (ranged only)")]
    public int[] magValues = { 15, 18, 21, 24, 28, 32 };
    public int[] magPrices = { 0, 120, 200, 320, 480, 720 };
    [Header("Fire Rate (ranged only — lower = faster)")]
    public float[] fireRateValues = { 0.25f, 0.22f, 0.19f, 0.16f, 0.13f, 0.10f };
    public int[] fireRatePrices = { 0, 180, 300, 460, 680, 1000 };
    [Header("Reload Speed (ranged only — lower multiplier = faster)")]
    public float[] reloadMultipliers = { 1.0f, 0.85f, 0.72f, 0.60f, 0.50f, 0.40f };
    public int[] reloadPrices = { 0, 130, 220, 350, 520, 780 };
}