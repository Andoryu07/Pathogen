using UnityEngine;
using System.Collections.Generic;

public class TalismanManager : MonoBehaviour
{
    public static TalismanManager Instance { get; private set; }
    [Header("Settings")]
    [SerializeField] private int totalTalismans = 10;
    private int collectedCount = 0;
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

    public void CollectTalisman()
    {
        collectedCount++;
        collectedCount = Mathf.Min(collectedCount, totalTalismans);
        Debug.Log($"[Talisman] Collected {collectedCount}/{totalTalismans}");
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
    private void ApplyReward(int threshold)
    {
        switch (threshold)
        {
            case 1:
                // +10% stamina — e.g. PlayerController.Instance.AddStaminaBonus(0.10f);
                Debug.Log("[Talisman] Applied: +10% Stamina");
                break;
            case 5:
                // +20% reload speed
                Debug.Log("[Talisman] Applied: +20% Reload Speed");
                break;
            case 10:
                // +30% ranged damage
                Debug.Log("[Talisman] Applied: +30% Ranged Damage");
                break;
        }
    }
}