using UnityEngine;
/// On Start checks if already collected and deactivates. On pickup registers as collected
public class PersistentPickup : MonoBehaviour
{
    [Header("Persistent ID")]
    [Tooltip("Unique ID for this pickup. Must be unique across the entire scene.")]
    [SerializeField] private string sceneID = "";

    void Start()
    {
        if (string.IsNullOrEmpty(sceneID))
        {
            Debug.LogWarning("[PersistentPickup] No sceneID set on " + gameObject.name);
            return;
        }

        // Already collected in a previous session — deactivate
        if (WorldPersistenceManager.Instance != null &&
            WorldPersistenceManager.Instance.IsPickupCollected(sceneID))
        {
            gameObject.SetActive(false);
        }
    }

    /// The Item script calls gameObject.SetActive(false)
    public void RegisterCollected()
    {
        if (!string.IsNullOrEmpty(sceneID))
            WorldPersistenceManager.Instance?.RegisterPickupCollected(sceneID);
    }

    public string SceneID => sceneID;
}