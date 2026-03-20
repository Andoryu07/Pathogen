using UnityEngine;
/// Hip Pouch world pickup
public class HipPouchPickup : InteractableBase
{
    [Header("World Sprite")]
    [SerializeField] private float worldScale = 0.3f;

    void Awake()
    {
        promptMessage = "E - Pick up Hip Pouch";
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) transform.localScale = new Vector3(worldScale, worldScale, 1f);
    }

    public override void Interact()
    {
        SpecialItemManager.Instance?.PickUpHipPouch();
        gameObject.SetActive(false);
    }
}