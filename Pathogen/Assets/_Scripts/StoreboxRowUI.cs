using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// Component on the StoreboxRow prefab
public class StoreboxRowUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;

    public void Populate(Sprite icon, string label)
    {
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(icon != null);
            if (icon != null) iconImage.sprite = icon;
        }
        if (labelText != null)
            labelText.text = label;
    }
}