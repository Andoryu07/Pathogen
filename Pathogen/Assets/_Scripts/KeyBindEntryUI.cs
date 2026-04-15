using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeybindEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private Button changeButton;

    private string actionName;
    private System.Action<string> onBindRequest;

    public void Setup(string name, KeyCode currentKey, System.Action<string> callback)
    {
        actionName = name;
        actionText.text = name;
        keyText.text = currentKey.ToString();
        onBindRequest = callback;
        changeButton.onClick.AddListener(() => onBindRequest?.Invoke(actionName));
    }

    public void UpdateDisplay(KeyCode newKey)
    {
        keyText.text = newKey.ToString();
    }
}