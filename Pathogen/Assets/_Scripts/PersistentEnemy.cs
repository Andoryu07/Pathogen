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
}