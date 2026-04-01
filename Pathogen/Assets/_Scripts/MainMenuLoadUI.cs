using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// Simple load-only UI for the main menu
public class MainMenuLoadUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private MainMenuSlotEntry[] slots;
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [Header("References")]
    [SerializeField] private MainMenuUI mainMenuUI;

    void Start()
    {
        gameObject.SetActive(false);
        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    void OnEnable()
    {
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            SaveData meta = SaveManager.Instance?.ReadSlotMeta(i);
            slots[i].Configure(i, meta, OnSlotSelected);
        }
    }

    private void OnSlotSelected(int slot)
    {
        if (SaveManager.Instance == null) return;
        gameObject.SetActive(false);
        mainMenuUI.StartGameFromLoad(slot);
    }

    private void OnBack()
    {
        gameObject.SetActive(false);
        mainMenuUI.ShowMainMenu();
    }
}