using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// One upgrade type row (e.g. "Damage", "Fire Rate") in the weapon upgrades panel
public class WeaponUpgradeEntry : MonoBehaviour
{

    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI currentValueText;
    [Header("Pip Bar (5 Images)")]
    [SerializeField] private Image[] pips = new Image[5];
    [Header("Upgrade Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [Header("Colors")]
    [SerializeField] private Color pipFilled = new Color(0.25f, 0.85f, 0.35f, 1f); // green
    [SerializeField] private Color pipEmpty = new Color(0.25f, 0.25f, 0.25f, 1f); // dark grey
    [SerializeField] private Color priceOk = new Color(0.85f, 0.80f, 0.25f, 1f); // gold
    [SerializeField] private Color priceBad = new Color(0.90f, 0.20f, 0.20f, 1f); // red
    [SerializeField] private Color bgNormal = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    [SerializeField] private Color bgMaxed = new Color(0.12f, 0.22f, 0.12f, 0.9f); // dark green

    private WeaponUpgradeData upgradeData;
    private string upgradeType;   // "Damage", "MagSize", "FireRate", "Reload"
    private string displayName;
    private int currentLevel;
    private const int MaxLevel = 5;
    public void Configure(WeaponUpgradeData data, string type, string label)
    {
        upgradeData = data;
        upgradeType = type;
        displayName = label;
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(TryUpgrade);
        Refresh();
    }
    public void Refresh()
    {
        if (upgradeData == null) return;
        currentLevel = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetLevel(upgradeData.weaponName, upgradeType)
            : 0;
        bool isMaxed = currentLevel >= MaxLevel;
        int nextCost = isMaxed ? 0 : GetPrice(currentLevel + 1);
        bool canAfford = !isMaxed && WalletManager.Instance != null &&
                         WalletManager.Instance.Balance >= nextCost;
        if (nameText != null) nameText.text = displayName;
        if (levelText != null)
            levelText.text = isMaxed ? "MAX" : $"Lv. {currentLevel}";
        if (priceText != null)
        {
            priceText.gameObject.SetActive(!isMaxed);
            if (!isMaxed)
            {
                priceText.text = $"{nextCost} ◈";
                priceText.color = canAfford ? priceOk : priceBad;
            }
        }
        if (currentValueText != null)
            currentValueText.text = GetCurrentValueText(currentLevel);
        for (int i = 0; i < pips.Length; i++)
        {
            if (pips[i] != null)
                pips[i].color = i < currentLevel ? pipFilled : pipEmpty;
        }
        if (upgradeButton != null)
        {
            bool canBuy = !isMaxed && canAfford;
            upgradeButton.interactable = canBuy;
            if (upgradeButtonText != null)
                upgradeButtonText.text = isMaxed ? "Maxed" : "Upgrade";
        }
        if (backgroundImage != null)
            backgroundImage.color = isMaxed ? bgMaxed : bgNormal;
    }

    private void TryUpgrade()
    {
        if (upgradeData == null || UpgradeManager.Instance == null) return;
        int nextLevel = currentLevel + 1;
        if (nextLevel > MaxLevel) return;
        int cost = GetPrice(nextLevel);
        if (!WalletManager.Instance.Spend(cost)) return;
        UpgradeManager.Instance.SetLevel(upgradeData.weaponName, upgradeType, nextLevel);
        // Apply to equipped weapon immediately if it matches
        PlayerController player = PlayerController.LocalInstance;
        if (player?.EquippedWeapon != null)
        {
            WeaponItem wi = player.EquippedWeapon.GetComponent<WeaponItem>();
            if (wi != null) wi.ApplyUpgrades(upgradeData);
        }
        HUDFeedback.Instance?.ShowInfo(
            $"{upgradeData.weaponName} {displayName} upgraded to level {nextLevel}!");

        InventoryUIManager.Instance?.RefreshWalletDisplay();
        Refresh();
    }
    private string GetCurrentValueText(int level)
    {
        return upgradeType switch
        {
            "Damage" => $"{upgradeData.damageValues[Mathf.Clamp(level, 0, 5)]:F0} dmg",
            "MagSize" => $"{upgradeData.magValues[Mathf.Clamp(level, 0, 5)]} rounds",
            "FireRate" => $"{upgradeData.fireRateValues[Mathf.Clamp(level, 0, 5)]:F2}s delay",
            "Reload" => $"{upgradeData.reloadMultipliers[Mathf.Clamp(level, 0, 5)]:F2} speed",
            _ => ""
        };
    }
    private int GetPrice(int level)
    {
        int[] prices = upgradeType switch
        {
            "Damage" => upgradeData.damagePrices,
            "MagSize" => upgradeData.magPrices,
            "FireRate" => upgradeData.fireRatePrices,
            "Reload" => upgradeData.reloadPrices,
            _ => new int[6]
        };
        return level < prices.Length ? prices[level] : 0;
    }


}