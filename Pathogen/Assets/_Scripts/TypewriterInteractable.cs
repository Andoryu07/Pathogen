using UnityEngine;
/// Typewriter world object — player presses E to open the save/load UI
public class TypewriterInteractable : InteractableBase
{
    [Header("References")]
    [SerializeField] private SaveUIManager saveUI;

    void Awake()
    {
        promptMessage = "E - Save Game";
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