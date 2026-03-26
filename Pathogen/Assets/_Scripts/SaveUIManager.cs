using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// Save/Load UI panel
public class SaveUIManager : MonoBehaviour
{
    public static SaveUIManager Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject saveUIPanel;
    [Header("Slots (one per save slot)")]
    [SerializeField] private SaveSlotUI[] slots;
    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [Header("Mode")]
    [SerializeField] private bool loadMode = false;
    [SerializeField] private Button toggleModeButton;
    [SerializeField] private TextMeshProUGUI toggleModeText;
    [SerializeField] private TextMeshProUGUI titleText;

    public bool IsOpen => saveUIPanel != null && saveUIPanel.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (toggleModeButton != null) toggleModeButton.onClick.AddListener(ToggleMode);
        saveUIPanel.SetActive(false);
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Open(bool isLoadMode = false)
    {
        loadMode = isLoadMode;
        saveUIPanel.SetActive(true);
        TimeScaleManager.Freeze(this);
        WeaponHUD.Instance?.Hide();
        RefreshSlots();
    }

    public void Close()
    {
        saveUIPanel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        WeaponHUD.Instance?.Show();
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    public void ToggleMode()
    {
        loadMode = !loadMode;
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (slots == null) return;

        if (titleText != null) titleText.text = loadMode ? "Load Game" : "Save Game";
        if (toggleModeText != null) toggleModeText.text = loadMode ? "Switch to Save" : "Switch to Load";

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            SaveData meta = SaveManager.Instance?.ReadSlotMeta(i);
            slots[i].Configure(i, meta, loadMode, OnSlotAction);
        }
    }

    private void OnSlotAction(int slot, bool isLoad)
    {
        if (isLoad)
            SaveManager.Instance?.Load(slot);
        else
            SaveManager.Instance?.Save(slot);

        RefreshSlots();
        if (isLoad) Close();
    }
}