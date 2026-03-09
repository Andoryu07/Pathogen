using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Small HUD widget that shows the currently equipped weapon
public class WeaponHUD : MonoBehaviour
{
    public static WeaponHUD Instance { get; private set; }
    [Header("References")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponName;
    [Header("Empty State")]
    [SerializeField] private string emptyLabel = "No weapon equipped";
    [SerializeField] private bool hideWhenEmpty = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        Refresh(null);
    }

    ///Needs to be called whenever equipped weapon changes
    public void Refresh(Item weapon)
    {
        if (weapon == null)
        {
            if (hideWhenEmpty) { hudPanel.SetActive(false); return; }
            hudPanel.SetActive(true);
            if (weaponIcon != null) weaponIcon.enabled = false;
            if (weaponName != null) weaponName.text = emptyLabel;
            return;
        }
        hudPanel.SetActive(true);
        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon.GetIcon();
            weaponIcon.enabled = weapon.GetIcon() != null;
        }
        if (weaponName != null)
            weaponName.text = weapon.GetItemName();
    }
}