using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
/// Video settings panel — display mode and max framerate
public class VideoSettingsPanel : MonoBehaviour
{
    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [Header("Display Mode")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [Header("VSync")]
    [SerializeField] private Toggle vsyncToggle;
    [Header("Brightness")]
    [Tooltip("Full-screen black overlay Image — lower alpha = brighter")]
    [SerializeField] private UnityEngine.UI.Image brightnessOverlay;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TextMeshProUGUI brightnessValueText;
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
    private int savedResolutionIndex;
    private int savedDisplayMode;       // 0=Windowed, 1=Fullscreen, 2=Borderless
    private bool savedVSync;
    private float savedBrightness;
    private int currentResolutionIndex;
    private int currentDisplayMode;
    private bool currentVSync;
    private float currentBrightness;
    private Resolution[] availableResolutions;
    private bool currentFullscreen;
    private int currentFramerate;
    private bool hasUnsavedChanges = false;
    private const string KeyFullscreen = "Settings_Fullscreen";
    private const string KeyFramerate = "Settings_Framerate";
    private const string KeyResolution = "Settings_Resolution";
    private const string KeyDisplayMode = "Settings_DisplayMode";
    private const string KeyVSync = "Settings_VSync";
    private const string KeyBrightness = "Settings_Brightness";
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
        if (saveButton != null) saveButton.onClick.AddListener(OnSave);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        if (resolutionDropdown != null)
        {
            availableResolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            foreach (var r in availableResolutions)
                options.Add(r.width + " x " + r.height);
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        // Display mode dropdown
        if (displayModeDropdown != null)
        {
            displayModeDropdown.ClearOptions();
            displayModeDropdown.AddOptions(new System.Collections.Generic.List<string>
        { "Windowed", "Fullscreen", "Borderless Window" });
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        }
        // VSync
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        // Brightness
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }
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
        savedFramerate = PlayerPrefs.GetInt(KeyFramerate, Application.targetFrameRate > 0 ? Application.targetFrameRate : 60);
        savedFramerate = Mathf.Clamp(savedFramerate, MinFPS, MaxFPS);
        currentFullscreen = savedFullscreen;
        currentFramerate = savedFramerate;
        if (framerateSlider != null) framerateSlider.SetValueWithoutNotify(currentFramerate);
        UpdateFramerateText();
        savedResolutionIndex = PlayerPrefs.GetInt(KeyResolution, FindCurrentResolutionIndex());
        currentResolutionIndex = savedResolutionIndex;
        if (resolutionDropdown != null) resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        savedDisplayMode = PlayerPrefs.GetInt(KeyDisplayMode, Screen.fullScreen ? 1 : 0);
        currentDisplayMode = savedDisplayMode;
        if (displayModeDropdown != null) displayModeDropdown.SetValueWithoutNotify(currentDisplayMode);
        savedVSync = PlayerPrefs.GetInt(KeyVSync, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        currentVSync = savedVSync;
        if (vsyncToggle != null) vsyncToggle.SetIsOnWithoutNotify(currentVSync);
        savedBrightness = PlayerPrefs.GetFloat(KeyBrightness, 0f);
        currentBrightness = savedBrightness;
        if (brightnessSlider != null) brightnessSlider.SetValueWithoutNotify(currentBrightness);
        ApplyBrightness(currentBrightness);
        UpdateBrightnessText();
    }

    private void OnFramerateChanged(float value)
    {
        currentFramerate = Mathf.RoundToInt(value);
        UpdateFramerateText();
        MarkDirty();
    }
    private void OnResolutionChanged(int index)
    {
        currentResolutionIndex = index;
        MarkDirty();
    }

    private void OnDisplayModeChanged(int index)
    {
        currentDisplayMode = index;
        MarkDirty();
    }

    private void OnVSyncChanged(bool value)
    {
        currentVSync = value;
        QualitySettings.vSyncCount = value ? 1 : 0;  // live preview
        MarkDirty();
    }

    private void OnBrightnessChanged(float value)
    {
        currentBrightness = value;
        ApplyBrightness(value);
        UpdateBrightnessText();
        MarkDirty();
    }

    private void ApplyBrightness(float value)
    {
        BrightnessManager.Instance?.SetBrightness(value);
        // Keep local overlay as fallback
        if (brightnessOverlay != null)
        {
            var c = brightnessOverlay.color;
            brightnessOverlay.color = new Color(c.r, c.g, c.b, value);
        }
    }

    private void UpdateBrightnessText()
    {
        if (brightnessValueText != null && brightnessSlider != null)
            brightnessValueText.text = Mathf.RoundToInt((1f - brightnessSlider.value) * 100f) + "%";
    }

    private int FindCurrentResolutionIndex()
    {
        if (availableResolutions == null) return 0;
        for (int i = 0; i < availableResolutions.Length; i++)
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
                return i;
        return 0;
    }

    private static FullScreenMode GetFullscreenMode(int index)
    {
        switch (index)
        {
            case 1: return FullScreenMode.ExclusiveFullScreen;
            case 2: return FullScreenMode.FullScreenWindow;
            default: return FullScreenMode.Windowed;
        }
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
        // Resolution
        savedResolutionIndex = currentResolutionIndex;
        if (availableResolutions != null && currentResolutionIndex < availableResolutions.Length)
        {
            Resolution r = availableResolutions[currentResolutionIndex];
            Screen.SetResolution(r.width, r.height, GetFullscreenMode(currentDisplayMode));
        }
        PlayerPrefs.SetInt(KeyResolution, savedResolutionIndex);
        // Display mode
        savedDisplayMode = currentDisplayMode;
        Screen.fullScreenMode = GetFullscreenMode(savedDisplayMode);
        PlayerPrefs.SetInt(KeyDisplayMode, savedDisplayMode);
        // VSync
        savedVSync = currentVSync;
        QualitySettings.vSyncCount = savedVSync ? 1 : 0;
        PlayerPrefs.SetInt(KeyVSync, savedVSync ? 1 : 0);
        // Brightness
        savedBrightness = currentBrightness;
        PlayerPrefs.SetFloat(KeyBrightness, savedBrightness);
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