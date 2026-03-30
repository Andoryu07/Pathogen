using UnityEngine;
/// Handles aiming, crosshair display, enemy detection under cursor, and firing
public class AimSystem : MonoBehaviour
{
    public static AimSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CrosshairUI crosshairUI;
    [SerializeField] private SpriteRenderer rangeIndicator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject projectilePrefab;
    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [Header("Range Indicator")]
    [SerializeField] private Color rangeCircleColor = new Color(1f, 1f, 1f, 0.06f);

    private bool isAiming = false;
    private bool cursorInRange = false;   
    private float fireCooldown = 0f;
    private WeaponItem equippedWeapon = null;
    private bool warnedOutOfRange = false; 

    public bool IsAiming => isAiming;
    public bool CursorInRange => cursorInRange;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (rangeIndicator != null)
        {
            rangeIndicator.color = rangeCircleColor;
            rangeIndicator.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Always tick cooldown
        if (fireCooldown > 0f) fireCooldown -= Time.deltaTime;
        // Block all aim input while any UI is open
        if (IsAnyUIOpen())
        {
            if (isAiming) ExitAim();
            return;
        }
        // Sync equipped weapon reference from PlayerController
        PlayerController player = PlayerController.LocalInstance;
        equippedWeapon = player?.EquippedWeapon?.GetComponent<WeaponItem>();
        // No weapon or melee — no aiming
        if (equippedWeapon == null || equippedWeapon.IsMelee)
        {
            if (isAiming) ExitAim();
            return;
        }
        // Right-click — aim toggle
        if (Input.GetMouseButtonDown(1)) EnterAim();
        if (Input.GetMouseButtonUp(1)) ExitAim();
        if (isAiming)
        {
            UpdateCrosshair();
            // Left-click — fire (only when cursor is within range)
            if (Input.GetMouseButtonDown(0) && fireCooldown <= 0f && cursorInRange)
                TryFire();
            // R — reload (delegate to WeaponHUD)
            if (Input.GetKeyDown(KeyCode.R))
                WeaponHUD.Instance?.TryReloadPublic();
        }
    }

    private void EnterAim()
    {
        isAiming = true;
        warnedOutOfRange = false;
        // Don't show crosshair here — UpdateCrosshair will show it only if in range
        UpdateRangeIndicator();
        if (rangeIndicator != null) rangeIndicator.gameObject.SetActive(true);
        Cursor.visible = false;
    }

    private void ExitAim()
    {
        isAiming = false;
        crosshairUI?.Hide();
        if (rangeIndicator != null) rangeIndicator.gameObject.SetActive(false);
        Cursor.visible = true;
    }

    private void UpdateCrosshair()
    {
        if (crosshairUI == null || mainCamera == null) return;
        Vector2 mouseScreen = Input.mousePosition;
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mouseScreen);
        float range = equippedWeapon.data.range;
        float dist = Vector2.Distance(transform.position, worldPos);
        bool wasInRange = cursorInRange;
        cursorInRange = dist <= range;

        if (cursorInRange)
        {
            // Cursor is in range — show crosshair and move it
            crosshairUI.Show();
            crosshairUI.MoveTo(mouseScreen);
            // Detect enemy under cursor
            Collider2D hit = Physics2D.OverlapPoint(worldPos, enemyLayer);
            crosshairUI.SetEnemyTarget(hit != null);
            // Reset out-of-range warning flag so it fires again next time
            warnedOutOfRange = false;
        }
        else
        {
            // Cursor is out of range — hide crosshair
            crosshairUI.Hide();
            crosshairUI.SetEnemyTarget(false);
            // Send feedback once when first going out of range
            if (!warnedOutOfRange)
            {
                HUDFeedback.Instance?.ShowWarning("Out of range — move cursor closer to shoot.");
                warnedOutOfRange = true;
            }
        }
    }

    private void UpdateRangeIndicator()
    {
        if (rangeIndicator == null || equippedWeapon == null) return;
        float diameter = equippedWeapon.data.range * 2f;
        rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
    }
    private void TryFire()
    {
        if (equippedWeapon == null) return;

        // Delegate mag check and consumption to WeaponHUD
        bool fired = WeaponHUD.Instance != null && WeaponHUD.Instance.TryFirePublic();
        if (!fired) return;
        AudioManager.Instance?.PlayPistolFire();
        fireCooldown = equippedWeapon.data.fireRate;
        Vector2 origin = transform.position;
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorld - origin).normalized;

        if (projectilePrefab != null)
        {
            // Spawn projectile and configure it
            GameObject go = Instantiate(projectilePrefab, origin, Quaternion.identity);
            Projectile p = go.GetComponent<Projectile>();
            if (p != null)
            {
                float dmgMultiplier = TalismanManager.Instance != null
                    ? TalismanManager.Instance.RangedDamageBonusMultiplier : 1f;
                p.damage = equippedWeapon.data.damage * dmgMultiplier;
                p.range = equippedWeapon.data.range;
                p.enemyLayer = enemyLayer;
                p.Launch(direction);
            }
        }
        else
        {
            // Fallback instant raycast if no prefab assigned
            float dmgMultiplier = TalismanManager.Instance != null
                ? TalismanManager.Instance.RangedDamageBonusMultiplier : 1f;
            float finalDamage = equippedWeapon.data.damage * dmgMultiplier;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction,
                                                  equippedWeapon.data.range, enemyLayer);
            if (hit.collider != null)
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(finalDamage);
            Debug.DrawRay(origin, direction * equippedWeapon.data.range,
                          hit.collider != null ? Color.red : Color.yellow, 0.5f);
        }
    }

    private static bool IsAnyUIOpen()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen) return true;
        if (StoreboxUIManager.Instance != null && StoreboxUIManager.Instance.IsOpen) return true;
        if (CodePadUI.Instance != null && CodePadUI.Instance.IsOpen) return true;
        if (SilasShopUI.Instance != null && SilasShopUI.Instance.IsOpen) return true;
        return false;
    }
}