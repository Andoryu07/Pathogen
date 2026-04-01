using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// One save slot button in the main menu load screen
public class MainMenuSlotEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI sceneText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private Button loadButton;
    [SerializeField] private TextMeshProUGUI loadButtonText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color colOccupied = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    [SerializeField] private Color colEmpty = new Color(0.12f, 0.12f, 0.12f, 0.9f);

    public void Configure(int slot, SaveData data, Action<int> onLoad)
    {
        if (loadButton != null) loadButton.onClick.RemoveAllListeners();

        bool hasData = data != null;

        if (backgroundImage != null)
            backgroundImage.color = hasData ? colOccupied : colEmpty;

        if (hasData)
        {
            if (slotNameText != null) slotNameText.text = data.saveName;
            if (sceneText != null) sceneText.text = data.sceneName;
            if (timestampText != null) timestampText.text = data.timestamp;
            if (playtimeText != null) playtimeText.text = FormatPlaytime(data.totalPlaytime);
            if (loadButtonText != null) loadButtonText.text = "Load";
            if (loadButton != null)
            {
                loadButton.interactable = true;
                loadButton.onClick.AddListener(() => onLoad?.Invoke(slot));
            }
        }
        else
        {
            if (slotNameText != null) slotNameText.text = "— Empty —";
            if (sceneText != null) sceneText.text = "";
            if (timestampText != null) timestampText.text = "";
            if (playtimeText != null) playtimeText.text = "";
            if (loadButtonText != null) loadButtonText.text = "—";
            if (loadButton != null) loadButton.interactable = false;
        }
    }

    private static string FormatPlaytime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        return $"Playtime: {h}h {m:D2}m";
    }
}