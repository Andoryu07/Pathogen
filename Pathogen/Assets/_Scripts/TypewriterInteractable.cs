using UnityEngine;
/// Typewriter world object — player presses E to open the save/load UI
public class TypewriterInteractable : InteractableBase
{
    [Header("References")]
    [SerializeField] private SaveUIManager saveUI;
    string interactKey = InputManager.Instance.GetKeyForAction("Interact").ToString();

    void Awake()
    {
        promptMessage = $"{interactKey} - Save Game";
    }

    public override void Interact()
    {
        if (saveUI == null)
        {
            Debug.LogWarning("[Typewriter] SaveUIManager not assigned!");
            return;
        }
        saveUI.Open();
    }
}