using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


/// Pause menu — press Escape to open/close
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject pausePanel;
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    public bool IsPaused => pausePanel != null && pausePanel.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        pausePanel.SetActive(false);

        if (continueButton != null) continueButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    void Update()
    {
        // Only open pause with Escape if no other UI is blocking
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else if (!IsAnyBlockingUIOpen())
                Pause();
        }
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        TimeScaleManager.Freeze(this);
        WeaponHUD.Instance?.Hide();
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        WeaponHUD.Instance?.Show();
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    private void OnSettings()
    {
        HUDFeedback.Instance?.ShowInfo("Settings — coming soon.");
    }

    private void OnMainMenu()
    {
        TimeScaleManager.UnfreezeAll();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnQuit()
    {
        Debug.Log("[PauseMenu] Quit requested.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private static bool IsAnyBlockingUIOpen()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen) return true;
        if (StoreboxUIManager.Instance != null && StoreboxUIManager.Instance.IsOpen) return true;
        if (CodePadUI.Instance != null && CodePadUI.Instance.IsOpen) return true;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return true;
        if (SaveUIManager.Instance != null && SaveUIManager.Instance.IsOpen) return true;
        return false;
    }
}