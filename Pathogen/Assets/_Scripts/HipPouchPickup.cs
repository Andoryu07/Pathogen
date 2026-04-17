using UnityEngine;
/// Hip Pouch world pickup
/// Immediately expands the inventory grid by 1 row when picked up
/// Does NOT appear in inventory
public class HipPouchPickup : InteractableBase
{
    [Header("World Sprite")]
    [SerializeField] private float worldScale = 0.3f;
    string interactKey = InputManager.Instance.GetKeyForAction("Interact").ToString();

    void Awake()
    {
        promptMessage = $"{interactKey} - Pick up Hip Pouch";
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) transform.localScale = new Vector3(worldScale, worldScale, 1f);
    }

    public override void Interact()
    {
        SpecialItemManager.Instance?.PickUpHipPouch();
        GetComponent<PersistentPickup>()?.RegisterCollected();
        gameObject.SetActive(false);
    }
}