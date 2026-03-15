using UnityEngine;
/// Silas NPC
public class SilasNPC : InteractableBase
{
    [Header("References")]
    [SerializeField] private SilasShopUI silasShopUI;

    void Awake()
    {
        promptMessage = "E - Talk to Silas";
    }

    public override void Interact()
    {
        if (silasShopUI == null)
        {
            Debug.LogWarning("[SilasNPC] SilasShopUI reference not assigned!");
            return;
        }
        silasShopUI.Open();
    }
}