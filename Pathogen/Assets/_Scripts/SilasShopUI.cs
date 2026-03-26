using UnityEngine;
using UnityEngine.UI;
/// Silas shop UI — tab-style navigation
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
        TimeScaleManager.Freeze(this);
        ShowItemShop();
        WeaponHUD.Instance?.Hide();
    }

    ///Open directly on a specific tab. 0=ItemShop, 1=WeaponUpgrades, 2=Quests
    public void OpenOnTab(int tab)
    {
        gameObject.SetActive(true);
        WeaponHUD.Instance?.Hide();
        switch (tab)
        {
            case 1: ShowWeaponUpgrades(); break;
            case 2: ShowQuests(); break;
            default: ShowItemShop(); break;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        TimeScaleManager.Unfreeze(this);
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