using UnityEngine;
/// Handles melee attack input for the player
[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    private PlayerController playerController;
    private Camera mainCamera;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        mainCamera = Camera.main;
    }

    void Start()
    {
    }

    void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        HandleMeleeInput();
    }

    private void HandleMeleeInput()
    {
        Item equipped = playerController.EquippedWeapon;

        if (Input.GetMouseButtonDown(0))
        {
            if (equipped == null) { Debug.Log("[PlayerCombat] No weapon equipped."); return; }
            WeaponItem wi = equipped.GetComponent<WeaponItem>() ?? equipped.GetComponentInChildren<WeaponItem>() ?? equipped.GetComponentInParent<WeaponItem>();
            if (wi == null || !wi.IsMelee) { Debug.Log("[PlayerCombat] Not a melee weapon — skipping."); return; }
            bool aiming = AimSystem.Instance != null && AimSystem.Instance.IsAiming;
            if (aiming || IsAnyUIOpen()) return;
            MeleeWeapon melee = GetComponent<MeleeWeapon>();
            if (melee == null) return;
            bool hit = melee.TryAttack(GetMouseDirection());
            Debug.Log("[PlayerCombat] TryAttack result: " + hit);
            if (hit)
                HUDFeedback.Instance?.ShowInfo("Swung " + equipped.GetItemName() + "!");
        }
    }
    private static bool IsAnyUIOpen()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen) return true;
        if (StoreboxUIManager.Instance != null && StoreboxUIManager.Instance.IsOpen) return true;
        if (CodePadUI.Instance != null && CodePadUI.Instance.IsOpen) return true;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return true;
        return false;
    }
    public Vector2 GetMouseDirection()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return ((Vector2)(mouseWorld - transform.position)).normalized;
    }
}