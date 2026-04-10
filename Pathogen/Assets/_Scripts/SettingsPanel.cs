using UnityEngine;
using UnityEngine.UI;

/// Controller for the Settings panel
public class SettingsPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button audioButton;
    [SerializeField] private Button videoButton;
    [SerializeField] private Button bindingsButton;
    [SerializeField] private Button backButton;
    [Header("Sub-panels")]
    [SerializeField] private GameObject audioSettingsPanel;
    [SerializeField] private GameObject videoSettingsPanel;
    [SerializeField] private GameObject bindingsSettingsPanel;  // placeholder for now
    [Header("Return target")]
    [Tooltip("Panel to show when Back is pressed — wire to MainMenuPanel or PauseMenuPanel.")]
    [SerializeField] private GameObject returnPanel;

    void Start()
    {
        if (audioButton != null) audioButton.onClick.AddListener(OnAudio);
        if (videoButton != null) videoButton.onClick.AddListener(OnVideo);
        if (bindingsButton != null) bindingsButton.onClick.AddListener(OnBindings);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    public void OnAudio()
    {
        gameObject.SetActive(false);
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(true);
    }

    public void OnVideo()
    {
        gameObject.SetActive(false);
        if (videoSettingsPanel != null) videoSettingsPanel.SetActive(true);
    }

    public void OnBindings()
    {
        HUDFeedback.Instance?.ShowInfo("Key bindings — coming soon.");
    }

    public void OnBack()
    {
        gameObject.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(true);
    }
}