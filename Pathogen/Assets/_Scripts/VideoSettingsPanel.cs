using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
/// Video settings panel — display mode and max framerate
public class VideoSettingsPanel : MonoBehaviour
{
    [Header("Display Mode")]
    [SerializeField] private Button windowedButton;
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Color selectedColor = new Color(0.25f, 0.55f, 0.9f, 1f);
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [Header("Framerate")]
    [SerializeField] private Slider framerateSlider;
    [SerializeField] private TextMeshProUGUI framerateValueText;
    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button backButton;
    [Header("References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private UnsavedChangesPanel unsavedPanel;

    private bool savedFullscreen;
    private int savedFramerate;
    private bool currentFullscreen;
    private int currentFramerate;
    private bool hasUnsavedChanges = false;
    private const string KeyFullscreen = "Settings_Fullscreen";
    private const string KeyFramerate = "Settings_Framerate";
    private const int MinFPS = 30;
    private const int MaxFPS = 240;

    void Start()
    {

        if (framerateSlider != null)
        {
            framerateSlider.minValue = MinFPS;
            framerateSlider.maxValue = MaxFPS;
            framerateSlider.wholeNumbers = true;
            framerateSlider.onValueChanged.AddListener(OnFramerateChanged);
        }
        if (windowedButton != null) windowedButton.onClick.AddListener(OnWindowed);
        if (fullscreenButton != null) fullscreenButton.onClick.AddListener(OnFullscreen);
        if (saveButton != null) saveButton.onClick.AddListener(OnSave);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    void OnEnable()
    {
        LoadCurrentSettings();
        hasUnsavedChanges = false;
        RefreshSaveButton();
        Canvas.ForceUpdateCanvases();
    }

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            OnBack();
    }

    private void LoadCurrentSettings()
    {
        savedFullscreen = PlayerPrefs.GetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0) == 1;
        savedFramerate = PlayerPrefs.GetInt(KeyFramerate, Application.targetFrameRate > 0
                                                              ? Application.targetFrameRate : 60);
        savedFramerate = Mathf.Clamp(savedFramerate, MinFPS, MaxFPS);

        currentFullscreen = savedFullscreen;
        currentFramerate = savedFramerate;

        if (framerateSlider != null)
            framerateSlider.SetValueWithoutNotify(currentFramerate);

        UpdateDisplayButtons();
        UpdateFramerateText();
    }

    private void OnWindowed()
    {
        currentFullscreen = false;
        UpdateDisplayButtons();
        MarkDirty();
    }

    private void OnFullscreen()
    {
        currentFullscreen = true;
        UpdateDisplayButtons();
        MarkDirty();
    }

    private void OnFramerateChanged(float value)
    {
        currentFramerate = Mathf.RoundToInt(value);
        UpdateFramerateText();
        MarkDirty();
    }

    private void UpdateDisplayButtons()
    {
        SetButtonColor(windowedButton, !currentFullscreen);
        SetButtonColor(fullscreenButton, currentFullscreen);
    }

    private void SetButtonColor(Button btn, bool selected)
    {
        if (btn == null) return;
        var img = btn.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.color = selected ? selectedColor : normalColor;
    }

    private void UpdateFramerateText()
    {
        if (framerateValueText != null)
            framerateValueText.text = currentFramerate + " FPS";
    }

    private void MarkDirty()
    {
        hasUnsavedChanges = true;
        RefreshSaveButton();
    }

    private void RefreshSaveButton()
    {
        if (saveButton != null)
            saveButton.interactable = hasUnsavedChanges;
    }
    private void OnSave() => SaveSettings();

    public void SaveSettings()
    {
        savedFullscreen = currentFullscreen;
        savedFramerate = currentFramerate;
        Screen.fullScreen = savedFullscreen;
        Application.targetFrameRate = savedFramerate;
        PlayerPrefs.SetInt(KeyFullscreen, savedFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KeyFramerate, savedFramerate);
        PlayerPrefs.Save();
        hasUnsavedChanges = false;
        RefreshSaveButton();
        HUDFeedback.Instance?.ShowInfo("Video settings saved.");
    }

    private void OnBack()
    {
        if (hasUnsavedChanges && unsavedPanel != null)
        {
            unsavedPanel.Open(
                onSaveAndProceed: () => { SaveSettings(); GoBack(); },
                onProceedWithout: () => { GoBack(); }
            );
        }
        else GoBack();
    }

    private void GoBack()
    {
        StartCoroutine(ShowSettings());
    }

    private IEnumerator ShowSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        yield return null;
        gameObject.SetActive(false);
    }
}