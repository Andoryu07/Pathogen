using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// One save slot entry in the Save/Load UI
public class SaveSlotUI : MonoBehaviour
{
    [Header("Info Text")]
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI sceneText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [Header("Buttons")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button deleteButton;
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [Header("Colors")]
    [SerializeField] private Color colOccupied = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    [SerializeField] private Color colEmpty = new Color(0.12f, 0.12f, 0.12f, 0.9f);

    private int slotIndex;
    private bool hasData;
    private System.Action<int, bool> onAction;  // (slot, isLoad)

    public void Configure(int slot, SaveData data, bool isLoadMode,
                          System.Action<int, bool> callback)
    {
        slotIndex = slot;
        hasData = data != null;
        onAction = callback;
        // Clear old listeners
        if (actionButton != null) actionButton.onClick.RemoveAllListeners();
        if (deleteButton != null) deleteButton.onClick.RemoveAllListeners();
        if (hasData)
        {
            // Slot has save data
            if (backgroundImage != null) backgroundImage.color = colOccupied;
            if (slotNameText != null) slotNameText.text = data.saveName;
            if (sceneText != null) sceneText.text = data.sceneName;
            if (timestampText != null) timestampText.text = data.timestamp;
            if (playtimeText != null) playtimeText.text = FormatPlaytime(data.totalPlaytime);
            if (actionButtonText != null) actionButtonText.text = isLoadMode ? "Load" : "Save";
            if (actionButton != null)
            {
                actionButton.interactable = true;
                bool load = isLoadMode;
                actionButton.onClick.AddListener(() => onAction?.Invoke(slotIndex, load));
            }
            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(true);
                deleteButton.onClick.AddListener(OnDeleteClicked);
            }
        }
        else
        {
            // Empty slot
            if (backgroundImage != null) backgroundImage.color = colEmpty;
            if (slotNameText != null) slotNameText.text = "— Empty —";
            if (sceneText != null) sceneText.text = "";
            if (timestampText != null) timestampText.text = "";
            if (playtimeText != null) playtimeText.text = "";
            if (actionButtonText != null) actionButtonText.text = isLoadMode ? "—" : "Save";
            if (actionButton != null)
            {
                actionButton.interactable = !isLoadMode;
                if (!isLoadMode)
                {
                    bool load = false;
                    actionButton.onClick.AddListener(() => onAction?.Invoke(slotIndex, load));
                }
            }
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        }
    }

    private void OnDeleteClicked()
    {
        SaveManager.Instance?.DeleteSlot(slotIndex);
        // Refresh the parent UI
        SaveUIManager.Instance?.GetComponent<SaveUIManager>()?.Open();
    }

    private static string FormatPlaytime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        return $"Playtime: {h}h {m:D2}m";
    }
}