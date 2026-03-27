using UnityEngine;
using UnityEngine.UI;
/// Bloody screen edge overlay based on player HP
/// 100%       — no effect
/// 99–50%     — slight blood corners, -20% movement speed
/// 50–1%      — full blood corners + reduced visibility, -40% movement speed
public class HealthOverlay : MonoBehaviour
{
    public static HealthOverlay Instance { get; private set; }

    [Header("Overlay Images")]
    [SerializeField] private Image slightBloodImage;  
    [SerializeField] private Image heavyBloodImage;   
    [Header("Thresholds")]
    [SerializeField] private float slightThreshold = 0.99f;  
    [SerializeField] private float heavyThreshold = 0.50f;  
    [Header("Speed Penalties")]
    [SerializeField] private float slightSpeedPenalty = 0.20f;  
    [SerializeField] private float heavySpeedPenalty = 0.40f;  
    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 3f;

    private enum HealthState { Full, Slight, Heavy }
    private HealthState currentState = HealthState.Full;
    private HealthState lastAppliedState = HealthState.Full;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        SetImageAlpha(slightBloodImage, 0f);
        SetImageAlpha(heavyBloodImage, 0f);
    }

    void Update()
    {
        PlayerController player = PlayerController.LocalInstance;
        if (player == null) return;
        float ratio = player.MaxHealth > 0 ? player.CurrentHealth / player.MaxHealth : 1f;
        HealthState newState = ratio >= slightThreshold ? HealthState.Full : ratio >= heavyThreshold ? HealthState.Slight : HealthState.Heavy;
        // Apply speed penalty if state changed
        if (newState != lastAppliedState)
        {
            ApplySpeedPenalty(lastAppliedState, newState);
            lastAppliedState = newState;
        }
        currentState = newState;
        UpdateOverlays(currentState, ratio);
    }

    private void UpdateOverlays(HealthState state, float ratio)
    {
        switch (state)
        {
            case HealthState.Full:
                FadeImage(slightBloodImage, 0f);
                FadeImage(heavyBloodImage, 0f);
                break;
            case HealthState.Slight:
                // Intensity scales with how low HP is in the 99-50% range
                float slightIntensity = 1f - ((ratio - heavyThreshold) /
                                              (slightThreshold - heavyThreshold));
                FadeImage(slightBloodImage, Mathf.Clamp01(slightIntensity));
                FadeImage(heavyBloodImage, 0f);
                break;
            case HealthState.Heavy:
                // Intensity scales with how low HP is in the 50-1% range
                float heavyIntensity = 1f - (ratio / heavyThreshold);
                FadeImage(slightBloodImage, 1f);
                FadeImage(heavyBloodImage, Mathf.Clamp01(heavyIntensity));
                break;
        }
    }
    public void ResetOverlays()
    {
        SetImageAlpha(slightBloodImage, 0f);
        SetImageAlpha(heavyBloodImage, 0f);
        if (slightBloodImage != null) slightBloodImage.gameObject.SetActive(false);
        if (heavyBloodImage != null) heavyBloodImage.gameObject.SetActive(false);
        lastAppliedState = HealthState.Full;
        currentState = HealthState.Full;
        PlayerController.LocalInstance?.RemoveSpeedPenalty(heavySpeedPenalty);
        PlayerController.LocalInstance?.RemoveSpeedPenalty(slightSpeedPenalty);
    }
    private void FadeImage(Image img, float targetAlpha)
    {
        if (img == null) return;
        Color c = img.color;
        float newAlpha = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        img.color = new Color(c.r, c.g, c.b, newAlpha);
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        img.color = new Color(c.r, c.g, c.b, alpha);
    }

    private void ApplySpeedPenalty(HealthState oldState, HealthState newState)
    {
        PlayerController player = PlayerController.LocalInstance;
        if (player == null) return;
        // Remove old penalty
        switch (oldState)
        {
            case HealthState.Slight: player.RemoveSpeedPenalty(slightSpeedPenalty); break;
            case HealthState.Heavy: player.RemoveSpeedPenalty(heavySpeedPenalty); break;
        }
        // Apply new penalty
        switch (newState)
        {
            case HealthState.Slight: player.ApplySpeedPenalty(slightSpeedPenalty); break;
            case HealthState.Heavy: player.ApplySpeedPenalty(heavySpeedPenalty); break;
        }
    }
}