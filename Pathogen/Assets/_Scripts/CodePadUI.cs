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
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI displayText;
    [Header("Colors")]
    [SerializeField] private Color textNormal = new Color(0.90f, 0.90f, 0.90f, 1f);
    [SerializeField] private Color textCorrect = new Color(0.20f, 0.90f, 0.30f, 1f);
    [SerializeField] private Color textWrong = new Color(0.90f, 0.20f, 0.20f, 1f);
    [Header("Settings")]
    [SerializeField] private int maxCodeLength = 6;

    private Lock currentLock = null;
    private string enteredCode = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        codePadPanel.SetActive(false);
    }

    void Update()
    {
        if (!codePadPanel.activeSelf) return;

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

    public void PressKey0() => AppendDigit("0");
    public void PressKey1() => AppendDigit("1");
    public void PressKey2() => AppendDigit("2");
    public void PressKey3() => AppendDigit("3");
    public void PressKey4() => AppendDigit("4");
    public void PressKey5() => AppendDigit("5");
    public void PressKey6() => AppendDigit("6");
    public void PressKey7() => AppendDigit("7");
    public void PressKey8() => AppendDigit("8");
    public void PressKey9() => AppendDigit("9");
    public void PressBackspace() => Backspace();
    public void PressEnter() => Submit();

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
            Invoke(nameof(ClosePad), 0.6f);
        }
        else
        {
            RefreshDisplay(textWrong);
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
        displayText.text = enteredCode.Length > 0
            ? new string('●', enteredCode.Length)
            : "_ _ _";
        displayText.color = colour;
    }
}