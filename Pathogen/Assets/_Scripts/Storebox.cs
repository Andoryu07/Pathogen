using UnityEngine;
public class Storebox : InteractableBase
{
    void Awake()
    {
        promptMessage = $"Open storage box";
    }

    public override void Interact()
    {
        if (StoreboxUIManager.Instance != null)
            StoreboxUIManager.Instance.OpenStorebox();
        else
            Debug.LogWarning("[Storebox] StoreboxUIManager not found in scene!");
    }
}