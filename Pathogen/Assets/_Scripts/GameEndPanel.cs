using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// End game panel — shown after the player unlocks the final lock
public class GameEndPanel : MonoBehaviour
{
    public static GameEndPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI runInfoText;
    [SerializeField] private TextMeshProUGUI rankText;
    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [Header("References")]
    [SerializeField] private MainMenuUI mainMenuUI;
    [Header("Rank Colors")]
    [SerializeField] private Color rankSPlus = new Color(1.0f, 0.85f, 0.0f, 1f);  // gold
    [SerializeField] private Color rankS = new Color(0.8f, 0.8f, 0.8f, 1f);   // silver
    [SerializeField] private Color rankA = new Color(0.2f, 0.7f, 1.0f, 1f);   // blue
    [SerializeField] private Color rankB = new Color(0.6f, 0.6f, 0.6f, 1f);   // grey

    //  Playtime thresholds per difficulty (seconds)
    //  S+ / S / A — anything above A threshold = B
    [Header("Rank Thresholds (seconds)")]
    [SerializeField] private float casualSPlus = 10800f;  // 3h
    [SerializeField] private float casualS = 14400f;  // 4h
    [SerializeField] private float casualA = 18000f;  // 5h

    [SerializeField] private float normalSPlus = 10800f;  // 3h
    [SerializeField] private float normalS = 14400f;  // 4h
    [SerializeField] private float normalA = 18000f;  // 5h

    [SerializeField] private float hardcoreSPlus = 18000f; // 5h
    [SerializeField] private float hardcoreS = 21600f; // 6h
    [SerializeField] private float hardcoreA = 25200f; // 7h

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        panel.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    public void Show()
    {
        panel.SetActive(true);
        TimeScaleManager.Freeze(this);
        WeaponHUD.Instance?.Hide();

        // Gather run data
        float playtime = SaveManager.Instance?.GetCurrentPlaytime() ?? 0f;
        Difficulty difficulty = DifficultyManager.Instance?.Current?.difficulty
                                 ?? Difficulty.Normal;
        int deaths = RunStatsManager.Instance?.DeathCount ?? 0;
        string rank = CalculateRank(playtime, difficulty, deaths);

        // Populate run info
        if (runInfoText != null)
        {
            runInfoText.text =
                "Playtime:   " + FormatTime(playtime) + "\n" +
                "Difficulty: " + difficulty.ToString() + "\n" +
                "Deaths:     " + deaths.ToString() + "\n" +
                "Rank:       " + rank;
        }

        // Rank display
        if (rankText != null)
        {
            rankText.text = rank;
            rankText.color = rank == "S+" ? rankSPlus :
                             rank == "S" ? rankS :
                             rank == "A" ? rankA : rankB;
        }
    }

    private string CalculateRank(float playtime, Difficulty difficulty, int deaths)
    {
        float threshSPlus, threshS, threshA;

        switch (difficulty)
        {
            case Difficulty.Casual:
                threshSPlus = casualSPlus; threshS = casualS; threshA = casualA;
                break;
            case Difficulty.Hardcore:
                threshSPlus = hardcoreSPlus; threshS = hardcoreS; threshA = hardcoreA;
                break;
            default:
                threshSPlus = normalSPlus; threshS = normalS; threshA = normalA;
                break;
        }

        // Determine base rank from playtime
        string baseRank = playtime <= threshSPlus ? "S+" :
                          playtime <= threshS ? "S" :
                          playtime <= threshA ? "A" : "B";

        // S+ requires 0 deaths — drop to S if deaths > 0
        if (baseRank == "S+" && deaths > 0)
            baseRank = "S";

        return baseRank;
    }

    private void OnMainMenu()
    {
        TimeScaleManager.UnfreezeAll();
        panel.SetActive(false);
        mainMenuUI?.ShowMainMenu();
    }

    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private static string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        int s = (int)(seconds % 60);
        return h > 0 ? $"{h}h {m:D2}m {s:D2}s" : $"{m}m {s:D2}s";
    }
}