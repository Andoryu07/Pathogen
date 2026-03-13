using UnityEngine;
using UnityEngine.UI;

///Infection system.
public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance { get; private set; }
    private const int HitsForStage1 = 1;
    private const int HitsForStage2 = 3;
    private const int HitsForStage3 = 5;
    [Header("Screen Overlay Images (assign in Inspector)")]
    [SerializeField] private Image cornersImage;   // blue vignette corners
    [SerializeField] private Image staticImage;    // static/noise overlay
    [Header("Stage 3 Damage")]
    [SerializeField] private float stage3Interval = 30f;   // seconds between ticks
    [SerializeField] private float stage3DamagePerc = 0.10f; // 10% of remaining HP

    private int infectionStage = 0;
    private int infectionHits = 0;
    private bool suppressed = false;
    private float suppressionTimer = 0f;
    private float stage3Timer = 0f;
    private bool statsApplied = false;
    private float appliedHPMult = 0f;
    private float appliedStamMult = 0f;
    public int InfectionStage => infectionStage;
    public bool IsInfected => infectionStage > 0;
    public bool IsSuppressed => suppressed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        SetOverlayAlpha(cornersImage, 0f);
        SetOverlayAlpha(staticImage, 0f);
    }

    void Update()
    {
        // Suppression countdown
        if (suppressed)
        {
            suppressionTimer -= Time.deltaTime;
            if (suppressionTimer <= 0f)
            {
                suppressed = false;
                HUDFeedback.Instance?.ShowWarning("Pain killers worn off — infection active.");
            }
        }
        // Stage 3 periodic damage (skipped while suppressed)
        if (infectionStage == 3 && !suppressed)
        {
            stage3Timer += Time.deltaTime;
            if (stage3Timer >= stage3Interval)
            {
                stage3Timer = 0f;
                ApplyStage3Drain();
            }
        }
    }

    ///Called by EnemyInfected on each successful hit
    public void RegisterInfectedHit()
    {
        infectionHits++;
        int oldStage = infectionStage;
        if (infectionHits >= HitsForStage3) infectionStage = 3;
        else if (infectionHits >= HitsForStage2) infectionStage = 2;
        else if (infectionHits >= HitsForStage1) infectionStage = 1;
        if (infectionStage > oldStage)
            OnStageAdvanced(infectionStage);
        else
            HUDFeedback.Instance?.ShowWarning(
                $"Infected — Stage {infectionStage}. {HitsUntilNext()} hit(s) to next stage.");
    }

    ///Pain Killers: suppress symptoms for duration (seconds)
    public void SuppressSymptoms(float duration)
    {
        suppressed = true;
        suppressionTimer = duration;
        RefreshOverlay();
        HUDFeedback.Instance?.ShowInfo(
            $"Symptoms suppressed for {Mathf.RoundToInt(duration / 60)} min.");
    }

    ///Antidote or Stage-3 full-heal: fully clears all infection
    public void ClearInfection()
    {
        RemoveStatPenalties();
        infectionStage = 0;
        infectionHits = 0;
        suppressed = false;
        suppressionTimer = 0f;
        stage3Timer = 0f;
        RefreshOverlay();
        HUDFeedback.Instance?.ShowHeal("Infection fully cured — stats restored.");
        Debug.Log("[Infection] Cleared.");
    }

    /// Called by PlayerController when healed to full HP
    public void OnPlayerFullyHealed()
    {
        if (infectionStage == 3)
            ClearInfection();
    }

    private void OnStageAdvanced(int stage)
    {
        // Remove old penalties before applying new ones
        RemoveStatPenalties();
        ApplyStatPenalties(stage);
        RefreshOverlay();
        string msg = stage switch
        {
            1 => "Infected — Stage 1! Max HP and Stamina reduced by 20%.",
            2 => "Infection Stage 2 — 40% stat reduction. Find an Antidote!",
            3 => "STAGE 3 — Critical! Taking damage every 30s. Antidote needed NOW!",
            _ => $"Infection Stage {stage}."
        };
        HUDFeedback.Instance?.ShowWarning(msg);
        Debug.Log($"[Infection] Stage {stage}.");
    }

    private void ApplyStatPenalties(int stage)
    {
        PlayerController player = PlayerController.LocalInstance;
        if (player == null) return;
        float mult = stage >= 2 ? 0.40f : 0.20f;
        appliedHPMult = mult;
        appliedStamMult = mult;
        statsApplied = true;
        player.ApplyInfectionPenalty(appliedHPMult, appliedStamMult);
        Debug.Log($"[Infection] Applied {mult * 100}% HP+Stamina penalty.");
    }

    private void RemoveStatPenalties()
    {
        if (!statsApplied) return;
        PlayerController player = PlayerController.LocalInstance;
        player?.RemoveInfectionPenalty(appliedHPMult, appliedStamMult);
        statsApplied = false;
        appliedHPMult = 0f;
        appliedStamMult = 0f;
        Debug.Log("[Infection] Stat penalties removed.");
    }

    private void ApplyStage3Drain()
    {
        PlayerController player = PlayerController.LocalInstance;
        if (player == null) return;

        float drain = player.CurrentHealth * stage3DamagePerc;
        player.TakeDamage(drain);
        HUDFeedback.Instance?.ShowWarning(
            $"Infection draining health! -{Mathf.RoundToInt(drain)} HP");
        Debug.Log($"[Infection] Stage 3 drain: -{drain} HP.");
    }

    private void RefreshOverlay()
    {
        // While suppressed — hide all effects
        if (suppressed)
        {
            SetOverlayAlpha(cornersImage, 0f);
            SetOverlayAlpha(staticImage, 0f);
            return;
        }

        switch (infectionStage)
        {
            case 0:
                SetOverlayAlpha(cornersImage, 0f);
                SetOverlayAlpha(staticImage, 0f);
                break;
            case 1:
                SetOverlayAlpha(cornersImage, 0.18f);
                SetOverlayAlpha(staticImage, 0f);
                break;
            case 2:
                // Stronger corners + light static
                SetOverlayAlpha(cornersImage, 0.38f);
                SetOverlayAlpha(staticImage, 0.12f);
                break;
            case 3:
                // Full intensity
                SetOverlayAlpha(cornersImage, 0.55f);
                SetOverlayAlpha(staticImage, 0.22f);
                break;
        }
    }

    private static void SetOverlayAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        img.color = new Color(c.r, c.g, c.b, alpha);
        img.gameObject.SetActive(alpha > 0f);
    }

    private int HitsUntilNext()
    {
        if (infectionStage == 0) return HitsForStage1 - infectionHits;
        if (infectionStage == 1) return HitsForStage2 - infectionHits;
        if (infectionStage == 2) return HitsForStage3 - infectionHits;
        return 0;
    }
}