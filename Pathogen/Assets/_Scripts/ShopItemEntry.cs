using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemEntry : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Row Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI noSpaceText;    
    [SerializeField] private Image progressBar;   
    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 1f;
    [Header("Colors")]
    [SerializeField] private Color colNormal = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    [SerializeField] private Color colHover = new Color(0.28f, 0.28f, 0.28f, 0.9f);
    [SerializeField] private Color colLocked = new Color(0.12f, 0.12f, 0.12f, 0.9f);
    [SerializeField] private Color colPriceOk = new Color(0.85f, 0.80f, 0.25f, 1f);  // gold
    [SerializeField] private Color colPriceBad = new Color(0.90f, 0.20f, 0.20f, 1f);  // red
    [SerializeField] private Color colProgressBar = new Color(0.25f, 0.85f, 0.35f, 1f);  // green

    private ShopItemData data;
    private bool isLocked = false;   // required item not owned
    private bool canAfford = false;
    private bool hasSpace = false;
    private bool isHolding = false;
    private float holdTimer = 0f;

    public void Configure(ShopItemData shopData)
    {
        data = shopData;
        // Resolve display name and icon from prefab if not overridden
        string resolvedName = shopData.displayName;
        Sprite resolvedIcon = shopData.displayIcon;
        if (shopData.itemPrefab != null)
        {
            Item itemComp = shopData.itemPrefab.GetComponent<Item>();
            if (itemComp != null)
            {
                if (string.IsNullOrEmpty(resolvedName)) resolvedName = itemComp.GetItemName();
                if (resolvedIcon == null) resolvedIcon = itemComp.GetIcon();
            }
        }
        // Unlock check
        isLocked = !string.IsNullOrEmpty(shopData.requiredItemName) && !InventoryGrid.Instance.HasItem(shopData.requiredItemName);
        if (isLocked)
        {
            // Show locked state — hide contents, show requirement
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            if (nameText != null) nameText.text = $"??? (Requires: {shopData.requiredItemName})";
            if (descText != null) descText.text = "";
            if (priceText != null) priceText.text = "";
            if (backgroundImage != null) backgroundImage.color = colLocked;
            if (noSpaceText != null) noSpaceText.gameObject.SetActive(false);
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            return;
        }
        // Unlocked — populate visuals
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(resolvedIcon != null);
            if (resolvedIcon != null) iconImage.sprite = resolvedIcon;
        }
        if (nameText != null) nameText.text = resolvedName;
        if (descText != null) descText.text = shopData.description;
        if (noSpaceText != null) noSpaceText.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (backgroundImage != null) backgroundImage.color = colNormal;

        RefreshAffordability();
    }

    public void RefreshAffordability()
    {
        if (isLocked || data == null) return;
        canAfford = WalletManager.Instance != null && WalletManager.Instance.Balance >= data.price;
        // Check if inventory has space for this item right now
        hasSpace = data.itemPrefab != null && InventoryGrid.Instance != null && InventoryGrid.Instance.HasSpaceForItem(data.itemPrefab.GetComponent<Item>());
        if (priceText != null)
        {
            priceText.text = $"{data.price} ◈";
            priceText.color = canAfford ? colPriceOk : colPriceBad;
        }
        // Show/hide No Space text proactively
        if (noSpaceText != null)
            noSpaceText.gameObject.SetActive(!hasSpace);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (isLocked || !canAfford || !hasSpace) return;
        isHolding = true;
        holdTimer = 0f;
        if (progressBar != null)
        {
            progressBar.color = colProgressBar;
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData) => CancelHold();
    public void OnPointerExit(PointerEventData eventData) => CancelHold();
    private void CancelHold()
    {
        isHolding = false;
        holdTimer = 0f;
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(false);
        }
        ResetBg();
    }

    void Update()
    {
        if (!isHolding) return;
        holdTimer += Time.deltaTime;
        if (progressBar != null)
            progressBar.fillAmount = Mathf.Clamp01(holdTimer / holdDuration);
        if (holdTimer >= holdDuration)
        {
            isHolding = false;
            TryPurchase();
        }
    }

    private void TryPurchase()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(false);
        }
        if (data?.itemPrefab == null) return;
        // Re-check affordability at time of purchase
        if (WalletManager.Instance == null || WalletManager.Instance.Balance < data.price)
        {
            if (priceText != null) priceText.color = colPriceBad;
            HUDFeedback.Instance?.ShowWarning("Not enough Patheos.");
            return;
        }
        // Space check — instantiate item and try adding to inventory
        GameObject instance = Instantiate(data.itemPrefab);
        Item itemComp = instance.GetComponent<Item>();
        if (itemComp == null)
        {
            Destroy(instance);
            HUDFeedback.Instance?.ShowWarning("Shop error — item has no Item component.");
            return;
        }
        bool added = InventoryGrid.Instance.TryAddItem(itemComp);
        if (!added)
        {
            Destroy(instance);
            // Show "No space" text on the entry
            if (noSpaceText != null) noSpaceText.gameObject.SetActive(true);
            HUDFeedback.Instance?.ShowWarning("No space in inventory!");
            return;
        }
        // Success — deduct currency
        WalletManager.Instance.Spend(data.price);
        instance.SetActive(false);  // item is in inventory grid, hide world GO
        HUDFeedback.Instance?.ShowInfo($"Bought {(nameText != null ? nameText.text : "item")} " + $"for {data.price} ◈");
        // Hide "no space" if it was showing
        if (noSpaceText != null) noSpaceText.gameObject.SetActive(false);
        // Refresh affordability after spending
        RefreshAffordability();
        // Refresh inventory UI if open
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        InventoryUIManager.Instance?.RefreshWalletDisplay();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isLocked && backgroundImage != null)
            backgroundImage.color = colHover;
    }

    private void ResetBg()
    {
        if (backgroundImage != null)
            backgroundImage.color = isLocked ? colLocked : colNormal;
    }
}