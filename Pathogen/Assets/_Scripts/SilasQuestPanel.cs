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
    [Header("Auto Refresh")]
    [SerializeField] private float refreshInterval = 0.5f;
    private float refreshTimer = 0f;

    void Awake()
    {
        QuestManager.Instance?.RegisterQuests(quests);
    }

    void OnEnable()
    {
        QuestManager.Instance?.RegisterQuests(quests);
        Refresh();
        refreshTimer = 0f;
    }

    void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            RefreshEntries();
        }
    }

    ///Refreshes all existing entry states without rebuilding the list
    private void RefreshEntries()
    {
        foreach (var entry in spawnedEntries)
            if (entry != null) entry.Refresh();
    }

    public void Refresh()
    {
        foreach (Transform c in entryListParent) Destroy(c.gameObject);
        spawnedEntries.Clear();
        if (quests == null || entryPrefab == null) return;
        foreach (QuestData quest in quests)
        {
            if (quest == null) continue;
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