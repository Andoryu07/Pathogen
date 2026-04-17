using UnityEngine;
/// World talisman pickup. Press E to smash it and collect the reward
public class TalismanPickup : InteractableBase
{
    [Header("World Sprite")]
    [SerializeField] private float worldScale = 0.35f;

    void Awake()
    {
        promptMessage = $"Smash Talisman";
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            transform.localScale = new Vector3(worldScale, worldScale, 1f);
    }

    public override void Interact()
    {
        GetComponent<PersistentPickup>()?.RegisterCollected();
        TalismanManager.Instance?.CollectTalisman();
        InventoryUIManager.Instance?.RefreshCollectiblesTab();
        gameObject.SetActive(false);
    }
}