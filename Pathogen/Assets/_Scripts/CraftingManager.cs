using UnityEngine;
using System.Collections.Generic;

/// Singleton that owns all recipes, tracks which are unlocked, and executes crafting (consuming ingredients, producing result).
public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }
    [Header("All Recipes in the Game")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    // Recipes the player has unlocked (either by finding all ingredients once, or by picking up a recipe document)
    private HashSet<CraftingRecipe> unlockedRecipes = new HashSet<CraftingRecipe>();

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

    /// Unlocks a recipe by name (called from e.g. picking up a recipe scroll)
    public void UnlockRecipe(string recipeName)
    {
        var recipe = allRecipes.Find(r => r.name == recipeName);
        if (recipe != null)
        {
            unlockedRecipes.Add(recipe);
            Debug.Log($"[Crafting] Recipe unlocked: {recipe.resultItemName}");
        }
    }

    public void UnlockRecipe(CraftingRecipe recipe)
    {
        if (recipe != null)
            unlockedRecipes.Add(recipe);
    }

    /// Returns true if the player should be able to see this recipe in the UI
    public bool IsRecipeVisible(CraftingRecipe recipe)
    {
        if (!recipe.lockedByDefault) return true;
        if (unlockedRecipes.Contains(recipe)) return true;

        // Auto-unlock: player currently has ALL ingredients in sufficient amounts
        if (HasAllIngredients(recipe))
        {
            unlockedRecipes.Add(recipe);
            return true;
        }
        return false;
    }

    ///Returns every recipe the player can currently see
    public List<CraftingRecipe> GetVisibleRecipes()
    {
        var visible = new List<CraftingRecipe>();
        foreach (var r in allRecipes)
            if (IsRecipeVisible(r))
                visible.Add(r);
        return visible;
    }

    /// Total units of itemName across all stacks in the inventory
    public int CountInInventory(string itemName)
    {
        return InventoryGrid.Instance.CountItem(itemName);
    }

    ///True if every ingredient requirement is currently met
    public List<string> GetUnlockedRecipeNames()
    {
        var list = new List<string>();
        foreach (var r in unlockedRecipes)
            if (r != null) list.Add(r.resultItemName);
        return list;
    }

    public void LoadUnlockedRecipes(List<string> names)
    {
        if (names == null) return;
        foreach (var name in names)
            UnlockRecipe(name);
    }

    public bool HasAllIngredients(CraftingRecipe recipe)
    {
        foreach (var ing in recipe.ingredients)
            if (CountInInventory(ing.itemName) < ing.amount)
                return false;
        return true;
    }

    /// Per-ingredient status: true = player has enough, false = not enough
    public List<bool> GetIngredientStatus(CraftingRecipe recipe)
    {
        var status = new List<bool>();
        foreach (var ing in recipe.ingredients)
            status.Add(CountInInventory(ing.itemName) >= ing.amount);
        return status;
    }

    /// Attempts to craft the recipe. Returns true on success
    /// Consumes ingredients and adds the result to inventory (or drops it)
    public bool TryCraft(CraftingRecipe recipe)
    {
        if (!HasAllIngredients(recipe))
        {
            Debug.Log($"[Crafting] Cannot craft {recipe.resultItemName} — missing ingredients.");
            return false;
        }

        // Consume ingredients — drain from stacks, remove empty stacks
        // Consume ingredients — one unit at a time, stack-aware
        foreach (var ing in recipe.ingredients)
        {
            int toRemove = ing.amount;
            while (toRemove > 0)
            {
                var snapshot = new List<Item>(InventoryGrid.Instance.GetAllItems());
                Item found = snapshot.Find(i => i.GetItemName() == ing.itemName);
                if (found == null) break;
                InventoryGrid.Instance.ConsumeOneFromStack(found);
                toRemove--;
            }
        }
        // Produce result
        if (recipe.resultItemPrefab == null)
        {
            Debug.LogWarning($"[Crafting] Recipe '{recipe.resultItemName}' has no result prefab assigned!");
            return false;
        }
        // Spawn result and add to inventory (stacking handled inside TryAddItem)
        for (int i = 0; i < recipe.resultAmount; i++)
        {
            Item result = Instantiate(recipe.resultItemPrefab);
            result.gameObject.SetActive(false);
            bool added = InventoryGrid.Instance.TryAddItem(result);
            if (!added)
            {
                // Inventory full — drop at player position
                PlayerController player = FindObjectOfType<PlayerController>();
                result.transform.position = player != null ? player.transform.position : Vector3.zero;
                result.gameObject.SetActive(true);
                Debug.Log($"[Crafting] Inventory full — {recipe.resultItemName} dropped.");
            }
        }
        Debug.Log($"[Crafting] Crafted: {recipe.resultItemName} x{recipe.resultAmount}");
        return true;
    }
}