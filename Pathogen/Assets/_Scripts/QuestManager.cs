using UnityEngine;
using System.Collections.Generic;

public enum QuestState { Available, Active, Completed, Claimed }
/// Singleton tracking quest states and progress
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    [Header("All Quests — register here so tracking works from game start")]
    [SerializeField] private QuestData[] allQuests;
    // Progress tracked per quest by questName
    private Dictionary<string, QuestState> states = new Dictionary<string, QuestState>();
    private Dictionary<string, int> progress = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Register quests assigned directly in the Inspector
        RegisterQuests(allQuests);
    }

    public QuestState GetState(QuestData quest)
    {
        if (states.TryGetValue(quest.questName, out QuestState s)) return s;
        return QuestState.Available;
    }

    public int GetProgress(QuestData quest)
    {
        if (progress.TryGetValue(quest.questName, out int p)) return p;
        return 0;
    }

    public bool IsActive(QuestData quest) => GetState(quest) == QuestState.Active;
    public bool IsCompleted(QuestData quest) => GetState(quest) == QuestState.Completed;
    public bool IsClaimed(QuestData quest) => GetState(quest) == QuestState.Claimed;

    public void AcceptQuest(QuestData quest)
    {
        if (GetState(quest) != QuestState.Available) return;
        states[quest.questName] = QuestState.Active;
        // For ObtainItem quests, pre-count items already in inventory
        if (quest.questType == QuestType.ObtainItem)
        {
            int alreadyHave = CountItemInInventory(quest.targetName);
            progress[quest.questName] = Mathf.Min(alreadyHave, quest.requiredCount);
            Debug.Log("[Quests] Accepted: " + quest.questName + " — pre-counted " + alreadyHave + " existing items.");
            if (progress[quest.questName] >= quest.requiredCount)
            {
                states[quest.questName] = QuestState.Completed;
                HUDFeedback.Instance?.ShowInfo("Quest complete: " + quest.questName + "! Return to Silas.");
            }
        }
        else
        {
            progress[quest.questName] = 0;
            Debug.Log("[Quests] Accepted: " + quest.questName);
        }
    }

    private int CountItemInInventory(string itemName)
    {
        int total = 0;
        foreach (Item item in InventoryGrid.Instance.GetAllItems())
        {
            if (item.GetItemName() == itemName)
                total += InventoryGrid.Instance.GetStackCount(item);
        }
        return total;
    }

    public void ReportEnemyKill(string enemyTag)
    {
        ReportProgress(QuestType.DefeatEnemy, enemyTag);
    }

    public void ReportItemObtained(string itemName)
    {
        ReportProgress(QuestType.ObtainItem, itemName);
    }

    public void ReportItemDonated(string itemName)
    {
        ReportProgress(QuestType.DonateItem, itemName);
    }

    private void ReportProgress(QuestType type, string target)
    {
        foreach (var kvp in new Dictionary<string, QuestState>(states))
        {
            if (kvp.Value != QuestState.Active) continue;
            // Look up in registeredQuests first, fall back to allQuests field
            QuestData quest = GetQuestDataByName(kvp.Key);
            if (quest == null)
            {
                Debug.LogWarning("[QuestManager] Quest " + kvp.Key + " active but not in registeredQuests — check allQuests array on QuestManager.");
                continue;
            }
            if (quest.questType != type) continue;
            if (quest.targetName != target) continue;
            progress[quest.questName] = Mathf.Min(GetProgress(quest) + 1, quest.requiredCount);
            Debug.Log("[QuestManager] Progress: " + quest.questName + " " + progress[quest.questName] + "/" + quest.requiredCount);
            if (progress[quest.questName] >= quest.requiredCount)
            {
                states[quest.questName] = QuestState.Completed;
                HUDFeedback.Instance?.ShowInfo("Quest complete: " + quest.questName + "! Return to Silas.");
                Debug.Log("[Quests] Completed: " + quest.questName);
            }
        }
    }

    /// Attempts to claim rewards. Returns false if inventory is full for any item reward
    public bool ClaimRewards(QuestData quest)
    {
        if (GetState(quest) != QuestState.Completed) return false;
        // Check space for all item rewards first
        if (quest.itemRewards != null)
        {
            foreach (GameObject prefab in quest.itemRewards)
            {
                if (prefab == null) continue;
                Item itemComp = prefab.GetComponent<Item>();
                if (itemComp != null && !InventoryGrid.Instance.HasSpaceForItem(itemComp))
                {
                    HUDFeedback.Instance?.ShowWarning("No inventory space for quest rewards!");
                    return false;
                }
            }
        }
        // Give Patheos
        if (quest.patheosReward > 0)
            WalletManager.Instance?.Add(quest.patheosReward);
        // Give item rewards
        if (quest.itemRewards != null)
        {
            foreach (GameObject prefab in quest.itemRewards)
            {
                if (prefab == null) continue;
                GameObject instance = Instantiate(prefab);
                Item itemComp = instance.GetComponent<Item>();
                if (itemComp != null)
                {
                    InventoryGrid.Instance.TryAddItem(itemComp);
                    instance.SetActive(false);
                }
                else Destroy(instance);
            }
        }
        states[quest.questName] = QuestState.Claimed;
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        InventoryUIManager.Instance?.RefreshWalletDisplay();
        HUDFeedback.Instance?.ShowInfo($"Rewards claimed for: {quest.questName}!");
        Debug.Log($"[Quests] Claimed: {quest.questName}");
        return true;
    }

    private List<QuestData> registeredQuests = new List<QuestData>();

    /// Register all quests at game start. Called by SilasQuestPanel.Awake() so progress is tracked even when the panel is closed.
    /// Merges new quests without clearing previously registered ones
    public void RegisterQuests(QuestData[] quests)
    {
        if (quests == null) return;
        foreach (var q in quests)
        {
            if (q == null) continue;
            if (!registeredQuests.Exists(r => r != null && r.questName == q.questName))
                registeredQuests.Add(q);
        }
        Debug.Log($"[QuestManager] {registeredQuests.Count} quest(s) registered.");
    }

    public List<SavedQuest> GetAllStates()
    {
        var list = new List<SavedQuest>();
        foreach (var kvp in states)
        {
            int prog = progress.TryGetValue(kvp.Key, out int p) ? p : 0;
            list.Add(new SavedQuest
            {
                questName = kvp.Key,
                state = (int)kvp.Value,
                progress = prog
            });
        }
        return list;
    }

    public void LoadAllStates(List<SavedQuest> saved)
    {
        states.Clear();
        progress.Clear();
        if (saved == null) return;
        foreach (var s in saved)
        {
            states[s.questName] = (QuestState)s.state;
            progress[s.questName] = s.progress;
        }
    }

    private QuestData GetQuestDataByName(string name)
    {
        return registeredQuests.Find(q => q != null && q.questName == name);
    }
}