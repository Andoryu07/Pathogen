using UnityEngine;
using System.Collections.Generic;

/// Tracks world state — which enemies are dead and which pickups have been collected
/// On scene load, all PersistentEnemy and PersistentPickup objects check in here and deactivate themselves if their ID is in the dead/collected lists
public class WorldPersistenceManager : MonoBehaviour
{
    public static WorldPersistenceManager Instance { get; private set; }

    private HashSet<string> deadEnemies = new HashSet<string>();
    private HashSet<string> collectedPickups = new HashSet<string>();
    private HashSet<string> unlockedLocks = new HashSet<string>();

    [Header("Auto-Load")]
    [Tooltip("If true, automatically loads world state from the last saved slot on Awake.")]
    [SerializeField] private bool autoLoadOnAwake = true;
    [SerializeField] private int autoLoadSlot = 0;  // which slot to auto-load from

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Load world state immediately in Awake — before any enemy/pickup Start() runs
            if (autoLoadOnAwake)
                AutoLoad();
        }
        else Destroy(gameObject);
    }

    private void AutoLoad()
    {
        // Find the most recent slot that exists
        string savesDir = System.IO.Path.Combine(
            UnityEngine.Application.persistentDataPath, "saves");
        System.DateTime newest = System.DateTime.MinValue;
        string bestPath = null;
        for (int i = 0; i < 3; i++)
        {
            string path = System.IO.Path.Combine(savesDir, "slot_" + i + ".json");
            if (!System.IO.File.Exists(path)) continue;
            System.DateTime t = System.IO.File.GetLastWriteTime(path);
            if (t > newest) { newest = t; bestPath = path; }
        }
        if (bestPath == null) return;
        try
        {
            string json = System.IO.File.ReadAllText(bestPath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;
            // Only load world state lists — not player stats or inventory (those are applied when the player explicitly hits Load)
            if (data.deadEnemyIDs != null)
                foreach (var id in data.deadEnemyIDs) deadEnemies.Add(id);
            if (data.collectedPickupIDs != null)
                foreach (var id in data.collectedPickupIDs) collectedPickups.Add(id);
            if (data.unlockedLockIDs != null)
                foreach (var id in data.unlockedLockIDs) unlockedLocks.Add(id);
            Debug.Log("[WorldPersist] Auto-loaded world state — dead enemies:" +
                      deadEnemies.Count + " collected pickups:" + collectedPickups.Count);
            foreach (var id in deadEnemies)
                Debug.Log("[WorldPersist] Dead enemy ID in list: " + id);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[WorldPersist] Auto-load failed: " + e.Message);
        }
    }

    public void RegisterEnemyDeath(string id)
    {
        deadEnemies.Add(id);
        Debug.Log("[WorldPersist] Enemy dead: " + id);
    }

    public bool IsEnemyDead(string id) => deadEnemies.Contains(id);
    public void RegisterPickupCollected(string id)
    {
        collectedPickups.Add(id);
        Debug.Log("[WorldPersist] Pickup collected: " + id);
    }

    public bool IsPickupCollected(string id) => collectedPickups.Contains(id);
    public void RegisterLockUnlocked(string id)
    {
        unlockedLocks.Add(id);
        Debug.Log("[WorldPersist] Lock unlocked: " + id);
    }

    public bool IsLockUnlocked(string id) => unlockedLocks.Contains(id);

    public List<string> GetDeadEnemyIDs() => new List<string>(deadEnemies);
    public List<string> GetCollectedPickupIDs() => new List<string>(collectedPickups);
    public List<string> GetUnlockedLockIDs() => new List<string>(unlockedLocks);

    public void LoadState(List<string> enemies, List<string> pickups, List<string> locks)
    {
        deadEnemies.Clear();
        collectedPickups.Clear();
        unlockedLocks.Clear();
        if (enemies != null) foreach (var id in enemies) deadEnemies.Add(id);
        if (pickups != null) foreach (var id in pickups) collectedPickups.Add(id);
        if (locks != null) foreach (var id in locks) unlockedLocks.Add(id);

        Debug.Log("[WorldPersist] Loaded — enemies:" + deadEnemies.Count +
                  " pickups:" + collectedPickups.Count +
                  " locks:" + unlockedLocks.Count);

        // Apply state to all persistent objects currently in the scene (their Start() already ran before the save was loaded)
        ApplyToScene();
    }

    /// Deactivates any scene objects whose IDs are in the dead/collected lists
    /// Called after loading so objects that already ran Start() still get removed
    private void ApplyToScene()
    {
        foreach (var pe in FindObjectsOfType<PersistentEnemy>())
            if (!string.IsNullOrEmpty(pe.SceneID) && IsEnemyDead(pe.SceneID))
                pe.gameObject.SetActive(false);

        foreach (var pp in FindObjectsOfType<PersistentPickup>())
            if (!string.IsNullOrEmpty(pp.SceneID) && IsPickupCollected(pp.SceneID))
                pp.gameObject.SetActive(false);
    }
}