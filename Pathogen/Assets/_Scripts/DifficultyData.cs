using UnityEngine;

public enum Difficulty { Casual, Normal, Hardcore }
/// ScriptableObject defining enemy stat multipliers for a difficulty level
[CreateAssetMenu(fileName = "Difficulty_Normal", menuName = "Pathogen/Difficulty Data")]
public class DifficultyData : ScriptableObject
{
    [Header("Identity")]
    public Difficulty difficulty = Difficulty.Normal;
    public string displayName = "Normal";
    [TextArea(1, 3)]
    public string description = "The intended experience.";

    [Header("Enemy Multipliers")]
    [Tooltip("Multiplier applied to all enemy max HP.")]
    public float enemyHealthMultiplier = 1.0f;
    [Tooltip("Multiplier applied to all enemy damage values.")]
    public float enemyDamageMultiplier = 1.0f;
}