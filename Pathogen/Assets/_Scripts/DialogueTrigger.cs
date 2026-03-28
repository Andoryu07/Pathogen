using UnityEngine;
/// Attach to any NPC or interactable object to start a dialogue on E press
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
        // Block interaction if NPC is hostile
        NPCRelationship rel = GetComponent<NPCRelationship>();
        if (rel != null && rel.CurrentState == RelationshipState.Hostile)
        {
            HUDFeedback.Instance?.ShowWarning(rel.NPCID + " is hostile — they won't talk to you.");
            return;
        }
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