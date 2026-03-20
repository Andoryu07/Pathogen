using UnityEngine;
/// Hazard Mask world pickup
public class HazardMaskPickup : InteractableBase
{
    [Header("World Sprite")]
    [SerializeField] private float worldScale = 0.3f;

    void Awake()
    {
        promptMessage = "E - Pick up Hazard Mask";
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) transform.localScale = new Vector3(worldScale, worldScale, 1f);
    }

    public override void Interact()
    {
        SpecialItemManager.Instance?.PickUpHazardMask();
        gameObject.SetActive(false);
    }
}