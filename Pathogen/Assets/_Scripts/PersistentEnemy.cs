using UnityEngine;

/// Gives each enemy a unique scene-level ID. On Start, checks if this enemy was already killed in a previous session and deactivates itself.
/// On death the enemy's DieRoutine should call RegisterDeath()
public class PersistentEnemy : MonoBehaviour
{
    [Header("Persistent ID")]
    [Tooltip("Unique ID for this enemy. Must be unique across the entire scene.")]
    [SerializeField] private string sceneID = "";


    void Start()
    {
        if (string.IsNullOrEmpty(sceneID))
        {
            Debug.LogWarning("[PersistentEnemy] No sceneID set on " + gameObject.name);
            return;
        }

        bool wpmExists = WorldPersistenceManager.Instance != null;
        bool isDead = wpmExists && WorldPersistenceManager.Instance.IsEnemyDead(sceneID);

        Debug.Log("[PersistentEnemy] Start — ID:" + sceneID +
                  " WPM exists:" + wpmExists +
                  " isDead:" + isDead);

        if (isDead)
            gameObject.SetActive(false);
    }

    ///Call this from the enemy's DieRoutine just before Destroy
    public void RegisterDeath()
    {
        if (!string.IsNullOrEmpty(sceneID))
            WorldPersistenceManager.Instance?.RegisterEnemyDeath(sceneID);
    }

    public string SceneID => sceneID;
    ///Returns current HP by reading from the enemy script on this GO
    public float GetCurrentHP()
    {
        var infected = GetComponent<EnemyInfected>();
        if (infected != null) return infected.CurrentHealth;
        var stalker = GetComponent<AnomalyStalker>();
        if (stalker != null) return stalker.CurrentHealth;
        var leaper = GetComponent<AnomalyLeaper>();
        if (leaper != null) return leaper.CurrentHealth;
        var brute = GetComponent<AnomalyBrute>();
        if (brute != null) return brute.CurrentHealth;
        var volkov = GetComponent<VolkovBoss>();
        if (volkov != null) return volkov.CurrentHealth;
        return 0f;
    }

    ///Applies saved HP to the enemy script
    public void ApplyHP(float hp)
    {
        var infected = GetComponent<EnemyInfected>();
        if (infected != null) { infected.SetCurrentHealth(hp); return; }
        var stalker = GetComponent<AnomalyStalker>();
        if (stalker != null) { stalker.SetCurrentHealth(hp); return; }
        var leaper = GetComponent<AnomalyLeaper>();
        if (leaper != null) { leaper.SetCurrentHealth(hp); return; }
        var brute = GetComponent<AnomalyBrute>();
        if (brute != null) { brute.SetCurrentHealth(hp); return; }
        var volkov = GetComponent<VolkovBoss>();
        if (volkov != null) { volkov.SetCurrentHealth(hp); return; }
    }
}