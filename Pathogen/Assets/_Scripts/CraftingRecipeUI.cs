using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// One full recipe row. Builds its own child UI entirely from code so the
/// prefab only needs this script + Image on the root — nothing else.
///
/// PREFAB SETUP (minimal):
///   CraftingRecipeButton
///     - Image component (background, RaycastTarget ON)
///     - This script
///     (all children are created at runtime by Setup())
/// </summary>
public class CraftingRecipeUI : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    // Row height exposed so InventoryUIManager can match ForceFullWidth
    public const float ROW_HEIGHT = 110f;

    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 1f;

    // ---------------------------------------------------------------
    private CraftingRecipe recipe;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private bool canCraft = false;

    private Image progressBar;
    private TextMeshProUGUI hintLabel;

    // Per-ingredient: box image + name text, for colour refresh
    private List<Image> ingredientBoxes = new List<Image>();
    private List<TextMeshProUGUI> ingredientTexts = new List<TextMeshProUGUI>();
    private List<bool> lastIngredientState = new List<bool>();

    // ---------------------------------------------------------------
    //  Colours
    // ---------------------------------------------------------------
    private static readonly Color ColHave = new Color(0.20f, 0.75f, 0.20f, 1f);
    private static readonly Color ColMissing = new Color(0.80f, 0.20f, 0.20f, 1f);
    private static readonly Color ColResult = new Color(0.25f, 0.55f, 0.90f, 1f);
    private static readonly Color BgCan = new Color(0.12f, 0.22f, 0.12f, 0.90f);
    private static readonly Color BgCant = new Color(0.22f, 0.12f, 0.12f, 0.90f);

    // ---------------------------------------------------------------
    //  Public setup — called by InventoryUIManager after Instantiate
    // ---------------------------------------------------------------
    public void Setup(CraftingRecipe r)
    {
        recipe = r;
        BuildUI();
        RefreshColors();
    }

    // ---------------------------------------------------------------
    //  Build all child objects from code
    // ---------------------------------------------------------------
    private void BuildUI()
    {
        RectTransform root = GetComponent<RectTransform>();

        // --- Main vertical layout on root ---
        var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 0f;
        vlg.padding = new RectOffset(6, 6, 6, 4);

        // --- Slots row (HLG) ---
        GameObject rowGO = CreateChild("SlotsRow", gameObject);
        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false;
        hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        AddLayoutElement(rowGO, preferredHeight: 72f, flexibleWidth: 1f);

        // Build ingredient cells + separators
        List<bool> status = CraftingManager.Instance.GetIngredientStatus(recipe);
        canCraft = CraftingManager.Instance.HasAllIngredients(recipe);

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            if (i > 0) SpawnSeparatorInRow(rowGO, "+");
            var ing = recipe.ingredients[i];
            bool has = i < status.Count && status[i];
            SpawnIngredientCell(rowGO, ing, has);
        }

        SpawnSeparatorInRow(rowGO, "=");
        SpawnResultCell(rowGO);

        // --- Hint label ---
        GameObject hintGO = CreateChild("HintLabel", gameObject);
        hintLabel = hintGO.AddComponent<TextMeshProUGUI>();
        hintLabel.text = "Hold to craft";
        hintLabel.fontSize = 13f;
        hintLabel.alignment = TextAlignmentOptions.Center;
        hintLabel.color = Color.white;
        AddLayoutElement(hintGO, preferredHeight: 16f, flexibleWidth: 1f);

        // --- Progress bar (sits at very bottom of root via absolute positioning) ---
        GameObject barGO = CreateChild("ProgressBar", gameObject);
        var barRT = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0f);
        barRT.anchorMax = new Vector2(1f, 0f);
        barRT.pivot = new Vector2(0.5f, 0f);
        barRT.sizeDelta = new Vector2(0f, 5f);
        barRT.anchoredPosition = Vector2.zero;

        progressBar = barGO.AddComponent<Image>();
        progressBar.color = new Color(0.3f, 0.9f, 0.3f, 1f);
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        progressBar.fillAmount = 0f;
        barGO.SetActive(false);

        // LayoutElement ignored = true so it doesn't push VLG layout
        var barLE = barGO.AddComponent<LayoutElement>();
        barLE.ignoreLayout = true;
    }

    // ---------------------------------------------------------------
    //  Slot builders
    // ---------------------------------------------------------------
    private void SpawnIngredientCell(GameObject parent, CraftingRecipe.Ingredient ing, bool has)
    {
        // Cell root
        GameObject cell = CreateChild($"Ing_{ing.itemName}", parent);
        AddLayoutElement(cell, preferredWidth: 64f, preferredHeight: 72f);
        var cellVLG = cell.AddComponent<VerticalLayoutGroup>();
        cellVLG.childControlWidth = true;
        cellVLG.childForceExpandWidth = true;
        cellVLG.childControlHeight = false;
        cellVLG.childForceExpandHeight = false;
        cellVLG.spacing = 2f;
        cellVLG.padding = new RectOffset(0, 0, 0, 0);
        cellVLG.childAlignment = TextAnchor.UpperCenter;

        // Box (coloured square)
        GameObject boxGO = CreateChild("Box", cell);
        Image boxImg = boxGO.AddComponent<Image>();
        boxImg.color = has ? ColHave : ColMissing;
        AddLayoutElement(boxGO, preferredWidth: 48f, preferredHeight: 48f);

        // Icon inside box
        if (ing.icon != null)
        {
            GameObject iconGO = CreateChild("Icon", boxGO);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.sizeDelta = Vector2.zero;
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = ing.icon;
            iconImg.raycastTarget = false;
        }

        // Amount label (top-right of box)
        if (ing.amount > 1)
        {
            GameObject amtGO = CreateChild("Amount", boxGO);
            var amtRT = amtGO.GetComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(1f, 1f);
            amtRT.anchorMax = new Vector2(1f, 1f);
            amtRT.pivot = new Vector2(1f, 1f);
            amtRT.sizeDelta = new Vector2(28f, 18f);
            amtRT.anchoredPosition = Vector2.zero;
            var amtTMP = amtGO.AddComponent<TextMeshProUGUI>();
            amtTMP.text = $"x{ing.amount}";
            amtTMP.fontSize = 11f;
            amtTMP.alignment = TextAlignmentOptions.TopRight;
            amtTMP.color = Color.white;
            var amtLE = amtGO.AddComponent<LayoutElement>();
            amtLE.ignoreLayout = true;
        }

        // Name label below box
        GameObject nameGO = CreateChild("Name", cell);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = ing.itemName;
        nameTMP.fontSize = 11f;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = has ? ColHave : ColMissing;
        nameTMP.enableWordWrapping = false;
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;
        AddLayoutElement(nameGO, preferredHeight: 16f, flexibleWidth: 1f);

        // Store refs for colour refresh
        ingredientBoxes.Add(boxImg);
        ingredientTexts.Add(nameTMP);
        lastIngredientState.Add(has);
    }

    private void SpawnResultCell(GameObject parent)
    {
        GameObject cell = CreateChild("Result", parent);
        AddLayoutElement(cell, preferredWidth: 64f, preferredHeight: 72f);
        var cellVLG = cell.AddComponent<VerticalLayoutGroup>();
        cellVLG.childControlWidth = true;
        cellVLG.childForceExpandWidth = true;
        cellVLG.childControlHeight = false;
        cellVLG.childForceExpandHeight = false;
        cellVLG.spacing = 2f;
        cellVLG.childAlignment = TextAnchor.UpperCenter;

        // Box
        GameObject boxGO = CreateChild("Box", cell);
        Image boxImg = boxGO.AddComponent<Image>();
        boxImg.color = ColResult;
        AddLayoutElement(boxGO, preferredWidth: 48f, preferredHeight: 48f);

        // Icon
        if (recipe.resultIcon != null)
        {
            GameObject iconGO = CreateChild("Icon", boxGO);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.sizeDelta = Vector2.zero;
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = recipe.resultIcon;
            iconImg.raycastTarget = false;
        }

        // Amount
        if (recipe.resultAmount > 1)
        {
            GameObject amtGO = CreateChild("Amount", boxGO);
            var amtRT = amtGO.GetComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(1f, 1f);
            amtRT.anchorMax = new Vector2(1f, 1f);
            amtRT.pivot = new Vector2(1f, 1f);
            amtRT.sizeDelta = new Vector2(28f, 18f);
            amtRT.anchoredPosition = Vector2.zero;
            var amtTMP = amtGO.AddComponent<TextMeshProUGUI>();
            amtTMP.text = $"x{recipe.resultAmount}";
            amtTMP.fontSize = 11f;
            amtTMP.alignment = TextAlignmentOptions.TopRight;
            amtTMP.color = Color.white;
            var amtLE = amtGO.AddComponent<LayoutElement>();
            amtLE.ignoreLayout = true;
        }

        // Name
        GameObject nameGO = CreateChild("ResultName", cell);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = recipe.resultItemName;
        nameTMP.fontSize = 11f;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        nameTMP.enableWordWrapping = false;
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;
        AddLayoutElement(nameGO, preferredHeight: 16f, flexibleWidth: 1f);
    }

    private void SpawnSeparatorInRow(GameObject parent, string text)
    {
        GameObject g = CreateChild(text, parent);
        var tmp = g.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        AddLayoutElement(g, preferredWidth: 20f, preferredHeight: 72f);
    }

    // ---------------------------------------------------------------
    //  Colour refresh
    // ---------------------------------------------------------------
    public void RefreshColors()
    {
        if (recipe == null) return;
        List<bool> status = CraftingManager.Instance.GetIngredientStatus(recipe);
        canCraft = CraftingManager.Instance.HasAllIngredients(recipe);

        for (int i = 0; i < ingredientBoxes.Count && i < status.Count; i++)
        {
            Color c = status[i] ? ColHave : ColMissing;
            if (ingredientBoxes[i] != null) ingredientBoxes[i].color = c;
            if (ingredientTexts[i] != null) ingredientTexts[i].color = c;
        }

        var bg = GetComponent<Image>();
        if (bg != null) bg.color = canCraft ? BgCan : BgCant;
        if (hintLabel != null)
            hintLabel.color = canCraft ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
    }

    // ---------------------------------------------------------------
    //  Hold-to-craft
    // ---------------------------------------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canCraft) return;
        isHolding = true;
        holdTimer = 0f;
        if (progressBar != null) progressBar.gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData) => CancelHold();
    public void OnPointerExit(PointerEventData eventData) => CancelHold();

    private void CancelHold()
    {
        isHolding = false;
        holdTimer = 0f;
        if (progressBar != null) { progressBar.fillAmount = 0f; progressBar.gameObject.SetActive(false); }
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
        if (progressBar != null) { progressBar.fillAmount = 0f; progressBar.gameObject.SetActive(false); }
        bool success = CraftingManager.Instance.TryCraft(recipe);
        if (success)
        {
            InventoryUIManager.Instance.RefreshCraftingList();
            InventoryUIManager.Instance.RefreshInventoryGrid();
        }
    }

    // ---------------------------------------------------------------
    //  Helpers
    // ---------------------------------------------------------------
    private static GameObject CreateChild(string name, GameObject parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void AddLayoutElement(GameObject go,
        float preferredWidth = -1f,
        float preferredHeight = -1f,
        float flexibleWidth = -1f,
        float flexibleHeight = -1f)
    {
        var le = go.AddComponent<LayoutElement>();
        if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
        if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
        if (flexibleWidth >= 0) le.flexibleWidth = flexibleWidth;
        if (flexibleHeight >= 0) le.flexibleHeight = flexibleHeight;
    }
}