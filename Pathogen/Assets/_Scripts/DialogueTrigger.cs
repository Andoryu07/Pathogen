using UnityEngine;
/// Attached to any NPC or interactable object to start a dialogue on E press
public class DialogueTrigger : InteractableBase
{
    [Header("Dialogue")]
    [SerializeField] private DialogueConversation conversation;
    [SerializeField] private string interactPrompt = "E - Talk";

    private Rigidbody2D npcRb;

    void Awake()
    {
        promptMessage = interactPrompt;
        npcRb = GetComponent<Rigidbody2D>();
    }

    public override void Interact()
    {
        if (conversation == null)
        {
            Debug.LogWarning("[DialogueTrigger] No conversation assigned on " + gameObject.name);
            return;
        }
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[DialogueTrigger] DialogueManager not found in scene.");
            return;
        }
        DialogueManager.Instance.StartDialogue(conversation, npcRb);
    }
}