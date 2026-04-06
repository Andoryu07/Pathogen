using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// Confirmation panel for key item locks
public class KeyItemConfirmPanel : MonoBehaviour
{
    public static KeyItemConfirmPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button closeButton;
    [Header("Colors")]
    [SerializeField] private Color colHasItem = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color colMissingItem = new Color(0.9f, 0.3f, 0.3f, 1f);

    private System.Action onConfirm;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        panel.SetActive(false);
        if (yesButton != null) yesButton.onClick.AddListener(OnYes);
        if (closeButton != null) closeButton.onClick.AddListener(OnClose);
    }

    void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            OnClose();
    }
    /// Opens the confirmation panel
    /// onConfirm is called only if the player clicks Yes
    public void Open(string requiredItemName, bool playerHasItem, bool consumesItem, System.Action onConfirm)
    {
        this.onConfirm = onConfirm;

        if (messageText != null)
            messageText.text = "This action requires: " + requiredItemName;

        if (statusText != null)
        {
            string ownStatus = playerHasItem ? "You have this item \nUse item and proceed?" : "You don't have this item.";
            string consumeStatus = consumesItem ? "This action will consume the item" : "This action will NOT consume the item";
            statusText.text = ownStatus + "\n" + consumeStatus;
            statusText.color = playerHasItem ? colHasItem : colMissingItem;
        }

        if (yesButton != null)
            yesButton.gameObject.SetActive(playerHasItem);

        panel.SetActive(true);
        TimeScaleManager.Freeze(this);
    }

    private void OnYes()
    {
        var callback = onConfirm; 
        Close();
        callback?.Invoke();
    }

    private void OnClose()
    {
        Close();
    }

    private void Close()
    {
        panel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        onConfirm = null;
    }
}