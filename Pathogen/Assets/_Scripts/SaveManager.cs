using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
/// Handles saving and loading game state to/from JSON files
/// Supports multiple save slots (default 3)
/// Save files stored at: Application.persistentDataPath/saves/slot_X.json
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private float playtimeAccumulator = 0f;

    //private int pendingLoadSlot = -1;   // -1 = no pending load
    private string SaveDir => Path.Combine(Application.persistentDataPath, "saves");
    private string SlotPath(int slot) => Path.Combine(SaveDir, $"slot_{slot}.json");
    private const int NewGameSlot = 3;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        Directory.CreateDirectory(SaveDir);
    }
    public void SaveNewGameBaseline()
    {
        // Wait one frame so scene has fully initialized
        StartCoroutine(SaveBaselineNextFrame());
    }
    private System.Collections.IEnumerator SaveBaselineNextFrame()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.5f);
        // Reset all enemies to full HP for the baseline
        foreach (var pe in FindObjectsOfType<PersistentEnemy>())
            pe.ResetToFullHealth();

        Save(NewGameSlot, "NewGameBaseline");
        Debug.Log("[SaveManager] New game baseline saved.");
    }
    private IEnumerator ApplyEnemyStatesNextFrame(SaveData data)
    {
        yield return null;
        foreach (var saved in data.enemyStates)
        {
            foreach (var pe in FindObjectsOfType<PersistentEnemy>())
            {
                if (pe.SceneID != saved.sceneID) continue;
                pe.ApplyHP(saved.currentHP);
                pe.transform.position = new Vector3(saved.posX, saved.posY, 0f);
                Rigidbody2D rb = pe.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = new Vector2(saved.posX, saved.posY);
                break;
            }
        }
    }
    void Update()
    {
        playtimeAccumulator += Time.deltaTime;
    }
    //public void SetPendingLoad(int slot) => pendingLoadSlot = slot;
    public int MaxSlots => maxSlots;
    public float GetCurrentPlaytime() => playtimeAccumulator;
    public bool SlotExists(int slot)
        => File.Exists(SlotPath(slot));
    public SaveData ReadSlotMeta(int slot)
    {
        if (!SlotExists(slot)) return null;
        try
        {
            string json = File.ReadAllText(SlotPath(slot));
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch { return null; }
    }

    public void Save(int slot, string saveName = "")
    {
        SaveData data = CollectSaveData(slot, saveName);
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SlotPath(slot), json);
        HUDFeedback.Instance?.ShowInfo("Game saved.");
        Debug.Log("[Save] Saved to slot " + slot);
    }

    public void Load(int slot)
    {
        if (!SlotExists(slot))
        {
            Debug.LogWarning("[Save] No save in slot " + slot);
            return;
        }
        try
        {
            string json = File.ReadAllText(SlotPath(slot));
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            ApplySaveData(data);
            HUDFeedback.Instance?.ShowInfo("Game loaded.");
            Debug.Log("[Save] Loaded slot " + slot);
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Failed to load slot " + slot + ": " + e.Message);
        }
    }

    public void DeleteSlot(int slot)
    {
        if (SlotExists(slot)) File.Delete(SlotPath(slot));
    }

    private SaveData CollectSaveData(int slot, string saveName)
    {
        SaveData data = new SaveData();

        // Meta
        SaveData existing = ReadSlotMeta(slot);
        data.saveName = string.IsNullOrEmpty(saveName) ? "Save " + (slot + 1) : saveName;
        data.difficulty = DifficultyManager.Instance != null
            ? (int)DifficultyManager.Instance.Current.difficulty : 1;
        data.sceneName = SceneManager.GetActiveScene().name;
        data.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        data.totalPlaytime = (existing?.totalPlaytime ?? 0f) + playtimeAccumulator;
        data.saveCount = (existing?.saveCount ?? 0) + 1;
        playtimeAccumulator = 0f;
        data.deathCount = RunStatsManager.Instance?.GetDeathCount() ?? 0;
        // Player
        PlayerController player = PlayerController.LocalInstance;
        if (player != null)
        {
            data.playerHP = player.CurrentHealth;
            data.playerMaxHP = player.MaxHealth;
            data.playerStamina = player.CurrentStamina;
            data.playerMaxStamina = player.MaxStamina;
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.equippedWeaponName = "";
            if (player?.EquippedWeapon != null)
                data.equippedWeaponName = player.EquippedWeapon.GetItemName();
        }
        // Infection
        if (InfectionManager.Instance != null)
        {
            data.infectionStage = InfectionManager.Instance.InfectionStage;
            data.infectionHits = InfectionManager.Instance.InfectionHits;
        }
        // Inventory
        data.inventoryItems = CollectInventoryItems();
        // Storebox
        data.storeboxItems = CollectStoreboxItems();
        // Special items
        if (SpecialItemManager.Instance != null)
        {
            data.hasLighter = SpecialItemManager.Instance.HasLighter;
            data.hasHazardMask = SpecialItemManager.Instance.HasHazardMask;
            data.hipPouchCount = SpecialItemManager.Instance.HipPouchCount;
        }
        data.gridWidth = InventoryGrid.Instance != null
            ? InventoryGrid.Instance.GridWidth : 7;
        // Wallet
        data.patheosBalance = WalletManager.Instance?.Balance ?? 0;
        // Upgrades
        data.upgradeLevels = UpgradeManager.Instance?.GetAllLevels()
            ?? new List<SavedUpgrade>();
        // Quests
        data.questStates = QuestManager.Instance?.GetAllStates()
            ?? new List<SavedQuest>();
        // Recipes
        data.unlockedRecipeNames = CraftingManager.Instance?.GetUnlockedRecipeNames()
            ?? new List<string>();
        // Documents
        if (ReadableManager.Instance != null)
            foreach (var doc in ReadableManager.Instance.GetAllReadables())
                data.collectedDocumentNames.Add(doc.GetItemName());
        // Tutorials
        if (TutorialManager.Instance != null)
            foreach (var t in TutorialManager.Instance.GetAllTutorials())
                data.tutorials.Add(new SavedTutorial { title = t.title, body = t.body });
        // Talismans
        data.talismanCount = TalismanManager.Instance?.CollectedCount ?? 0;
        // World state
        if (WorldPersistenceManager.Instance != null)
        {
            data.deadEnemyIDs = WorldPersistenceManager.Instance.GetDeadEnemyIDs();
            data.collectedPickupIDs = WorldPersistenceManager.Instance.GetCollectedPickupIDs();
            data.unlockedLockIDs = WorldPersistenceManager.Instance.GetUnlockedLockIDs();
        }
        data.enemyStates.Clear();
        foreach (var pe in FindObjectsOfType<PersistentEnemy>())
        {
            if (string.IsNullOrEmpty(pe.SceneID)) continue;
            data.enemyStates.Add(new SavedEnemyState
            {
                sceneID = pe.SceneID,
                currentHP = pe.GetCurrentHP(),
                posX = pe.transform.position.x,
                posY = pe.transform.position.y
            });
        }
        data.searchSpotStates.Clear();
        foreach (var spot in FindObjectsOfType<SearchSpot>())
        {
            var state = spot.GetSaveState();
            if (state.isRevealed)  // only save spots that have been interacted with
                data.searchSpotStates.Add(state);
        }
        VolkovBoss volkov = FindObjectOfType<VolkovBoss>();
        data.volkovDefeated = volkov == null || volkov.gameObject == null;
        return data;
    }

    private List<SavedItem> CollectInventoryItems()
    {
        var list = new List<SavedItem>();
        if (InventoryGrid.Instance == null) return list;
        var seen = new HashSet<Item>();
        foreach (var item in InventoryGrid.Instance.GetAllItems())
        {
            if (seen.Contains(item)) continue;
            seen.Add(item);
            Vector2Int pos = InventoryGrid.Instance.GetItemPosition(item);
            list.Add(new SavedItem
            {
                itemName = item.GetItemName(),
                gridX = pos.x,
                gridY = pos.y,
                rotated = InventoryGrid.Instance.IsItemRotated(item),
                stackCount = InventoryGrid.Instance.GetStackCount(item)
            });
        }
        return list;
    }

    private List<SavedItem> CollectStoreboxItems()
    {
        var list = new List<SavedItem>();
        if (StoreboxManager.Instance == null) return list;
        var seen = new HashSet<Item>();
        foreach (var item in StoreboxManager.Instance.GetStoredItems())
        {
            if (seen.Contains(item)) continue;
            seen.Add(item);
            list.Add(new SavedItem
            {
                itemName = item.GetItemName(),
                stackCount = StoreboxManager.Instance.GetStackCount(item)
            });
        }
        return list;
    }

    private void ApplySaveData(SaveData data)
    {
        // Player position and stats
        PlayerController player = PlayerController.LocalInstance;
        if (player != null)
        {
            player.RestoreBaseStats();   // reset to base before applying saved values
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, 0f);
            player.SetHealth(data.playerHP, data.playerMaxHP);
            player.SetStamina(data.playerStamina, data.playerMaxStamina);
            player.SetMovementEnabled(true);
        }
        // Infection — load AFTER player stats so penalties apply to restored base
        InfectionManager.Instance?.LoadState(data.infectionStage, data.infectionHits);
        // Clear and restore inventory
        RestoreInventory(data.inventoryItems);
        PlayerController.LocalInstance?.EquipWeapon(null);
        WeaponHUD.Instance?.Refresh(null);

        if (!string.IsNullOrEmpty(data.equippedWeaponName))
        {
            Item weapon = InventoryGrid.Instance?.GetItem(data.equippedWeaponName);
            if (weapon != null)
            {
                PlayerController.LocalInstance?.EquipWeapon(weapon);
                WeaponHUD.Instance?.Refresh(weapon);
            }
        }
        // Clear and restore storebox
        RestoreStorebox(data.storeboxItems);
        // Special items
        SpecialItemManager.Instance?.LoadState(
            data.hasLighter, data.hasHazardMask, data.hipPouchCount);
        // Restore grid width expansions
        int currentWidth = InventoryGrid.Instance?.GridWidth ?? 7;
        for (int i = currentWidth; i < data.gridWidth; i++)
        {
            InventoryGrid.Instance?.ExpandGrid();
            InventoryUIManager.Instance?.RebuildGrid();
        }
        // Wallet
        WalletManager.Instance?.LoadBalance(data.patheosBalance);
        // Upgrades
        UpgradeManager.Instance?.LoadAllLevels(data.upgradeLevels);
        // Quests
        QuestManager.Instance?.LoadAllStates(data.questStates);
        // Recipes
        CraftingManager.Instance?.LoadUnlockedRecipes(data.unlockedRecipeNames);
        // Documents
        ReadableManager.Instance?.LoadDocuments(data.collectedDocumentNames);
        // Tutorials
        TutorialManager.Instance?.LoadTutorials(data.tutorials);
        // Talismans
        TalismanManager.Instance?.LoadCount(data.talismanCount);
        // World state
        WorldPersistenceManager.Instance?.LoadState(
            data.deadEnemyIDs,
            data.collectedPickupIDs,
            data.unlockedLockIDs);
        StartCoroutine(ApplyEnemyStatesNextFrame(data));
        if (data.volkovDefeated)
        {
            VolkovBoss volkov = FindObjectOfType<VolkovBoss>();
            if (volkov != null) volkov.gameObject.SetActive(false);
        }

        foreach (var savedState in data.searchSpotStates)
        {
            foreach (var spot in FindObjectsOfType<SearchSpot>())
            {
                // Match by spotID field — make sure each SearchSpot has a unique spotID set
                if (spot.GetSaveState().spotID == savedState.spotID)
                {
                    spot.LoadSaveState(savedState);
                    break;
                }
            }
        }
        // Difficulty
        DifficultyManager.Instance?.SetDifficulty((Difficulty)data.difficulty);
        // Playtime
        playtimeAccumulator = 0f;
        RunStatsManager.Instance?.LoadDeathCount(data.deathCount);
        // Refresh all UI
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        InventoryUIManager.Instance?.RefreshWalletDisplay();
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    private void RestoreInventory(List<SavedItem> savedItems)
    {
        if (InventoryGrid.Instance == null) return;
        // Clear current inventory
        foreach (var item in new List<Item>(InventoryGrid.Instance.GetAllItems()))
            InventoryGrid.Instance.RemoveItemStack(item);
        // We can't restore without item prefabs — ItemRegistry handles this
        ItemRegistry registry = ItemRegistry.Instance;
        if (registry == null)
        {
            Debug.LogWarning("[Save] ItemRegistry not found — inventory not restored.");
            return;
        }

        foreach (var saved in savedItems)
        {
            GameObject prefab = registry.GetPrefab(saved.itemName);
            if (prefab == null)
            {
                Debug.LogWarning("[Save] No prefab found for: " + saved.itemName);
                continue;
            }
            GameObject go = Instantiate(prefab);
            Item itemComp = go.GetComponent<Item>();
            if (itemComp == null) { Destroy(go); continue; }
            // Place at saved position with saved rotation
            if (InventoryGrid.Instance.CanPlaceItemAt(itemComp, saved.gridX, saved.gridY, saved.rotated))
            {
                InventoryGrid.Instance.PlaceItemAtPublic(itemComp, saved.gridX, saved.gridY,
                                                          saved.rotated, saved.stackCount);
                go.SetActive(false);
            }
            else
            {
                // Fallback — try to find any free spot
                bool placed = InventoryGrid.Instance.TryAddItem(itemComp);
                if (!placed) Destroy(go);
                else go.SetActive(false);
            }
        }
    }

    private void RestoreStorebox(List<SavedItem> savedItems)
    {
        if (StoreboxManager.Instance == null) return;
        StoreboxManager.Instance.ClearAll();
        ItemRegistry registry = ItemRegistry.Instance;
        if (registry == null) return;
        foreach (var saved in savedItems)
        {
            GameObject prefab = registry.GetPrefab(saved.itemName);
            if (prefab == null) continue;
            GameObject go = Instantiate(prefab);
            Item itemComp = go.GetComponent<Item>();
            if (itemComp == null) { Destroy(go); continue; }
            go.SetActive(false);
            StoreboxManager.Instance.LoadItem(itemComp, saved.stackCount);
        }
    }
}