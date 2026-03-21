using UnityEngine;
/// Registers the ItemRegistry ScriptableObject at startup
public class ItemRegistryLoader : MonoBehaviour
{
    [SerializeField] private ItemRegistry registry;

    void Awake()
    {
        if (registry != null)
            ItemRegistry.Register(registry);
        else
            Debug.LogError("[ItemRegistryLoader] No ItemRegistry assigned!");
    }
}