using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
/// A searchable spot in the world
public class SearchSpot : InteractableBase
{

    [Header("Spot Identity")]
    [SerializeField] private string spotName = "Container";
    [Header("Fixed Items (always in this spot)")]
    [SerializeField] private List<GameObject> fixedItemPrefabs = new List<GameObject>();
    [Header("Random Loot Table (optional)")]
    [SerializeField] private List<LootEntry> randomLoot = new List<LootEntry>();
    [Header("Persistence")]
    [SerializeField] private string spotID = "";
    [Header("UI References (shared SearchSpot UI panel)")]
    [SerializeField] private GameObject searchPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private TextMeshProUGUI scrollHintText;
    [SerializeField] private Button takeButton;
    [SerializeField] private Button closeButton;
    private List<Item> spawnedItems = new List<Item>();
    private int currentIndex = 0;
    private bool isRevealed = false;
    private bool isOpen = false;
    private Lock lockComponent;
    string interactKey = InputManager.Instance.GetKeyForAction("Interact").ToString();

    void Awake()
    {
        promptMessage = $"{interactKey} - Search " + spotName;
        lockComponent = GetComponent<Lock>();
    }

    void Start()
    {
        if (takeButton != null) takeButton.onClick.AddListener(TakeCurrentItem);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (searchPanel != null) searchPanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        if (Input.GetKeyDown(KeyCode.E)) { TakeCurrentItem(); return; }
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f) Navigate(-1);
        if (scroll < 0f) Navigate(1);
    }

    public override void Interact()
    {
        if (lockComponent != null && lockComponent.isLocked)
        {
            lockComponent.Interact();
            if (!lockComponent.isLocked)
            {
                promptMessage = $"{interactKey} - Search " + spotName;
            }
            else
            {
                return;
            }
        }
        if (!isRevealed)
        {
            RevealContents();
            return;
        }
        if (!isOpen) Open();
    }
    private void RevealContents()
    {
        isRevealed = true;
        spawnedItems.Clear();
        // Spawn fixed items
        foreach (var prefab in fixedItemPrefabs)
        {
            if (prefab == null) continue;
            GameObject go = Instantiate(prefab);
            Item item = go.GetComponent<Item>();
            if (item != null)
            {
                go.SetActive(false);
                spawnedItems.Add(item);
            }
            else Destroy(go);
        }
        // Roll random loot
        foreach (var entry in randomLoot)
        {
            if (entry.prefab == null) continue;
            if (Random.value > entry.chance) continue;
            int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
            for (int i = 0; i < amount; i++)
            {
                GameObject go = Instantiate(entry.prefab);
                Item item = go.GetComponent<Item>();
                if (item != null) { go.SetActive(false); spawnedItems.Add(item); }
                else Destroy(go);
            }
        }
        if (spawnedItems.Count == 0)
        {
            HUDFeedback.Instance?.ShowInfo(spotName + " — nothing inside.");
            CheckDeactivate();
            return;
        }

        Open();
    }

    private void Open()
    {
        if (spawnedItems.Count == 0) { CheckDeactivate(); return; }
        isOpen = true;
        currentIndex = Mathf.Clamp(currentIndex, 0, spawnedItems.Count - 1);
        if (searchPanel != null) searchPanel.SetActive(true);
        TimeScaleManager.Freeze(this);
        WeaponHUD.Instance?.Hide();
        RefreshPanel();
    }

    public void Close()
    {
        isOpen = false;
        if (searchPanel != null) searchPanel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        WeaponHUD.Instance?.Show();
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    private void Navigate(int dir)
    {
        if (spawnedItems.Count == 0) return;
        currentIndex = (currentIndex + dir + spawnedItems.Count) % spawnedItems.Count;
        RefreshPanel();
    }

    private void TakeCurrentItem()
    {
        if (spawnedItems.Count == 0) { Close(); return; }
        currentIndex = Mathf.Clamp(currentIndex, 0, spawnedItems.Count - 1);
        Item item = spawnedItems[currentIndex];
        // Check inventory space
        if (!InventoryGrid.Instance.HasSpaceForItem(item))
        {
            HUDFeedback.Instance?.ShowWarning("Not enough inventory space — reorganise first.");
            return;
        }
        // Add to inventory
        bool added = InventoryGrid.Instance.TryAddItem(item);
        if (added)
        {
            item.gameObject.SetActive(false);
            spawnedItems.RemoveAt(currentIndex);
            HUDFeedback.Instance?.ShowInfo("Picked up: " + item.GetItemName());
            WeaponHUD.Instance?.RefreshAmmoText();
            InventoryUIManager.Instance?.RefreshInventoryGrid();
            // Adjust index
            if (spawnedItems.Count == 0)
            {
                Close();
                CheckDeactivate();
                return;
            }
            currentIndex = Mathf.Clamp(currentIndex, 0, spawnedItems.Count - 1);
            RefreshPanel();
        }
    }

    private void RefreshPanel()
    {
        if (spawnedItems.Count == 0) return;
        Item current = spawnedItems[currentIndex];
        if (titleText != null) titleText.text = spotName;
        if (itemNameText != null) itemNameText.text = current.GetItemName();
        if (itemIconImage != null)
        {
            itemIconImage.sprite = current.GetIcon();
            itemIconImage.enabled = current.GetIcon() != null;
        }
        if (itemCountText != null)
            itemCountText.text = (currentIndex + 1) + " / " + spawnedItems.Count;
        if (scrollHintText != null)
            scrollHintText.gameObject.SetActive(spawnedItems.Count > 1);
        // Warn if can't take current item
        bool hasSpace = InventoryGrid.Instance.HasSpaceForItem(current);
        if (takeButton != null) takeButton.interactable = hasSpace;
        if (itemNameText != null)
            itemNameText.color = hasSpace ? Color.white : new Color(0.9f, 0.3f, 0.3f, 1f);
    }
    private void CheckDeactivate()
    {
        if (spawnedItems.Count == 0)
        {
            GetComponent<PersistentPickup>()?.RegisterCollected();
            promptMessage = spotName + " — empty";
            // Disable interactable collider so prompt no longer shows
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
    public SavedSearchSpot GetSaveState()
    {
        var state = new SavedSearchSpot { spotID = spotID, isRevealed = isRevealed };
        foreach (var item in spawnedItems)
            state.remainingItems.Add(item.GetItemName());
        return state;
    }

    public void LoadSaveState(SavedSearchSpot state)
    {
        if (state == null || !state.isRevealed) return;
        isRevealed = true;

        // Respawn only the remaining items
        spawnedItems.Clear();
        ItemRegistry registry = ItemRegistry.Instance;
        if (registry == null) return;
        foreach (var name in state.remainingItems)
        {
            GameObject prefab = registry.GetPrefab(name);
            if (prefab == null) continue;
            GameObject go = Instantiate(prefab);
            Item item = go.GetComponent<Item>();
            if (item != null) { go.SetActive(false); spawnedItems.Add(item); }
            else Destroy(go);
        }
        // If no items remain, deactivate
        if (spawnedItems.Count == 0) CheckDeactivate();
    }
}


[System.Serializable]
public class LootEntry
{
    public GameObject prefab;
    [Range(0f, 1f)]
    public float chance = 1f;
    public int minAmount = 1;
    public int maxAmount = 1;
}