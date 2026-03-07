using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Pathogen/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public class Ingredient
    {
        public string itemName;     // must match Item.GetItemName() exactly
        public Sprite icon;         // drag the matching icon here
        public int amount = 1;
    }

    [Header("Ingredients")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    [Header("Result")]
    public string resultItemName;   // name of the item to add to inventory
    public Sprite resultIcon;
    public int resultAmount = 1;    // how many are produced (for stack display)

    [Header("Result Item Prefab")]
    [Tooltip("The Item prefab to instantiate and add to inventory when crafted.")]
    public Item resultItemPrefab;

    [Header("Unlock")]
    [Tooltip("If true, recipe is hidden until unlocked via CraftingManager.UnlockRecipe().")]
    public bool lockedByDefault = false;
}