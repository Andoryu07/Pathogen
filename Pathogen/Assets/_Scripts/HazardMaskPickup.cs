using UnityEngine;
/// Hazard Mask world pickup
/// Does NOT appear in inventory — sets ownership flag in SpecialItemManager
public class HazardMaskPickup : InteractableBase
{
    [Header("World Sprite")]
    [SerializeField] private float worldScale = 0.3f;
    string interactKey = InputManager.Instance.GetKeyForAction("Interact").ToString();

    void Awake()
    {
        promptMessage = $"{interactKey} - Pick up Hazard Mask";
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) transform.localScale = new Vector3(worldScale, worldScale, 1f);
    }

    public override void Interact()
    {
        SpecialItemManager.Instance?.PickUpHazardMask();
        GetComponent<PersistentPickup>()?.RegisterCollected();
        gameObject.SetActive(false);
    }
}