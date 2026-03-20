using UnityEngine;
using UnityEngine.Rendering.Universal;

/// Controls the Lighter — toggled with L key
public class LighterController : MonoBehaviour
{
    public static LighterController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Light2D lighterLight;
    [SerializeField] private UnityEngine.UI.Image darkOverlay;
    [Header("Lighter Settings")]
    [SerializeField] private float lightRadius = 4f;
    [SerializeField] private float lightIntensity = 1.2f;
    [Header("Dark Area Settings")]
    [SerializeField] private float darkAlpha = 0.97f;  // nearly black
    [SerializeField] private float fadeSpeed = 3f;     // alpha transition speed

    private bool lighterOn = false;
    private bool inDarkZone = false;
    private float targetAlpha = 0f;
    public bool LighterOn => lighterOn;
    public bool InDarkZone => inDarkZone;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (lighterLight != null)
        {
            lighterLight.pointLightOuterRadius = lightRadius;
            lighterLight.intensity = lightIntensity;
            lighterLight.gameObject.SetActive(false);
        }
        if (darkOverlay != null)
        {
            var c = darkOverlay.color;
            darkOverlay.color = new Color(c.r, c.g, c.b, 0f);
            darkOverlay.raycastTarget = false;
        }
    }

    void Update()
    {
        // Toggle lighter with L
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!SpecialItemManager.Instance.HasLighter) return;
            lighterOn = !lighterOn;
            if (lighterLight != null) lighterLight.gameObject.SetActive(lighterOn);
            HUDFeedback.Instance?.ShowInfo(lighterOn ? "Lighter on." : "Lighter off.");
        }
        // Fade overlay toward target alpha
        if (darkOverlay != null)
        {
            float current = darkOverlay.color.a;
            float target = inDarkZone ? darkAlpha : 0f;
            if (!Mathf.Approximately(current, target))
            {
                float next = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
                var c = darkOverlay.color;
                darkOverlay.color = new Color(c.r, c.g, c.b, next);
            }
        }
    }

    public void EnterDarkZone()
    {
        inDarkZone = true;
        if (!lighterOn)
            HUDFeedback.Instance?.ShowWarning("It's dark here — press L to use your lighter.");
    }

    public void ExitDarkZone()
    {
        inDarkZone = false;
    }
}
