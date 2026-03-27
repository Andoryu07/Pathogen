using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
/// Main menu controller. Handles New Game → difficulty selection → load game scene
public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;
    [Header("Difficulty Panel")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Button casualButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardcoreButton;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button backButton;
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Prototype";

    void Start()
    {
        mainMenuPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (casualButton != null) casualButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Casual));
        if (normalButton != null) normalButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Normal));
        if (hardcoreButton != null) hardcoreButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Hardcore));
        if (backButton != null) backButton.onClick.AddListener(OnBack);

        // Show descriptions on hover
        if (casualButton != null) AddHoverDescription(casualButton, Difficulty.Casual);
        if (normalButton != null) AddHoverDescription(normalButton, Difficulty.Normal);
        if (hardcoreButton != null) AddHoverDescription(hardcoreButton, Difficulty.Hardcore);

        // Set default description
        UpdateDescription(Difficulty.Normal);
    }

    private void OnNewGame()
    {
        mainMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    private void OnLoadGame()
    {
        // Placeholder — will open save/load UI when main menu is fleshed out
        Debug.Log("[MainMenu] Load game — not yet implemented.");
    }

    private void OnDifficultySelected(Difficulty d)
    {
        DifficultyManager.Instance?.SetDifficulty(d);
        Debug.Log("[MainMenu] Starting game on difficulty: " + d);
        SceneManager.LoadScene(gameSceneName);
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
}