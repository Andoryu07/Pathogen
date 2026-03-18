using UnityEngine;

public enum QuestType
{
    DefeatEnemy,  // Kill a certain enemy type X times
    ObtainItem,   // Pick up X of a certain item
    DonateItem    // Give X of a certain item to Silas
}

/// Defines one quest offered by Silas.
[CreateAssetMenu(fileName = "QuestData", menuName = "Pathogen/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Identity")]
    public string questName = "Quest Name";
    [TextArea(1, 3)]
    public string description = "What the player needs to do.";

    [Header("Objective")]
    public QuestType questType = QuestType.DefeatEnemy;
    [Tooltip("Enemy tag (e.g. 'Enemy') or item name to target. Must match exactly.")]
    public string targetName = "";
    public int requiredCount = 3;

    [Header("Rewards")]
    public int patheosReward = 500;
    public GameObject[] itemRewards;
}