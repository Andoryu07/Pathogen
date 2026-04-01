using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
/// Shown when the player dies. Freezes time, shows cause summary
public class GameOverPanel : MonoBehaviour
{
    public static GameOverPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainMenuButton;
    [Header("Fade In")]
    [SerializeField] private float fadeInDuration = 1.2f;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private MainMenuUI mainMenuUI;  

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        panel.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(onMainMenu);

    }

    public void Show(string cause = "")
    {
        panel.SetActive(true);
        TimeScaleManager.Freeze(this);
        if (retryButton != null)
            retryButton.gameObject.SetActive(FindMostRecentSlot() >= 0);
        if (titleText != null) titleText.text = "YOU ARE DEAD";
        if (subtitleText != null)
        {
            float playtime = SaveManager.Instance != null
                ? SaveManager.Instance.GetCurrentPlaytime() : 0f;
            subtitleText.text = (string.IsNullOrEmpty(cause) ? "" : cause + "\n")
                              + "Survived: " + FormatPlaytime(playtime);
        }
        if (canvasGroup != null)
            StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void OnRetry()
    {
        TimeScaleManager.UnfreezeAll();
        panel.SetActive(false);
        int slot = FindMostRecentSlot();
        if (slot >= 0) SaveManager.Instance?.Load(slot);
    }
    private void onMainMenu()
    {
        TimeScaleManager.UnfreezeAll();
        panel.SetActive(false);
        mainMenuUI.ShowMainMenu();
    }
    private void OnQuit()
    {
        TimeScaleManager.UnfreezeAll();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private int FindMostRecentSlot()
    {
        if (SaveManager.Instance == null) return -1;
        System.DateTime newest = System.DateTime.MinValue;
        int bestSlot = -1;
        for (int i = 0; i < SaveManager.Instance.MaxSlots; i++)
        {
            SaveData data = SaveManager.Instance.ReadSlotMeta(i);
            if (data == null) continue;
            if (System.DateTime.TryParse(data.timestamp, out System.DateTime t) && t > newest)
            {
                newest = t;
                bestSlot = i;
            }
        }
        return bestSlot;
    }

    private static string FormatPlaytime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        int s = (int)(seconds % 60);
        return h > 0 ? $"{h}h {m:D2}m" : $"{m}m {s:D2}s";
    }
}