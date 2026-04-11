using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            OnBack();
    }

    private void OnAudio()
    {
        if (audioSettingsPanel != null)
            StartCoroutine(ShowPanel(audioSettingsPanel));
        else Debug.LogWarning("[SettingsPanel] audioSettingsPanel not wired!");
    }

    private void OnVideo()
    {
        if (videoSettingsPanel != null)
            StartCoroutine(ShowPanel(videoSettingsPanel));
        else Debug.LogWarning("[SettingsPanel] videoSettingsPanel not wired!");
    }

    private IEnumerator ShowPanel(GameObject target)
    {
        target.SetActive(true);
        Canvas.ForceUpdateCanvases();
        yield return null;  // one frame for layout rebuild
        gameObject.SetActive(false);
    }

    private void OnBindings()
    {
        HUDFeedback.Instance?.ShowInfo("Key bindings — coming soon.");
    }

    private void OnBack()
    {
        gameObject.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(true);
    }
}