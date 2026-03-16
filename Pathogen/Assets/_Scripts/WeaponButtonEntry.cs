using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Left-side weapon list button. All visuals wired in Inspector
public class WeaponButtonEntry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private Image backgroundImage;
    [Header("Colors")]
    [SerializeField] private Color colNormal = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    [SerializeField] private Color colSelected = new Color(0.25f, 0.55f, 0.90f, 0.85f);

    private Button button;

    public void Configure(WeaponUpgradeData data, System.Action<WeaponUpgradeData> onSelected)
    {
        button = GetComponent<Button>();

        if (weaponIcon != null)
        {
            weaponIcon.gameObject.SetActive(data.weaponIcon != null);
            if (data.weaponIcon != null) weaponIcon.sprite = data.weaponIcon;
        }

        if (weaponName != null) weaponName.text = data.weaponName;

        if (button != null)
        {
            WeaponUpgradeData cap = data;
            button.onClick.AddListener(() => onSelected?.Invoke(cap));
        }
    }
    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? colSelected : colNormal;
    }
}