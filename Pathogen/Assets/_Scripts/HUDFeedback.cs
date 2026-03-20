using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// Displays a brief feedback message on the HUD
public class HUDFeedback : MonoBehaviour
{
    public static HUDFeedback Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI label;
    [Header("Settings")]
    [SerializeField] private float displayDuration = 2.2f;
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine activeCoroutine;

    private static readonly Color ColHeal = new Color(0.25f, 0.90f, 0.35f, 1f);
    private static readonly Color ColStamina = new Color(0.30f, 0.70f, 1.00f, 1f);
    private static readonly Color ColWarning = new Color(0.95f, 0.35f, 0.25f, 1f);
    private static readonly Color ColInfo = new Color(0.85f, 0.85f, 0.85f, 1f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        feedbackPanel.SetActive(false);
    }

    public void ShowHeal(string msg) => Show(msg, ColHeal);
    public void ShowStamina(string msg) => Show(msg, ColStamina);
    public void ShowWarning(string msg) => Show(msg, ColWarning);
    public void ShowInfo(string msg) => Show(msg, ColInfo);

    private void Show(string msg, Color colour)
    {
        if (label == null) return;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        label.text = msg;
        label.color = colour;
        feedbackPanel.SetActive(true);
        activeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(displayDuration);
        float elapsed = 0f;
        Color start = label.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            label.color = new Color(start.r, start.g, start.b,
                                    Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        feedbackPanel.SetActive(false);
    }
}