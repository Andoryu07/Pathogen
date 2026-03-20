using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// One crafting recipe row
public class CraftingRecipeUI : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public const float ROW_HEIGHT = 110f;
    [Header("Ingredient Slots (wire in order, up to max ingredients)")]
    [SerializeField] private Image[] ingredientBoxImages;
    [SerializeField] private TextMeshProUGUI[] ingredientNameTexts;
    [SerializeField] private Image[] ingredientIconImages;
    [Header("Result Slot")]
    [SerializeField] private Image resultIconImage;
    [SerializeField] private TextMeshProUGUI resultNameText;
    [Header("Hold UI")]
    [SerializeField] private TextMeshProUGUI hintLabel;
    [SerializeField] private Image progressBar;
    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 1f;

    private static readonly Color ColHave = new Color(0.20f, 0.75f, 0.20f, 1f);
    private static readonly Color ColMissing = new Color(0.80f, 0.20f, 0.20f, 1f);
    private static readonly Color ColResult = new Color(0.25f, 0.55f, 0.90f, 1f);
    private static readonly Color BgCan = new Color(0.12f, 0.22f, 0.12f, 0.90f);
    private static readonly Color BgCant = new Color(0.22f, 0.12f, 0.12f, 0.90f);
    private CraftingRecipe recipe;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private bool canCraft = false;

    public void Setup(CraftingRecipe r)
    {
        recipe = r;
        // Populate icons and names from recipe data
        for (int i = 0; i < r.ingredients.Count; i++)
        {
            if (i >= ingredientBoxImages.Length) break;
            var ing = r.ingredients[i];
            if (ingredientIconImages != null && i < ingredientIconImages.Length
                && ingredientIconImages[i] != null)
            {
                ingredientIconImages[i].sprite = ing.icon;
                ingredientIconImages[i].enabled = ing.icon != null;
            }
            if (ingredientNameTexts != null && i < ingredientNameTexts.Length
                && ingredientNameTexts[i] != null)
                ingredientNameTexts[i].text = ing.itemName;
        }

        if (resultIconImage != null)
        {
            resultIconImage.sprite = r.resultIcon;
            resultIconImage.enabled = r.resultIcon != null;
        }
        if (resultNameText != null)
            resultNameText.text = r.resultItemName;

        if (progressBar != null) progressBar.gameObject.SetActive(false);
        RefreshColors();
    }

    public void RefreshColors()
    {
        if (recipe == null) return;
        List<bool> status = CraftingManager.Instance.GetIngredientStatus(recipe);
        canCraft = CraftingManager.Instance.HasAllIngredients(recipe);
        for (int i = 0; i < status.Count; i++)
        {
            Color c = status[i] ? ColHave : ColMissing;
            if (ingredientBoxImages != null && i < ingredientBoxImages.Length
                && ingredientBoxImages[i] != null)
                ingredientBoxImages[i].color = c;
            if (ingredientNameTexts != null && i < ingredientNameTexts.Length
                && ingredientNameTexts[i] != null)
                ingredientNameTexts[i].color = c;
        }
        var bg = GetComponent<Image>();
        if (bg != null) bg.color = canCraft ? BgCan : BgCant;
        if (hintLabel != null)
            hintLabel.color = canCraft
                ? Color.white
                : new Color(0.55f, 0.55f, 0.55f, 1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canCraft) return;
        isHolding = true;
        holdTimer = 0f;
        if (progressBar != null)
        {
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
            ExecuteCraft();
        }
    }

    private void ExecuteCraft()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(false);
        }
        bool success = CraftingManager.Instance.TryCraft(recipe);
        if (success)
        {
            InventoryUIManager.Instance.RefreshCraftingList();
            InventoryUIManager.Instance.RefreshInventoryGrid();
        }
    }
}