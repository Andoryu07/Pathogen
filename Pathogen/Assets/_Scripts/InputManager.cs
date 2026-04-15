using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] private BindingData bindingData;
    private Dictionary<string, KeyCode> currentBindings = new Dictionary<string, KeyCode>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        LoadBindings();
    }

    public void LoadBindings()
    {
        foreach (var binding in bindingData.defaultBindings)
        {
            string savedKey = PlayerPrefs.GetString("Key_" + binding.actionName, binding.defaultKey.ToString());
            KeyCode code = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedKey);
            currentBindings[binding.actionName] = code;
        }
    }

    public KeyCode GetKeyForAction(string actionName)
    {
        return currentBindings.ContainsKey(actionName) ? currentBindings[actionName] : KeyCode.None;
    }

    // This is what you'll use in your PlayerController
    public bool GetKeyDown(string actionName) => Input.GetKeyDown(currentBindings[actionName]);
    public bool GetKey(string actionName) => Input.GetKey(currentBindings[actionName]);

    public void UpdateBinding(string actionName, KeyCode newKey)
    {
        currentBindings[actionName] = newKey;
        PlayerPrefs.SetString("Key_" + actionName, newKey.ToString());
        PlayerPrefs.Save();
    }

    public Dictionary<string, KeyCode> GetAllBindings() => currentBindings;
}