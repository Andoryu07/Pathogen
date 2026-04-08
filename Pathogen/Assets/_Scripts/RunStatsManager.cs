using UnityEngine;
/// Tracks run statistics — currently death count
public class RunStatsManager : MonoBehaviour
{
    public static RunStatsManager Instance { get; private set; }
    private int deathCount = 0;
    public int DeathCount => deathCount;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void RegisterDeath()
    {
        deathCount++;
        Debug.Log("[RunStats] Deaths: " + deathCount);
    }

    public void ResetForNewGame()
    {
        deathCount = 0;
    }
    public int GetDeathCount() => deathCount;
    public void LoadDeathCount(int count) => deathCount = Mathf.Max(0, count);
}