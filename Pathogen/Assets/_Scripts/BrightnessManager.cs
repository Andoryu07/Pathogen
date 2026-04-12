using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrightnessManager : MonoBehaviour
{
    public static BrightnessManager Instance { get; private set; }
    [SerializeField] private UnityEngine.UI.Image overlay;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        float saved = PlayerPrefs.GetFloat("Settings_Brightness", 0f);
        if (overlay != null)
        {
            var c = overlay.color;
            overlay.color = new Color(c.r, c.g, c.b, saved);
        }
    }

    public void SetBrightness(float value)
    {
        if (overlay == null) return;
        var c = overlay.color;
        overlay.color = new Color(c.r, c.g, c.b, value);
    }
}