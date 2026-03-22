using UnityEngine;
using System;
using System.Collections.Generic;

public enum DialogueActionType
{
    None,
    OpenSilasShop,          // opens SilasShopUI
    OpenItemShop,           // opens item shop tab directly
    OpenWeaponUpgrades,     // opens weapon upgrades tab
    OpenQuests,             // opens quest tab
    UnlockCraftingRecipe,   // unlocks a recipe by name
    GiveItem,               // adds an item to inventory (uses ItemRegistry)
    TriggerCustomEvent,     // fires a named event other scripts can listen to
    TakeItem,               // player gives an item to the NPC (removed from inventory)
}

[Serializable]
public class DialogueChoice
{
    [Tooltip("Text shown on the choice button.")]
    public string choiceText = "...";
    [Tooltip("Index of the DialogueLine to jump to when this choice is selected. -1 = end conversation.")]
    public int nextLineIndex = -1;
    [Tooltip("Optional action triggered when this choice is selected.")]
    public DialogueActionType actionType = DialogueActionType.None;
    [Tooltip("Parameter for the action (recipe name, item name, event name etc.)")]
    public string actionParameter = "";
    [Tooltip("Amount for GiveItem/TakeItem actions.")]
    public int actionAmount = 1;
    [Tooltip("Line to jump to if TakeItem fails (player lacks items). -1 = stay on this line.")]
    public int actionFailLineIndex = -1;
}

[Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    public string speakerName = "";
    public Sprite speakerPortrait;
    [Header("Text")]
    [TextArea(2, 6)]
    public string text = "";
    [Header("Navigation")]
    [Tooltip("Index of the next line to show. -1 = end conversation. Ignored if choices exist.")]
    public int nextLineIndex = -1;
    [Header("Choices (leave empty for no choice)")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    [Header("Action after this line is dismissed (no choices)")]
    [Tooltip("Fires when the player advances past this line (only if no choices).")]
    public DialogueActionType actionType = DialogueActionType.None;
    public string actionParameter = "";
    [Tooltip("Amount for GiveItem/TakeItem actions.")]
    public int actionAmount = 1;
    [Tooltip("Line to jump to if TakeItem fails (player lacks items). -1 = stay on this line.")]
    public int actionFailLineIndex = -1;
}

/// ScriptableObject defining a complete dialogue conversation
[CreateAssetMenu(fileName = "Dialogue_New", menuName = "Pathogen/Dialogue Conversation")]
public class DialogueConversation : ScriptableObject
{
    [Tooltip("Starting line index (usually 0).")]
    public int startLineIndex = 0;
    [Tooltip("All lines in this conversation.")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}