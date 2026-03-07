using UnityEngine;
using System.Collections.Generic;


/// Singleton that owns all recipes, tracks which are unlocked,
/// and executes crafting (consuming ingredients, producing result).
public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("All Recipes in the Game")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

    // Recipes the player has unlocked (either by finding all ingredients
    // once, or by picking up a recipe document).
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

    /// Unlocks a recipe by name (called from e.g. picking up a recipe scroll).
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


    /// Returns true if the player should be able to see this recipe in the UI.
    /// Visibility rules:
    ///   - Not locked by default  → always visible
    ///   - Locked by default      → visible only once unlocked OR once the
    ///                              player has had every ingredient at least once
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

    ///Returns every recipe the player can currently see.
    public List<CraftingRecipe> GetVisibleRecipes()
    {
        var visible = new List<CraftingRecipe>();
        foreach (var r in allRecipes)
            if (IsRecipeVisible(r))
                visible.Add(r);
        return visible;
    }

    /// How many of itemName the player currently has in inventory.
    public int CountInInventory(string itemName)
    {
        int count = 0;
        foreach (var item in InventoryGrid.Instance.GetAllItems())
            if (item.GetItemName() == itemName)
                count++;
        return count;
    }

    ///True if every ingredient requirement is currently met.
    public bool HasAllIngredients(CraftingRecipe recipe)
    {
        foreach (var ing in recipe.ingredients)
            if (CountInInventory(ing.itemName) < ing.amount)
                return false;
        return true;
    }

    public List<bool> GetIngredientStatus(CraftingRecipe recipe)
    {
        var status = new List<bool>();
        foreach (var ing in recipe.ingredients)
            status.Add(CountInInventory(ing.itemName) >= ing.amount);
        return status;
    }

    public bool TryCraft(CraftingRecipe recipe)
    {
        if (!HasAllIngredients(recipe))
        {
            Debug.Log($"[Crafting] Cannot craft {recipe.resultItemName} — missing ingredients.");
            return false;
        }

        // Consume ingredients
        foreach (var ing in recipe.ingredients)
        {
            int toRemove = ing.amount;
            var items = new List<Item>(InventoryGrid.Instance.GetAllItems());
            foreach (var item in items)
            {
                if (toRemove <= 0) break;
                if (item.GetItemName() == ing.itemName)
                {
                    InventoryGrid.Instance.RemoveItem(item);
                    toRemove--;
                }
            }
        }

        // Produce result
        if (recipe.resultItemPrefab == null)
        {
            Debug.LogWarning($"[Crafting] Recipe '{recipe.resultItemName}' has no result prefab assigned!");
            return false;
        }

        // Instantiate result (inactive so it doesn't appear in the world)
        Item result = Instantiate(recipe.resultItemPrefab);
        result.gameObject.SetActive(false);

        bool addedToInventory = InventoryGrid.Instance.TryAddItem(result);
        if (!addedToInventory)
        {
            // Drop at player position if inventory is full
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                result.transform.position = player.transform.position;
                result.gameObject.SetActive(true);
                Debug.Log($"[Crafting] Inventory full — {recipe.resultItemName} dropped at player position.");
            }
            else
            {
                // Last resort: just enable near origin
                result.transform.position = Vector3.zero;
                result.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log($"[Crafting] Crafted: {recipe.resultItemName}");
        }

        return true;
    }
}