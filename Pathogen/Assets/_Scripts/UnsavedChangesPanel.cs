using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Warning panel shown when the player tries to close audio/video settings with unsaved changes
public class UnsavedChangesPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private System.Action onSaveAndProceed;
    private System.Action onProceedWithout;

    void Awake()
    {
        // Hide the panel immediately when the game loads,
        if (panel != null) panel.SetActive(false);
    }

    void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(OnYes);
        if (noButton != null) noButton.onClick.AddListener(OnNo);
    }

    public void Open(System.Action onSaveAndProceed, System.Action onProceedWithout)
    {
        this.onSaveAndProceed = onSaveAndProceed;
        this.onProceedWithout = onProceedWithout;

        if (messageText != null)
            messageText.text = "You have unsaved changes.\nSave before leaving?";

        panel.SetActive(true);
    }

    private void OnYes()
    {
        panel.SetActive(false);
        onSaveAndProceed?.Invoke();
        onSaveAndProceed = null;
        onProceedWithout = null;
    }

    private void OnNo()
    {
        panel.SetActive(false);
        onProceedWithout?.Invoke();
        onSaveAndProceed = null;
        onProceedWithout = null;
    }
}