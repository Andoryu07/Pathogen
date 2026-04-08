using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
/// Main menu controller. Handles New Game → difficulty selection → load game scene
public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [Header("Difficulty Panel")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Button casualButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardcoreButton;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button backButton;
    [Header("Scene")]
    [SerializeField] private GameObject mainMenuCanvasOrPanel;  // the main menu UI root to hide
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private MainMenuLoadUI loadGameUI;

    [SerializeField] private GameObject pausepanel;
    public static MainMenuUI Instance { get; private set; }
    public bool IsMenuOpen => mainMenuCanvasOrPanel != null && mainMenuCanvasOrPanel.activeSelf;
    private bool pendingNewGame = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        AudioManager.Instance?.StopMovement();
        PlayerController.LocalInstance?.SetMovementEnabled(false);
        TimeScaleManager.Freeze(this);
        mainMenuCanvasOrPanel.SetActive(true);
        mainMenuPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadGameUI != null) loadGameUI.gameObject.SetActive(false);
        gameUIPanel.SetActive(false);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (casualButton != null) casualButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Casual));
        if (normalButton != null) normalButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Normal));
        if (hardcoreButton != null) hardcoreButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Hardcore));
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        if (casualButton != null) AddHoverDescription(casualButton, Difficulty.Casual);
        if (normalButton != null) AddHoverDescription(normalButton, Difficulty.Normal);
        if (hardcoreButton != null) AddHoverDescription(hardcoreButton, Difficulty.Hardcore);
        UpdateDescription(Difficulty.Normal);
        ShowMainMenu();

    }
    private void OnContinue()
    {
        int slot = FindMostRecentSlot();
        mainMenuCanvasOrPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        pausepanel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        PlayerController.LocalInstance?.SetMovementEnabled(true);
        if (slot >= 0) SaveManager.Instance?.Load(slot);
    }

    private void OnLoadGame()
    {
        mainMenuPanel.SetActive(false);
        if (loadGameUI != null) loadGameUI.gameObject.SetActive(true);
        pausepanel.SetActive(false);

    }
    public void OnLoadGameClosed()
    {
        mainMenuPanel.SetActive(true);
    }
    public void StartGameFromLoad(int slot)
    {
        mainMenuCanvasOrPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        TimeScaleManager.Unfreeze(this);
        PlayerController.LocalInstance?.SetMovementEnabled(true);
        AudioManager.Instance?.StopMovement();
        SaveManager.Instance?.Load(slot);
    }
    private void OnSettings()
    {
        mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
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
            if (System.DateTime.TryParse(data.timestamp,
                out System.DateTime t) && t > newest)
            { newest = t; bestSlot = i; }
        }
        return bestSlot;
    }

    private bool HasAnySave()
        => FindMostRecentSlot() >= 0;
    private void OnNewGame()
    {
        // If a baseline exists, load it to reset the scene state
        SaveData baseline = SaveManager.Instance?.ReadSlotMeta(3);
        if (baseline != null && baseline.saveName == "NewGameBaseline")
        {
            mainMenuPanel.SetActive(false);
            difficultyPanel.SetActive(true);
            pendingNewGame = true;
            return;
        }
        mainMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    private void OnDifficultySelected(Difficulty d)
    {
        DifficultyManager.Instance?.SetDifficulty(d);
        difficultyPanel.SetActive(false);
        mainMenuCanvasOrPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        pausepanel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        PlayerController.LocalInstance?.SetMovementEnabled(true);
        AudioManager.Instance?.StopMovement();
        if (pendingNewGame)
        {
            pendingNewGame = false;
            // Reset all managers then load baseline
            SaveManager.Instance?.Load(3);
        }
        else
        {
            // First ever new game — save baseline
            SaveManager.Instance?.SaveNewGameBaseline();
            RunStatsManager.Instance?.ResetForNewGame();

        }
    }

    private void OnBack()
    {
        difficultyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void UpdateDescription(Difficulty d)
    {
        if (descriptionText == null || DifficultyManager.Instance == null) return;
        foreach (var data in DifficultyManager.Instance.GetAllDifficulties())
        {
            if (data != null && data.difficulty == d)
            {
                descriptionText.text = "<b>" + data.displayName + "</b>\n" + data.description;
                return;
            }
        }
    }

    private void AddHoverDescription(Button btn, Difficulty d)
    {
        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                   ?? btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        entry.callback.AddListener(_ => UpdateDescription(d));
        trigger.triggers.Add(entry);
    }

    public void ShowMainMenu()
    {
        mainMenuCanvasOrPanel.SetActive(true);
        mainMenuPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        TimeScaleManager.Freeze(this);
        PlayerController.LocalInstance?.SetMovementEnabled(false);
        AudioManager.Instance?.StopMovement();
        // Refresh save-dependent buttons every time menu opens
        bool hasSave = HasAnySave();
        if (continueButton != null) continueButton.gameObject.SetActive(hasSave);
        if (loadGameButton != null) loadGameButton.gameObject.SetActive(hasSave);
    }
}