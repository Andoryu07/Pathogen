using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interactable Settings")]
    [SerializeField] protected string promptMessage = "Interact";
    public abstract void Interact();
    public virtual string GetInteractionPrompt()
    {
        return promptMessage;
    }

    protected virtual void OnHighlight() { }
    protected virtual void OnStopHighlight() { }
}