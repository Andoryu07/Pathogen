using UnityEngine;
using System.Collections.Generic;

public enum QuestState { Available, Active, Completed, Claimed }
/// Singleton tracking quest states and progress
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // Progress tracked per quest by questName
    private Dictionary<string, QuestState> states = new Dictionary<string, QuestState>();
    private Dictionary<string, int> progress = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
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
        progress[quest.questName] = 0;
        Debug.Log($"[Quests] Accepted: {quest.questName}");
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
        // Notify all registered quests — SilasQuestPanel re-registers on open
        // We broadcast to all active quests of matching type
        foreach (var kvp in new Dictionary<string, QuestState>(states))
        {
            if (kvp.Value != QuestState.Active) continue;
            QuestData quest = GetQuestDataByName(kvp.Key);
            if (quest == null || quest.questType != type) continue;
            if (quest.targetName != target) continue;

            progress[quest.questName] = Mathf.Min(
                GetProgress(quest) + 1, quest.requiredCount);

            if (progress[quest.questName] >= quest.requiredCount)
            {
                states[quest.questName] = QuestState.Completed;
                HUDFeedback.Instance?.ShowInfo($"Quest complete: {quest.questName}! Return to Silas.");
                Debug.Log($"[Quests] Completed: {quest.questName}");
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
    public void RegisterQuests(QuestData[] quests)
    {
        registeredQuests.Clear();
        if (quests != null)
            registeredQuests.AddRange(quests);
    }

    private QuestData GetQuestDataByName(string name)
    {
        return registeredQuests.Find(q => q != null && q.questName == name);
    }
}