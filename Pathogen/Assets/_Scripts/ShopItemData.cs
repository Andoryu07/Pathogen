using UnityEngine;
/// Defines one item entry in Silas's shop.
[CreateAssetMenu(fileName = "ShopItem", menuName = "Pathogen/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Item")]
    public GameObject itemPrefab;
    [Tooltip("Overrides the name shown. Leave empty to use the Item component's name.")]
    public string displayName = "";
    [Tooltip("Overrides the icon shown. Leave empty to use the Item component's icon.")]
    public Sprite displayIcon;
    [Header("Price")]
    public int price = 100;
    [Header("Unlock Requirement")]
    [Tooltip("Player must own this item name to see this entry. Leave empty = always visible.")]
    public string requiredItemName = "";
    [Header("Description")]
    [TextArea(1, 3)]
    public string description = "";
}