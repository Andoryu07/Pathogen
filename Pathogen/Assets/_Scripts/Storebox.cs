using UnityEngine;
/// Place this on a box GameObject in the world alongside a Collider2D
/// (set to trigger, on the Interactable layer).
/// When the player presses E nearby, it opens the StoreboxUI panel.
public class Storebox : InteractableBase
{
    void Awake()
    {
        promptMessage = "E - Open storage box";
    }

    public override void Interact()
    {
        if (StoreboxUIManager.Instance != null)
            StoreboxUIManager.Instance.OpenStorebox();
        else
            Debug.LogWarning("[Storebox] StoreboxUIManager not found in scene!");
    }
}