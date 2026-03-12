using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// Keypad panel for code-locked puzzles
public class CodePadUI : MonoBehaviour
{
    public static CodePadUI Instance { get; private set; }
    public bool IsOpen => codePadPanel != null && codePadPanel.activeSelf;
    [Header("Panel")]
    [SerializeField] private GameObject codePadPanel;
    [Header("Appearance")]
    [SerializeField] private int maxCodeLength = 6;
    [SerializeField] private Color bgColor = new Color(0.10f, 0.10f, 0.12f, 0.97f);
    [SerializeField] private Color displayBgColor = new Color(0.06f, 0.06f, 0.08f, 1f);
    [SerializeField] private Color keyBgColor = new Color(0.20f, 0.20f, 0.23f, 1f);
    [SerializeField] private Color keyHoverColor = new Color(0.30f, 0.30f, 0.35f, 1f);
    [SerializeField] private Color enterColor = new Color(0.15f, 0.55f, 0.20f, 1f);
    [SerializeField] private Color backColor = new Color(0.55f, 0.20f, 0.15f, 1f);
    [SerializeField] private Color textNormal = new Color(0.90f, 0.90f, 0.90f, 1f);
    [SerializeField] private Color textCorrect = new Color(0.20f, 0.90f, 0.30f, 1f);
    [SerializeField] private Color textWrong = new Color(0.90f, 0.20f, 0.20f, 1f);

    private Lock currentLock = null;
    private string enteredCode = "";
    private TextMeshProUGUI displayText;
    private bool uiBuilt = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        Image bg = codePadPanel.GetComponent<Image>();
        if (bg != null) bg.color = bgColor;

