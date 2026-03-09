using UnityEngine;
/// Handles all Use and Equip logic for items.
public class UseItemHandler : MonoBehaviour
{
    public static UseItemHandler Instance { get; private set; }
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    /// Attempts to use a consumable item. Returns true if the item had a valid effect and should be consumed from inventory.
    public bool TryUseItem(Item item)
    {
        if (item == null) return false;
        float maxHP = playerController.MaxHealth;
        switch (item.GetItemName())
        {
            case "Bandage":
                playerController.Heal(maxHP * 0.20f);
                ShowFeedback("Used Bandage  20% HP");
                return true;
            case "Enhanced Bandage":
                playerController.Heal(maxHP * 0.80f);
                ShowFeedback("Used Enhanced Bandage  +80% HP");
                return true;
            case "Herbal Extract":
                playerController.Heal(maxHP);
                ShowFeedback("Used Herbal Extract  — fully healed");
                return true;
            case "First Aid Kit":
                playerController.Heal(maxHP);
                ShowFeedback("Used First Aid Kit  — fully healed");
                return true;
            case "Antidote":
                //Infection not implemented yet, 10% heal as temporary effect
                playerController.Heal(maxHP * 0.10f);
                ShowFeedback("Used Antidote — infection neutralised");
                return true;
            // ---- Stamina ----
            case "Caffeine":
                // permanently increases max stamina by 15%
                playerController.AddStaminaBonus(0.15f);
                ShowFeedback("Used Caffeine  +15% Max Stamina (permanent)");
                return true;
            // ---- Throwables (placeholder — no throw system yet) ----
            case "Molotov Cocktail":
                ShowFeedback("Molotov ready — throw system coming soon");
                return false;
            case "Pipe Bomb":
                ShowFeedback("Pipe Bomb ready — throw system coming soon");
                return false;
            default:
                ShowFeedback($"Cannot use {item.GetItemName()} right now.");
                return false;
        }
    }

    private void ShowFeedback(string msg)
    {
        Debug.Log($"[UseItem] {msg}");
        //Will be hooked into HUD once it's created
    }
}