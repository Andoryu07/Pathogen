using UnityEngine;
using System.Collections;

/// Controls the Hazard Mask — toggled with H key when owned
public class HazardMaskController : MonoBehaviour
{
    public static HazardMaskController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Image hazardOverlay;
    [Header("Hazard Settings")]
    [SerializeField] private float damageInterval = 3f;    // seconds between damage ticks
    [SerializeField] private float damageAmount = 10f;   // HP lost per tick
    [SerializeField] private Color overlayColor = new Color(0.1f, 0.6f, 0.1f, 0.25f); // green tint

    private bool maskEquipped = false;
    private bool inHazardZone = false;
    private Coroutine damageCoroutine;
    public bool MaskEquipped => maskEquipped;
    public bool InHazardZone => inHazardZone;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (hazardOverlay != null)
        {
            hazardOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            hazardOverlay.raycastTarget = false;
            hazardOverlay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Toggle mask with H — only if owned and in hazard zone
        if (InputManager.Instance.GetKey("HazardMask"))
        {
            if (!SpecialItemManager.Instance.HasHazardMask)
            {
                HUDFeedback.Instance?.ShowWarning("You don't have a Hazard Mask.");
                return;
            }
            if (!inHazardZone)
            {
                HUDFeedback.Instance?.ShowInfo("No hazardous area detected.");
                return;
            }
            ToggleMask();
        }
    }

    private void ToggleMask()
    {
        maskEquipped = !maskEquipped;
        if (hazardOverlay != null)
        {
            hazardOverlay.gameObject.SetActive(maskEquipped);
            hazardOverlay.color = maskEquipped ? overlayColor : new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        }
        HUDFeedback.Instance?.ShowInfo(maskEquipped ? "Hazard Mask equipped — protected from contamination." : "Hazard Mask removed.");
        // Stop or start damage based on new state
        if (inHazardZone)
        {
            if (maskEquipped) StopHazardDamage();
            else StartHazardDamage();
        }
    }

    public void EnterHazardZone()
    {
        inHazardZone = true;
        HUDFeedback.Instance?.ShowWarning("Hazardous area! Press H to equip your Hazard Mask.");

        if (!maskEquipped)
            StartHazardDamage();
    }

    public void ExitHazardZone()
    {
        inHazardZone = false;
        StopHazardDamage();
        // Auto-unequip mask on exit
        if (maskEquipped)
        {
            maskEquipped = false;
            if (hazardOverlay != null) hazardOverlay.gameObject.SetActive(false);
            HUDFeedback.Instance?.ShowInfo("Left hazardous area — Hazard Mask removed.");
        }
    }

    private void StartHazardDamage()
    {
        if (damageCoroutine != null) return;
        damageCoroutine = StartCoroutine(HazardDamageRoutine());
    }

    private void StopHazardDamage()
    {
        if (damageCoroutine == null) return;
        StopCoroutine(damageCoroutine);
        damageCoroutine = null;
    }

    private IEnumerator HazardDamageRoutine()
    {
        while (inHazardZone && !maskEquipped)
        {
            yield return new WaitForSeconds(damageInterval);
            if (!inHazardZone || maskEquipped) break;

            PlayerController.LocalInstance?.TakeDamage(damageAmount);
            HUDFeedback.Instance?.ShowWarning(
                $"Contamination! -{damageAmount} HP — equip your Hazard Mask!");
        }
        damageCoroutine = null;
    }
}