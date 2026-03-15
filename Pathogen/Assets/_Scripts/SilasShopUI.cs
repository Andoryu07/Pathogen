using UnityEngine;
using UnityEngine.UI;

/// Silas shop UI — tab-style navigation.
/// Opens directly on the Item Shop tab
public class SilasShopUI : MonoBehaviour
{
    public static SilasShopUI Instance { get; private set; }

    [Header("Tab Buttons")]
    [SerializeField] private Button itemShopTabButton;
    [SerializeField] private Button weaponUpgradesTabButton;
    [SerializeField] private Button questsTabButton;
    [SerializeField] private Button closeButton;
    [Header("Panels")]
    [SerializeField] private GameObject itemShopPanel;
    [SerializeField] private GameObject weaponUpgradesPanel;
    [SerializeField] private GameObject questsPanel;

    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (itemShopTabButton != null) itemShopTabButton.onClick.AddListener(ShowItemShop);
        if (weaponUpgradesTabButton != null) weaponUpgradesTabButton.onClick.AddListener(ShowWeaponUpgrades);
        if (questsTabButton != null) questsTabButton.onClick.AddListener(ShowQuests);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        ShowItemShop();   // always open on item shop first
        WeaponHUD.Instance?.Hide();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        WeaponHUD.Instance?.Show();
        WeaponHUD.Instance?.RefreshAmmoText();
    }

    private void ShowItemShop()
    {
        itemShopPanel?.SetActive(true);
        weaponUpgradesPanel?.SetActive(false);
        questsPanel?.SetActive(false);
    }

    private void ShowWeaponUpgrades()
    {
        itemShopPanel?.SetActive(false);
        weaponUpgradesPanel?.SetActive(true);
        questsPanel?.SetActive(false);
    }

    private void ShowQuests()
    {
        itemShopPanel?.SetActive(false);
        weaponUpgradesPanel?.SetActive(false);
        questsPanel?.SetActive(true);
    }
}