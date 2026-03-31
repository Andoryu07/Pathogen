using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onConfirmAction;
    private Action onCancelAction;

    void Awake()
    {
        gameObject.SetActive(false);
        noButton.onClick.AddListener(Close);
        yesButton.onClick.AddListener(OnConfirmInternal);
    }
    public void Open(Action confirmAction, Action cancelAction)
    {
        onConfirmAction = confirmAction;
        onCancelAction = cancelAction;
        gameObject.SetActive(true);
    }

    private void OnConfirmInternal()
    {
        onConfirmAction?.Invoke();
        Close();
    }

    public void Close()
    {
        onCancelAction?.Invoke();
        gameObject.SetActive(false);
    }
}