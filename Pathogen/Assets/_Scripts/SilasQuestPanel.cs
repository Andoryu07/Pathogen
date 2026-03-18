using UnityEngine;
using System.Collections.Generic;

/// Populates the quest panel with QuestEntry rows
public class SilasQuestPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entryListParent;
    [SerializeField] private GameObject entryPrefab;
    [Header("Quests")]
    [SerializeField] private QuestData[] quests;

    private List<QuestEntry> spawnedEntries = new List<QuestEntry>();

    void OnEnable()
    {
        // Register quests with QuestManager so it can route progress reports
        QuestManager.Instance?.RegisterQuests(quests);
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform c in entryListParent) Destroy(c.gameObject);
        spawnedEntries.Clear();
        if (quests == null || entryPrefab == null) return;
        foreach (QuestData quest in quests)
        {
            if (quest == null) continue;
            // Don't show already claimed quests
            if (QuestManager.Instance != null &&
                QuestManager.Instance.IsClaimed(quest)) continue;
            GameObject go = Instantiate(entryPrefab, entryListParent);
            QuestEntry entry = go.GetComponent<QuestEntry>();
            if (entry == null) continue;
            entry.Configure(quest, OnQuestClaimed);
            spawnedEntries.Add(entry);
        }
    }

    private void OnQuestClaimed()
    {
        // Rebuild list — claimed quest will be filtered out
        Refresh();
    }
}