        BuildUI();
        codePadPanel.SetActive(false);
    }

    void Update()
    {
        if (!codePadPanel.activeSelf) return;
        // Keyboard input support
        foreach (char c in Input.inputString)
        {
            if (c >= '0' && c <= '9') AppendDigit(c.ToString());
            else if (c == '\b') Backspace();
            else if (c == '\n' || c == '\r') Submit();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) ClosePad();
    }

    public void OpenPad(Lock lockSource)
    {
        currentLock = lockSource;
        enteredCode = "";
        codePadPanel.SetActive(true);
        RefreshDisplay(textNormal);
    }

    public void ClosePad()
    {
        codePadPanel.SetActive(false);
        currentLock = null;
        enteredCode = "";
    }
    private void AppendDigit(string digit)
    {
        if (enteredCode.Length >= maxCodeLength) return;
        enteredCode += digit;
        RefreshDisplay(textNormal);
    }

    private void Backspace()
    {
        if (enteredCode.Length == 0) return;
        enteredCode = enteredCode.Substring(0, enteredCode.Length - 1);
        RefreshDisplay(textNormal);
    }

    private void Submit()
    {
        if (currentLock == null) return;

        bool correct = currentLock.TryCodeUnlock(enteredCode);

        if (correct)
        {
            RefreshDisplay(textCorrect);
            // Small delay before closing so player sees the green flash
            Invoke(nameof(ClosePad), 0.6f);
        }
        else
        {
            RefreshDisplay(textWrong);
            // Clear input after a short pause so player can retry
            Invoke(nameof(ClearAfterWrong), 0.8f);
        }
    }

    private void ClearAfterWrong()
    {
        enteredCode = "";
        RefreshDisplay(textNormal);
    }

    private void RefreshDisplay(Color colour)
    {
        if (displayText == null) return;

        // Show entered digits as * bullets while typing, then reveal on submit
        displayText.text = enteredCode.Length > 0 ? new string('●', enteredCode.Length) : "_ _ _";
        displayText.color = colour;
    }
    private void BuildUI()
    {
        if (uiBuilt) return;
        uiBuilt = true;
        RectTransform root = codePadPanel.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(260f, 360f);
        root.anchoredPosition = Vector2.zero;
        var vlg = codePadPanel.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(14, 14, 14, 14);
        var titleGO = CreateChild("Title", codePadPanel);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "ENTER CODE";
        titleTMP.fontSize = 16f;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.65f, 0.65f, 0.70f, 1f);
        titleTMP.fontStyle = FontStyles.Bold;
        AddLE(titleGO, h: 24f);
        var dispGO = CreateChild("Display", codePadPanel);
        var dispImg = dispGO.AddComponent<Image>();
        dispImg.color = displayBgColor;
        AddLE(dispGO, h: 52f);
        var dispTxtGO = CreateChild("DisplayText", dispGO);
        var dispRT = dispTxtGO.GetComponent<RectTransform>();
        dispRT.anchorMin = Vector2.zero; dispRT.anchorMax = Vector2.one;
        dispRT.sizeDelta = Vector2.zero; dispRT.anchoredPosition = Vector2.zero;
        displayText = dispTxtGO.AddComponent<TextMeshProUGUI>();
        displayText.text = "_ _ _";
        displayText.fontSize = 26f;
        displayText.alignment = TextAlignmentOptions.Center;
        displayText.color = textNormal;
        // ── Keypad grid (3 columns) ──
        // Rows: 1-2-3 / 4-5-6 / 7-8-9 / ← 0 ↵
        string[][] rows = new string[][]
        {
            new[] { "1", "2", "3" },
            new[] { "4", "5", "6" },
            new[] { "7", "8", "9" },
            new[] { "←", "0",  "↵" }
        };

        foreach (var row in rows)
        {
            var rowGO = CreateChild("Row", codePadPanel);
            var rowHLG = rowGO.AddComponent<HorizontalLayoutGroup>();
            rowHLG.childControlWidth = true;
            rowHLG.childForceExpandWidth = true;
            rowHLG.childControlHeight = true;
            rowHLG.childForceExpandHeight = true;
            rowHLG.spacing = 8f;
            AddLE(rowGO, h: 56f);

            foreach (var label in row)
            {
                var keyGO = CreateChild($"Key_{label}", rowGO);
                var keyImg = keyGO.AddComponent<Image>();
                Color kc = label == "↵" ? enterColor : label == "←" ? backColor : keyBgColor;
                keyImg.color = kc;
                var lblGO = CreateChild("Label", keyGO);
                var lblRT = lblGO.GetComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
                lblRT.sizeDelta = Vector2.zero;
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text = label;
                lbl.fontSize = label == "↵" || label == "←" ? 20f : 22f;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.color = Color.white;
                lbl.raycastTarget = false;
                var btn = keyGO.AddComponent<Button>();
                btn.targetGraphic = keyImg;
                var cb = btn.colors;
                cb.normalColor = kc;
                cb.highlightedColor = label == "↵" ? Lighten(enterColor) : label == "←" ? Lighten(backColor) : keyHoverColor;
                cb.pressedColor = Darken(kc);
                btn.colors = cb;
                string captured = label;
                btn.onClick.AddListener(() => OnKeyPressed(captured));
            }
        }
        var hintGO = CreateChild("Hint", codePadPanel);
        var hintTMP = hintGO.AddComponent<TextMeshProUGUI>();
        hintTMP.text = "ESC to cancel";
        hintTMP.fontSize = 11f;
        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.color = new Color(0.45f, 0.45f, 0.50f, 1f);
        AddLE(hintGO, h: 16f);
    }

    private void OnKeyPressed(string key)
    {
        switch (key)
        {
            case "←": Backspace(); break;
            case "↵": Submit(); break;
            default: AppendDigit(key); break;
        }
    }

    private static GameObject CreateChild(string name, GameObject parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void AddLE(GameObject go, float h = -1f, float w = -1f)
    {
        var le = go.AddComponent<LayoutElement>();
        if (h >= 0) le.preferredHeight = h;
        if (w >= 0) le.preferredWidth = w;
    }

    private static Color Lighten(Color c) =>
        new Color(Mathf.Min(c.r + 0.12f, 1f),
                  Mathf.Min(c.g + 0.12f, 1f),
                  Mathf.Min(c.b + 0.12f, 1f), c.a);

    private static Color Darken(Color c) =>
        new Color(Mathf.Max(c.r - 0.10f, 0f),
                  Mathf.Max(c.g - 0.10f, 0f),
                  Mathf.Max(c.b - 0.10f, 0f), c.a);
}