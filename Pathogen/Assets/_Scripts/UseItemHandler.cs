using UnityEngine;
/// Handles all Use and Equip logic for consumable items.
public class UseItemHandler : MonoBehaviour
{
    public static UseItemHandler Instance { get; private set; }
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [Header("Pain Killer Settings")]
    [SerializeField] private float painKillerDuration = 300f; // 5 minutes in seconds
    private bool painKillersActive = false;
    private float painKillerTimer = 0f;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }
    void Update()
    {
        if (painKillersActive)
        {
            painKillerTimer -= Time.deltaTime;
            if (painKillerTimer <= 0f)
            {
                painKillersActive = false;
                painKillerTimer = 0f;
                Debug.Log("[UseItem] Pain Killers worn off — infection effects resumed.");
            }
        }
    }

    /// Attempts to use a consumable item
    /// Returns true if the item had a valid effect and should be removed from inventory. Returns false to keep it.
    public bool TryUseItem(Item item)
    {
        if (item == null) return false;
        float maxHP = playerController.MaxHealth;
        switch (item.GetItemName())
        {
            case "Bandage":
                playerController.Heal(maxHP * 0.20f);
                ShowHealFeedback("Bandage  +20% HP");
                return true;
            case "Antiseptic Spray":
                playerController.Heal(maxHP * 0.50f);
                ShowHealFeedback("Antiseptic Spray  +50% HP");
                return true;
            case "Enhanced Bandage":
                playerController.Heal(maxHP * 0.80f);
                ShowHealFeedback("Enhanced Bandage  +80% HP");
                return true;
            case "Herbal Extract":
                playerController.Heal(maxHP);
                ShowHealFeedback("Herbal Extract — fully healed");
                return true;
            case "First Aid Kit":
                playerController.Heal(maxHP);
                ShowHealFeedback("First Aid Kit — fully healed");
                return true;
            case "Pain Killers":
                painKillersActive = true;
                painKillerTimer = painKillerDuration;
                // Hook: InfectionManager.Instance?.SuppressSymptoms(painKillerDuration);
                ShowFeedback("Pain Killers — symptoms suppressed 5 min");
                return true;
            case "Antidote":
                // Hook: InfectionManager.Instance?.ClearInfection();
                ShowFeedback("Antidote — infection cured");
                return true;
            case "Caffeine":
                playerController.AddStaminaBonus(0.15f);
                ShowStaminaFeedback("Caffeine — +15% Max Stamina (permanent)");
                return true;
            case "Crowbar":
            case "Pistol":
            case "Shotgun":
            case "Hunting Rifle":
                ShowFeedback($"{item.GetItemName()} is a weapon — use Equip.");
                return false;
            case "Molotov Cocktail":
            case "Pipe Bomb":
                ShowFeedback($"{item.GetItemName()} — throw system not implemented yet.");
                return false;
            default:
                ShowFeedback($"Cannot use {item.GetItemName()} right now.");
                return false;
        }
    }
    public bool ArePainKillersActive() => painKillersActive;
    public float PainKillerTimeRemaining() => painKillerTimer;
    private void ShowFeedback(string msg) => ShowFeedbackColour(msg, FeedbackType.Info);
    private void ShowHealFeedback(string msg) => ShowFeedbackColour(msg, FeedbackType.Heal);
    private void ShowStaminaFeedback(string msg) => ShowFeedbackColour(msg, FeedbackType.Stamina);
    private void ShowWarnFeedback(string msg) => ShowFeedbackColour(msg, FeedbackType.Warn);
    private enum FeedbackType { Heal, Stamina, Info, Warn }
    private void ShowFeedbackColour(string msg, FeedbackType type)
    {
        Debug.Log($"[UseItem] {msg}");
        if (HUDFeedback.Instance == null) return;
        switch (type)
        {
            case FeedbackType.Heal: HUDFeedback.Instance.ShowHeal(msg); break;
            case FeedbackType.Stamina: HUDFeedback.Instance.ShowStamina(msg); break;
            case FeedbackType.Warn: HUDFeedback.Instance.ShowWarning(msg); break;
            default: HUDFeedback.Instance.ShowInfo(msg); break;
        }
    }
}