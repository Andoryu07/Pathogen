using UnityEngine;
using UnityEngine.Rendering.Universal;
/// Controls the Lighter — toggled with L key
public class LighterController : MonoBehaviour
{
    public static LighterController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Light2D lighterLight;
    [SerializeField] private SpriteRenderer darkOverlay;   // world-space black sprite (optional)
    [Header("Lighter Settings")]
    [SerializeField] private float lightRadius = 5f;
    [SerializeField] private float lightIntensity = 1.2f;
    [Header("Dark Area Settings")]
    [SerializeField] private float darkAlpha = 0.97f;
    [SerializeField] private float fadeSpeed = 3f;

    private bool lighterOn = false;
    private bool inDarkZone = false;
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
            darkOverlay.color = new Color(0f, 0f, 0f, 0f);
            darkOverlay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!SpecialItemManager.Instance.HasLighter) return;
            lighterOn = !lighterOn;
            if (lighterLight != null) lighterLight.gameObject.SetActive(lighterOn);
            HUDFeedback.Instance?.ShowInfo(lighterOn ? "Lighter on." : "Lighter off.");
        }
        if (darkOverlay != null)
        {
            if (inDarkZone && !darkOverlay.gameObject.activeSelf)
                darkOverlay.gameObject.SetActive(true);
            float current = darkOverlay.color.a;
            float target = inDarkZone ? darkAlpha : 0f;
            if (!Mathf.Approximately(current, target))
            {
                float next = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
                darkOverlay.color = new Color(0f, 0f, 0f, next);
            }
            else if (!inDarkZone && Mathf.Approximately(current, 0f))
            {
                darkOverlay.gameObject.SetActive(false);
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