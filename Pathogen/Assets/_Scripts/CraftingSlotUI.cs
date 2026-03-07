using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// One ingredient (or result) cell inside a recipe row.
/// Shows: coloured border box + icon + name + amount label.
public class CraftingSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image boxImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Colors")]
    [SerializeField] private Color hasSufficientColor = new Color(0.15f, 0.65f, 0.15f, 1f);  // green
    [SerializeField] private Color missingColor = new Color(0.70f, 0.15f, 0.15f, 1f);  // red
    [SerializeField] private Color neutralColor = new Color(0.30f, 0.55f, 0.80f, 1f);  // blue (result)

    ///Set up an ingredient slot.
    public void SetIngredient(Sprite icon, string itemName, int required, bool hasSufficient)
    {
        if (iconImage != null) { iconImage.sprite = icon; iconImage.enabled = (icon != null); }
        if (nameText != null) nameText.text = itemName;
        if (amountText != null) amountText.text = required > 1 ? $"x{required}" : "";
        if (boxImage != null) boxImage.color = hasSufficient ? hasSufficientColor : missingColor;
        if (nameText != null) nameText.color = hasSufficient ? hasSufficientColor : missingColor;
        if (amountText != null) amountText.color = hasSufficient ? hasSufficientColor : missingColor;
    }

    ///Set up a result slot (always neutral blue).
    public void SetResult(Sprite icon, string itemName, int amount)
    {
        if (iconImage != null) { iconImage.sprite = icon; iconImage.enabled = (icon != null); }
        if (nameText != null) nameText.text = itemName;
        if (amountText != null) amountText.text = amount > 1 ? $"x{amount}" : "";
        if (boxImage != null) boxImage.color = neutralColor;
        if (nameText != null) nameText.color = Color.white;
        if (amountText != null) amountText.color = Color.white;
    }

    ///Refresh just the colours (called while crafting list is open).
    public void RefreshStatus(bool hasSufficient, bool isResult = false)
    {
        if (isResult) return;
        Color c = hasSufficient ? hasSufficientColor : missingColor;
        if (boxImage != null) boxImage.color = c;
        if (nameText != null) nameText.color = c;
        if (amountText != null) amountText.color = c;
    }
}