using UnityEngine;
using System.Collections.Generic;

public class TalismanManager : MonoBehaviour
{
    public static TalismanManager Instance { get; private set; }
    [Header("Settings")]
    [SerializeField] private int totalTalismans = 10;
    private int collectedCount = 0;
    // Reward thresholds — mirrors GDD values
    private static readonly (int threshold, string description)[] Rewards =
    {
        (1,  "+10% Stamina"),
        (5,  "+20% Reload Speed"),
        (10, "+30% Ranged Damage"),
    };

    public int CollectedCount => collectedCount;
    public int TotalCount => totalTalismans;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    ///Call this when the player smashes a talisman in the world
    public void CollectTalisman()
    {
        collectedCount++;
        collectedCount = Mathf.Min(collectedCount, totalTalismans);
        Debug.Log($"[Talisman] Collected {collectedCount}/{totalTalismans}");
        // Check if a reward threshold was just crossed
        foreach (var reward in Rewards)
        {
            if (collectedCount == reward.threshold)
            {
                ApplyReward(reward.threshold);
                Debug.Log($"[Talisman] Reward unlocked: {reward.description}");
            }
        }
    }
    public bool IsRewardUnlocked(int threshold) => collectedCount >= threshold;
    public int GetTalismanCount() => collectedCount;
    ///Restore talisman count from save — re-applies all earned rewards
    public void LoadCount(int count)
    {
        collectedCount = 0;
        int target = Mathf.Min(count, totalTalismans);
        for (int i = 0; i < target; i++)
            CollectTalisman();
    }

    private void ApplyReward(int threshold)
    {
        switch (threshold)
        {
            case 1:
                PlayerController.LocalInstance?.AddStaminaBonus(0.10f);
                HUDFeedback.Instance?.ShowInfo("Talisman reward: +10% Max Stamina!");
                break;
            case 5:
                reloadSpeedBonusMultiplier *= 0.80f;
                ApplyBonusesToEquippedWeapon();
                HUDFeedback.Instance?.ShowInfo("Talisman reward: +20% Reload Speed!");
                break;
            case 10:
                rangedDamageBonusMultiplier *= 1.30f;
                ApplyBonusesToEquippedWeapon();
                HUDFeedback.Instance?.ShowInfo("Talisman reward: +30% Ranged Damage!");
                break;
        }
    }

    // Persistent bonus multipliers — stored so they apply when new weapons are equipped
    private float reloadSpeedBonusMultiplier = 1.0f;
    private float rangedDamageBonusMultiplier = 1.0f;
    public float ReloadSpeedBonusMultiplier => reloadSpeedBonusMultiplier;
    public float RangedDamageBonusMultiplier => rangedDamageBonusMultiplier;
    private void ApplyBonusesToEquippedWeapon()
    {
        PlayerController player = PlayerController.LocalInstance;
        if (player?.EquippedWeapon == null) return;
        WeaponItem wi = player.EquippedWeapon.GetComponent<WeaponItem>();
        if (wi?.data == null) return;
        wi.data.reloadSpeedMultiplier = wi.data.reloadSpeedMultiplier * reloadSpeedBonusMultiplier;
        wi.data.rangedDamageMultiplier = wi.data.rangedDamageMultiplier * rangedDamageBonusMultiplier;
    }

    ///Called by WeaponItem.Equip to apply accumulated talisman bonuses
    public void ApplyBonusesToWeapon(WeaponItem wi)
    {
        if (wi?.data == null) return;
        wi.data.reloadSpeedMultiplier *= reloadSpeedBonusMultiplier;
        wi.data.rangedDamageMultiplier *= rangedDamageBonusMultiplier;
    }
}