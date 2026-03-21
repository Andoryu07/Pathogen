using UnityEngine;
/// Singleton tracking special item ownership
public class SpecialItemManager : MonoBehaviour
{
    public static SpecialItemManager Instance { get; private set; }

    private bool hasLighter = true;   
    private bool hasHazardMask = false;
    private int hipPouchCount = 0;
    private const int MaxHipPouches = 3;
    public bool HasLighter => hasLighter;
    public bool HasHazardMask => hasHazardMask;
    public int HipPouchCount => hipPouchCount;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    ///Restore special item state from save data
    public void LoadState(bool lighter, bool hazardMask, int pouches)
    {
        hasLighter = lighter;
        hasHazardMask = hazardMask;
        hipPouchCount = pouches;
        Debug.Log("[SpecialItems] State loaded.");
    }

    public void PickUpHazardMask()
    {
        if (hasHazardMask) return;
        hasHazardMask = true;
        HUDFeedback.Instance?.ShowInfo("Hazard Mask obtained — press H to equip.");
        Debug.Log("[SpecialItems] Hazard Mask picked up.");
    }

    public void PickUpHipPouch()
    {
        if (hipPouchCount >= MaxHipPouches)
        {
            HUDFeedback.Instance?.ShowInfo("Inventory already at maximum capacity.");
            return;
        }
        hipPouchCount++;
        InventoryGrid.Instance?.ExpandGrid();
        InventoryUIManager.Instance?.RebuildGrid();
        HUDFeedback.Instance?.ShowInfo("Hip Pouch used — inventory expanded!");
        Debug.Log("[SpecialItems] Hip Pouch used. Expansions: " + hipPouchCount + "/" + MaxHipPouches);
    }
}