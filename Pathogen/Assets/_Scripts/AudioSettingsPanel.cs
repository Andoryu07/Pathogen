using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
/// Audio settings panel — Music and SFX volume sliders
/// Detects unsaved changes and warns before closing
public class AudioSettingsPanel : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button backButton;
    [Header("References")]
    [SerializeField] private GameObject settingsPanel;        // parent settings panel to return to
    [SerializeField] private UnsavedChangesPanel unsavedPanel;
 
    private float savedMusicVolume;
    private float savedSFXVolume;
    private bool hasUnsavedChanges = false;
    // PlayerPrefs keys
    private const string KeyMusic = "Settings_MusicVolume";
    private const string KeySFX = "Settings_SFXVolume";

    void Start()
    {
        if (saveButton != null) saveButton.onClick.AddListener(OnSave);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
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
        savedMusicVolume = PlayerPrefs.GetFloat(KeyMusic, 1f);
        savedSFXVolume = PlayerPrefs.GetFloat(KeySFX, 1f);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(savedMusicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(savedSFXVolume);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(savedMusicVolume);
            AudioManager.Instance.SetSFXVolume(savedSFXVolume);
        }

        UpdateValueTexts();
    }

    private void OnMusicChanged(float value)
    {
        UpdateValueTexts();
        // Apply live preview
        AudioManager.Instance?.SetMusicVolume(value);
        MarkDirty();
    }

    private void OnSFXChanged(float value)
    {
        UpdateValueTexts();
        AudioManager.Instance?.SetSFXVolume(value);
        MarkDirty();
    }

    private void UpdateValueTexts()
    {
        if (musicValueText != null && musicSlider != null)
            musicValueText.text = Mathf.RoundToInt(musicSlider.value * 100f) + "%";
        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
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

    private void OnSave()
    {
        SaveSettings();
    }

    public void SaveSettings()
    {
        if (musicSlider != null)
        {
            savedMusicVolume = musicSlider.value;
            PlayerPrefs.SetFloat(KeyMusic, savedMusicVolume);
            AudioManager.Instance?.SetMusicVolume(savedMusicVolume);
        }
        if (sfxSlider != null)
        {
            savedSFXVolume = sfxSlider.value;
            PlayerPrefs.SetFloat(KeySFX, savedSFXVolume);
            AudioManager.Instance?.SetSFXVolume(savedSFXVolume);
        }
        PlayerPrefs.Save();
        hasUnsavedChanges = false;
        RefreshSaveButton();
        HUDFeedback.Instance?.ShowInfo("Audio settings saved.");
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
        else
        {
            GoBack();
        }
    }

    private void GoBack()
    {
        if (hasUnsavedChanges)
        {
            AudioManager.Instance?.SetMusicVolume(savedMusicVolume);
            AudioManager.Instance?.SetSFXVolume(savedSFXVolume);
        }
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