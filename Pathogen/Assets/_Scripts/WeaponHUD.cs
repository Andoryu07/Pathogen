using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// HUD widget showing the equipped weapon, current mag, and reserve ammo
/// Also handles left-click to fire and R to reload (no animations yet)
public class WeaponHUD : MonoBehaviour
{
    public static WeaponHUD Instance { get; private set; }
    [Header("References")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [Header("Ammo Display")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [Header("Empty State")]
    [SerializeField] private bool hideWhenEmpty = true;
    // Colours for ammo text
    private static readonly Color ColNormal = new Color(0.90f, 0.90f, 0.90f, 1f);
    private static readonly Color ColLow = new Color(0.95f, 0.55f, 0.15f, 1f);  // orange
    private static readonly Color ColEmpty = new Color(0.90f, 0.20f, 0.20f, 1f);  // red
    private WeaponItem equippedWeaponItem = null;
    private bool isReloading = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start() => Refresh(null);

    void Update()
    {   
        // Input is handled by AimSystem — WeaponHUD only manages display and logic.
        // R key reload still works outside of aim mode (quality of life)
        if (equippedWeaponItem == null || equippedWeaponItem.IsMelee) return;
        if (IsAnyUIOpen()) return;
        if (AimSystem.Instance != null && AimSystem.Instance.IsAiming) return;
        string interactKey = InputManager.Instance.GetKeyForAction("Reload").ToString();
        if (InputManager.Instance.GetKey("Reload")) TryReload();
    }
    public void Hide() => hudPanel.SetActive(false);
    public void Show()
    {
        // Only show if something is actually equipped (respects hideWhenEmpty)
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null || player.EquippedWeapon == null)
        {
            if (!hideWhenEmpty) hudPanel.SetActive(true);
            return;
        }
        hudPanel.SetActive(true);
    }

    ///Call whenever the equipped weapon changes
    public void Refresh(Item weapon)
    {
        // Find WeaponItem component on the weapon prefab
        equippedWeaponItem = weapon != null ? weapon.GetComponent<WeaponItem>() : null;
        if (weapon == null)
        {
            if (hideWhenEmpty) { hudPanel.SetActive(false); return; }
            hudPanel.SetActive(true);
            if (weaponIcon != null) weaponIcon.enabled = false;
            if (weaponNameText != null) weaponNameText.text = "No weapon equipped";
            if (ammoText != null) ammoText.enabled = false;
            return;
        }
        hudPanel.SetActive(true);
        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon.GetIcon();
            weaponIcon.enabled = weapon.GetIcon() != null;
        }
        if (weaponNameText != null)
            weaponNameText.text = weapon.GetItemName();
        RefreshAmmoText();
    }

    ///Call after any inventory ammo change to keep reserve count current
    public void RefreshAmmoText()
    {
        if (ammoText == null) return;
        if (equippedWeaponItem == null || equippedWeaponItem.IsMelee)
        {
            ammoText.enabled = false;
            return;
        }
        int mag = equippedWeaponItem.CurrentMag;
        int reserve = equippedWeaponItem.AmmoInInventory();
        ammoText.enabled = true;
        ammoText.text = $"{mag}  /  {reserve}";
        // Colour the text based on mag fullness
        float ratio = equippedWeaponItem.MagSize > 0
            ? (float)mag / equippedWeaponItem.MagSize : 1f;
        ammoText.color = mag == 0 ? ColEmpty :
                         ratio < 0.3f ? ColLow : ColNormal;
    }

    /// Called by AimSystem on left-click while aiming
    /// Returns true ONLY if a round was actually consumed — AimSystem uses this to decide whether to spawn a projectile and apply fire cooldown
    public bool TryFirePublic() => TryFire();

    private bool TryFire()
    {
        if (equippedWeaponItem == null) return false;
        if (isReloading)
        {
            HUDFeedback.Instance?.ShowWarning("Can't fire while reloading!");
            return false;
        }
        if (equippedWeaponItem.CurrentMag <= 0)
        {
            HUDFeedback.Instance?.ShowWarning("Magazine empty — press R to reload.");
            return false;
        }
        bool consumed = equippedWeaponItem.ConsumeRound();
        if (!consumed) return false;
        RefreshAmmoText();
        Debug.Log($"[WeaponHUD] Fired {equippedWeaponItem.data.weaponName} — " +
                  $"{equippedWeaponItem.CurrentMag}/{equippedWeaponItem.MagSize} remaining.");
        // Warn when low
        if (equippedWeaponItem.CurrentMag == 0)
            HUDFeedback.Instance?.ShowWarning("Magazine empty — press R to reload.");
        else if ((float)equippedWeaponItem.CurrentMag / equippedWeaponItem.MagSize < 0.3f)
            HUDFeedback.Instance?.ShowWarning($"Low ammo — {equippedWeaponItem.CurrentMag} rounds left.");

        return true;
    }

    ///Called by AimSystem on R press while aiming
    public void TryReloadPublic() => TryReload();

    private void TryReload()
    {
        if (isReloading)
        {
            HUDFeedback.Instance?.ShowInfo("Already reloading...");
            return;
        }
        if (equippedWeaponItem.CurrentMag >= equippedWeaponItem.MagSize)
        {
            HUDFeedback.Instance?.ShowInfo("Magazine already full.");
            return;
        }
        if (equippedWeaponItem.AmmoInInventory() <= 0)
        {
            HUDFeedback.Instance?.ShowWarning($"No {equippedWeaponItem.AmmoItemName} in inventory.");
            return;
        }
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        float duration = equippedWeaponItem.GetReloadDuration();
        HUDFeedback.Instance?.ShowInfo($"Reloading... ({duration:F1}s)");
        AudioManager.Instance?.PlayPistolReload();
        yield return new WaitForSecondsRealtime(duration);
        if (equippedWeaponItem == null) { isReloading = false; yield break; }
        int loaded = equippedWeaponItem.Reload();
        RefreshAmmoText();
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        if (loaded > 0)
            HUDFeedback.Instance?.ShowInfo(
                $"Reloaded +{loaded} — {equippedWeaponItem.CurrentMag}/{equippedWeaponItem.MagSize}");
        else
            HUDFeedback.Instance?.ShowWarning($"No {equippedWeaponItem.AmmoItemName} in inventory.");

        isReloading = false;
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