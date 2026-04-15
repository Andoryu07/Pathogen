using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class KeybindSettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private GameObject overlayWaitPanel;
    [SerializeField] private Button backButton;     
    [SerializeField] private GameObject settingsPanel;

    private string actionToRebind = null;
    private Dictionary<string, KeybindEntryUI> uiEntries = new Dictionary<string, KeybindEntryUI>();

    void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        foreach (var item in InputManager.Instance.GetAllBindings())
        {
            GameObject go = Instantiate(entryPrefab, container);
            KeybindEntryUI ui = go.GetComponent<KeybindEntryUI>();
            ui.Setup(item.Key, item.Value, StartRebinding);
            uiEntries.Add(item.Key, ui);
        }
        overlayWaitPanel.SetActive(false);
    }

    private void OnBack()
    {
        GoBack();
    }

    private void GoBack()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void StartRebinding(string actionName)
    {
        actionToRebind = actionName;
        overlayWaitPanel.SetActive(true);
        StartCoroutine(WaitForKeyPress());
    }

    private IEnumerator WaitForKeyPress()
    {
        yield return null; // Wait one frame to avoid registering the mouse click
        while (actionToRebind != null)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        TryBind(actionToRebind, kcode);
                        actionToRebind = null;
                        overlayWaitPanel.SetActive(false);
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    private void TryBind(string action, KeyCode key)
    {
        // Conflict Check
        foreach (var pair in InputManager.Instance.GetAllBindings())
        {
            if (pair.Value == key && pair.Key != action)
            {
                Debug.LogWarning("Key " + key + " is already bound to " + pair.Key);
                return;
            }
        }

        InputManager.Instance.UpdateBinding(action, key);
        uiEntries[action].UpdateDisplay(key);
    }
}