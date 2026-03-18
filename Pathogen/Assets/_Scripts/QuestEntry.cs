using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// One quest row in the quest panel
public class QuestEntry : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [Header("Accept Interaction")]
    [SerializeField] private Image acceptProgressBar; 
    [SerializeField] private TextMeshProUGUI acceptLabel;      
    [SerializeField] private float holdDuration = 1f;
    [Header("Claim Button")]
    [SerializeField] private Button claimButton;
    [Header("Donate Button (DonateItem quests only)")]
    [SerializeField] private Button donateButton;
    [SerializeField] private TextMeshProUGUI donateButtonText;
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [Header("Colors")]
    [SerializeField] private Color bgAvailable = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    [SerializeField] private Color bgActive = new Color(0.15f, 0.20f, 0.28f, 0.9f); // blue tint
    [SerializeField] private Color bgCompleted = new Color(0.15f, 0.28f, 0.15f, 0.9f); // green tint
    [SerializeField] private Color bgHover = new Color(0.28f, 0.28f, 0.28f, 0.9f);
    [SerializeField] private Color colProgress = new Color(0.30f, 0.90f, 0.40f, 1f);
    [SerializeField] private Color colReward = new Color(0.85f, 0.80f, 0.25f, 1f);
    [SerializeField] private Color colAcceptBar = new Color(0.25f, 0.85f, 0.35f, 1f);

    private QuestData data;
    private QuestState currentState;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private System.Action onClaimedCallback;

    public void Configure(QuestData questData, System.Action onClaimed)
    {
        data = questData;
        onClaimedCallback = onClaimed;

        if (claimButton != null) claimButton.onClick.AddListener(TryClaim);
        if (donateButton != null) donateButton.onClick.AddListener(TryDonate);

        Refresh();
    }

    public void Refresh()
    {
        if (data == null) return;
        currentState = QuestManager.Instance != null
            ? QuestManager.Instance.GetState(data)
            : QuestState.Available;

        int current = QuestManager.Instance?.GetProgress(data) ?? 0;

        // Name and description
        if (nameText != null) nameText.text = data.questName;
        if (descText != null) descText.text = data.description;

        // Reward text
        if (rewardText != null)
        {
            string itemNames = "";
            if (data.itemRewards != null)
            {
                foreach (var prefab in data.itemRewards)
                {
                    if (prefab == null) continue;
                    Item ic = prefab.GetComponent<Item>();
                    string n = ic != null ? ic.GetItemName() : prefab.name;
                    itemNames += string.IsNullOrEmpty(itemNames) ? n : $", {n}";
                }
            }
            string pathStr = data.patheosReward > 0 ? $"{data.patheosReward} ◈" : "";
            string sep = !string.IsNullOrEmpty(pathStr) && !string.IsNullOrEmpty(itemNames) ? " + " : "";
            rewardText.text = $"Reward: {pathStr}{sep}{itemNames}";
            rewardText.color = colReward;
        }

        switch (currentState)
        {
            case QuestState.Available:
                SetAvailableState();
                break;
            case QuestState.Active:
                SetActiveState(current);
                break;
            case QuestState.Completed:
                SetCompletedState(current);
                break;
        }
    }

    private void SetAvailableState()
    {
        if (backgroundImage != null) backgroundImage.color = bgAvailable;
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (acceptLabel != null) { acceptLabel.gameObject.SetActive(true); acceptLabel.text = "Hold to Accept"; }
        if (acceptProgressBar != null) acceptProgressBar.gameObject.SetActive(false);
        if (claimButton != null) claimButton.gameObject.SetActive(false);
        if (donateButton != null) donateButton.gameObject.SetActive(false);
    }

    private void SetActiveState(int current)
    {
        if (backgroundImage != null) backgroundImage.color = bgActive;
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = $"Progress: {current} / {data.requiredCount}";
            progressText.color = colProgress;
        }
        if (acceptLabel != null) acceptLabel.gameObject.SetActive(false);
        if (acceptProgressBar != null) acceptProgressBar.gameObject.SetActive(false);
        if (claimButton != null) claimButton.gameObject.SetActive(false);

        // Show donate button only for DonateItem quests
        bool isDonate = data.questType == QuestType.DonateItem;
        if (donateButton != null) donateButton.gameObject.SetActive(isDonate);
        if (donateButtonText != null && isDonate)
            donateButtonText.text = InventoryGrid.Instance.HasItem(data.targetName)
                ? $"Donate {data.targetName}"
                : $"Need: {data.targetName}";
        if (donateButton != null && isDonate)
            donateButton.interactable = InventoryGrid.Instance.HasItem(data.targetName);
    }

    private void SetCompletedState(int current)
    {
        if (backgroundImage != null) backgroundImage.color = bgCompleted;
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = $"Complete! {current} / {data.requiredCount}";
            progressText.color = colProgress;
        }
        if (acceptLabel != null) acceptLabel.gameObject.SetActive(false);
        if (acceptProgressBar != null) acceptProgressBar.gameObject.SetActive(false);
        if (claimButton != null) claimButton.gameObject.SetActive(true);
        if (donateButton != null) donateButton.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (currentState != QuestState.Available) return;
        isHolding = true;
        holdTimer = 0f;
        if (acceptProgressBar != null)
        {
            acceptProgressBar.color = colAcceptBar;
            acceptProgressBar.fillAmount = 0f;
            acceptProgressBar.gameObject.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData) => CancelHold();
    public void OnPointerExit(PointerEventData eventData) => CancelHold();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == QuestState.Available && backgroundImage != null)
            backgroundImage.color = bgHover;
    }

    private void CancelHold()
    {
        isHolding = false;
        holdTimer = 0f;
        if (acceptProgressBar != null)
        {
            acceptProgressBar.fillAmount = 0f;
            acceptProgressBar.gameObject.SetActive(false);
        }
        // Reset bg to correct state colour
        if (backgroundImage != null)
            backgroundImage.color = currentState == QuestState.Available ? bgAvailable :
                                    currentState == QuestState.Active ? bgActive : bgCompleted;
    }

    void Update()
    {
        if (!isHolding) return;
        holdTimer += Time.deltaTime;
        if (acceptProgressBar != null)
            acceptProgressBar.fillAmount = Mathf.Clamp01(holdTimer / holdDuration);
        if (holdTimer >= holdDuration)
        {
            isHolding = false;
            QuestManager.Instance?.AcceptQuest(data);
            Refresh();
        }
    }

    private void TryDonate()
    {
        if (data == null || data.questType != QuestType.DonateItem) return;

        Item item = InventoryGrid.Instance.GetItem(data.targetName);
        if (item == null)
        {
            HUDFeedback.Instance?.ShowWarning($"You don't have {data.targetName}.");
            return;
        }

        InventoryGrid.Instance.RemoveItem(item);
        QuestManager.Instance?.ReportItemDonated(data.targetName);
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        Refresh();
    }

    private void TryClaim()
    {
        bool claimed = QuestManager.Instance != null &&
                       QuestManager.Instance.ClaimRewards(data);
        if (claimed)
            onClaimedCallback?.Invoke();
    }
}