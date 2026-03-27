using UnityEngine;
using UnityEngine.SceneManagement;
/// Persists the chosen difficulty across scenes and provides multipliers to enemies
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Available Difficulties (assign all 3 assets)")]
    [SerializeField] private DifficultyData[] difficulties;

    private DifficultyData current;

    public float HealthMultiplier => current != null ? current.enemyHealthMultiplier : 1f;
    public float DamageMultiplier => current != null ? current.enemyDamageMultiplier : 1f;
    public DifficultyData Current => current;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // Default to Normal if nothing chosen yet
        SetDifficulty(Difficulty.Normal);
    }

    public void SetDifficulty(Difficulty d)
    {
        if (difficulties == null) return;
        foreach (var data in difficulties)
        {
            if (data != null && data.difficulty == d)
            {
                current = data;
                Debug.Log("[Difficulty] Set to: " + d);
                return;
            }
        }
        Debug.LogWarning("[Difficulty] No DifficultyData found for: " + d);
    }

    public DifficultyData[] GetAllDifficulties() => difficulties;
}