using UnityEngine;
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
        // Check repeatable
        if (!conversation.isRepeatable &&
            DialogueManager.Instance.HasBeenPlayed(conversation))
        {
            HUDFeedback.Instance?.ShowInfo("...");
            return;
        }
        // Check conditions
        if (!DialogueManager.Instance.AreConditionsMet(conversation))
        {
            HUDFeedback.Instance?.ShowInfo("...");
            return;
        }
        DialogueManager.Instance.StartDialogue(conversation, npcRb);
    }
}