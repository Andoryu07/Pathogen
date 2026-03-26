using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
/// Singleton managing all dialogue flow
/// Handles: line display, text animation, choices, actions, freezing
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private GameObject portraitFrame;      
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choicesContainer;   
    [SerializeField] private Button[] choiceButtons;      
    [SerializeField] private TextMeshProUGUI[] choiceButtonTexts;
    [Header("Text Animation")]
    [SerializeField] private float charsPerSecond = 40f;
    [SerializeField] private bool animateText = true;
    [Header("Continue Hint")]
    [SerializeField] private GameObject continueHint;  // "Double-click to continue" label
    private HashSet<string> playedConversations = new HashSet<string>();
    private DialogueConversation activeConversation;
    private int currentLineIndex;
    private bool isAnimating = false;
    private bool isOpen = false;
    private Coroutine typeCoroutine;
    private Rigidbody2D frozenNpcRb;
    private float lastClickTime = 0f;
    private const float DoubleClickGap = 0.35f;
    public bool IsOpen => isOpen;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (continueHint != null) continueHint.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLast = Time.unscaledTime - lastClickTime;
            lastClickTime = Time.unscaledTime;

            if (isAnimating)
            {
                if (timeSinceLast <= DoubleClickGap)
                {
                    // Double-click while animating — finish AND advance
                    FinishAnimation();
                    AdvanceLine();
                }
                else
                {
                    // Single click while animating — just finish animation
                    FinishAnimation();
                }
            }
            else
            {
                // Animation done — single click advances
                AdvanceLine();
            }
        }
    }

    public void StartDialogue(DialogueConversation conversation, Rigidbody2D npcRb = null)
    {
        if (conversation == null || conversation.lines.Count == 0) return;
        activeConversation = conversation;
        currentLineIndex = conversation.startLineIndex;
        frozenNpcRb = npcRb;
        isOpen = true;
        // Freeze player and NPC
        FreezeParticipants(true);
        dialoguePanel.SetActive(true);
        TimeScaleManager.Freeze(this);
        WeaponHUD.Instance?.Hide();
        ShowLine(currentLineIndex);
    }

    public void EndDialogue()
    {
        isOpen = false;
        if (activeConversation != null)
            playedConversations.Add(activeConversation.name);
        if (typeCoroutine != null) { StopCoroutine(typeCoroutine); typeCoroutine = null; }
        dialoguePanel.SetActive(false);
        TimeScaleManager.Unfreeze(this);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (continueHint != null) continueHint.SetActive(false);
        FreezeParticipants(false);
        WeaponHUD.Instance?.Show();
        WeaponHUD.Instance?.RefreshAmmoText();
        activeConversation = null;
        frozenNpcRb = null;
    }

    private void ShowLine(int index)
    {
        if (activeConversation == null) return;
        if (index < 0 || index >= activeConversation.lines.Count)
        {
            EndDialogue();
            return;
        }
        currentLineIndex = index;
        DialogueLine line = activeConversation.lines[index];
        // Speaker name
        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;
        // Portrait
        if (portraitFrame != null)
            portraitFrame.SetActive(line.speakerPortrait != null);
        if (speakerPortrait != null && line.speakerPortrait != null)
            speakerPortrait.sprite = line.speakerPortrait;
        // Hide choices while text is showing
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (continueHint != null) continueHint.SetActive(false);
        // Text
        if (animateText)
        {
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            typeCoroutine = StartCoroutine(TypeText(line.text, line));
        }
        else
        {
            if (dialogueText != null) dialogueText.text = line.text;
            OnLineFullyShown(line);
        }
    }

    private IEnumerator TypeText(string fullText, DialogueLine line)
    {
        isAnimating = true;
        if (dialogueText != null) dialogueText.text = "";
        float delay = 1f / charsPerSecond;
        foreach (char c in fullText)
        {
            if (dialogueText != null) dialogueText.text += c;
            yield return new WaitForSecondsRealtime(delay);  // unscaled — works when time is frozen
        }
        isAnimating = false;
        typeCoroutine = null;
        OnLineFullyShown(line);
    }

    private void FinishAnimation()
    {
        if (typeCoroutine != null) { StopCoroutine(typeCoroutine); typeCoroutine = null; }
        isAnimating = false;
        DialogueLine line = activeConversation.lines[currentLineIndex];
        if (dialogueText != null) dialogueText.text = line.text;
        OnLineFullyShown(line);
    }

    private void OnLineFullyShown(DialogueLine line)
    {
        if (line.choices != null && line.choices.Count > 0)
            ShowChoices(line);
        else if (continueHint != null)
            continueHint.SetActive(true);
    }

    private void AdvanceLine()
    {
        DialogueLine line = activeConversation.lines[currentLineIndex];
        // Don't advance if choices are showing — player must click a button
        if (line.choices != null && line.choices.Count > 0) return;
        // Fire action before advancing
        if (line.actionType != DialogueActionType.None)
        {
            ExecuteAction(line.actionType, line.actionParameter,
                          line.actionAmount, line.actionFailLineIndex);
            // TakeItem handles its own navigation — don't advance here
            if (line.actionType == DialogueActionType.TakeItem) return;
        }
        int next = line.nextLineIndex;
        if (next < 0) EndDialogue();
        else ShowLine(next);
    }

    private void ShowChoices(DialogueLine line)
    {
        if (choicesContainer == null) return;
        choicesContainer.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;
            if (i < line.choices.Count)
            {
                DialogueChoice choice = line.choices[i];
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceButtonTexts[i] != null)
                    choiceButtonTexts[i].text = choice.choiceText;
                int capturedI = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(line.choices[capturedI]));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (choice.actionType != DialogueActionType.None)
        {
            ExecuteAction(choice.actionType, choice.actionParameter,
                          choice.actionAmount, choice.actionFailLineIndex);
            if (choice.actionType == DialogueActionType.TakeItem) return;
        }
        if (choice.nextLineIndex < 0) EndDialogue();
        else ShowLine(choice.nextLineIndex);
    }

    private void ExecuteAction(DialogueActionType type, string parameter,
                               int amount = 1, int failLineIndex = -1)
    {
        switch (type)
        {
            case DialogueActionType.OpenSilasShop:
                EndDialogue();
                SilasShopUI.Instance?.Open();
                break;
            case DialogueActionType.OpenItemShop:
                EndDialogue();
                SilasShopUI.Instance?.Open();
                // Tab switching handled by SilasShopUI — it opens on item shop by default
                break;
            case DialogueActionType.OpenWeaponUpgrades:
                EndDialogue();
                // Open Silas shop on weapon upgrades tab
                SilasShopUI.Instance?.OpenOnTab(1);
                break;
            case DialogueActionType.OpenQuests:
                EndDialogue();
                SilasShopUI.Instance?.OpenOnTab(2);
                break;
            case DialogueActionType.UnlockCraftingRecipe:
                CraftingManager.Instance?.UnlockRecipe(parameter);
                HUDFeedback.Instance?.ShowInfo("New recipe unlocked: " + parameter);
                break;
            case DialogueActionType.GiveItem:
                GiveItemToPlayer(parameter, amount);
                break;
            case DialogueActionType.TriggerCustomEvent:
                DialogueEventBus.Trigger(parameter);
                break;
            case DialogueActionType.TakeItem:
                TakeItemFromPlayer(parameter, amount, failLineIndex);
                return;

            case DialogueActionType.ChangeRelationship:
                {
                    var npcRel = NPCRelationship.Find(parameter);
                    npcRel?.SetRelationship((RelationshipState)amount);
                    break;
                }
            case DialogueActionType.ImproveRelationship:
                {
                    NPCRelationship.Find(parameter)?.ImproveRelationship();
                    break;
                }
            case DialogueActionType.WorsenRelationship:
                {
                    NPCRelationship.Find(parameter)?.WorsenRelationship();
                    break;
                }
        }
    }

    private void GiveItemToPlayer(string itemName, int amount = 1)
    {
        if (ItemRegistry.Instance == null) return;
        GameObject prefab = ItemRegistry.Instance.GetPrefab(itemName);
        if (prefab == null) { Debug.LogWarning("[Dialogue] No item prefab for: " + itemName); return; }

        int given = 0;
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(prefab);
            Item itemComp = go.GetComponent<Item>();
            if (itemComp == null) { Destroy(go); break; }
            bool added = InventoryGrid.Instance.TryAddItem(itemComp);
            if (added) { go.SetActive(false); given++; }
            else { Destroy(go); break; }
        }
        HUDFeedback.Instance?.ShowInfo("Received: " + given + "x " + itemName);
    }

    /// Player gives items to the NPC
    /// Requires the FULL amount — partial gives are rejected
    /// On failure: shows feedback and jumps to failLineIndex (or stays on current line if -1)
    /// On success: advances normally
    private void TakeItemFromPlayer(string itemName, int amount, int failLineIndex)
    {
        // Check total available
        int available = InventoryGrid.Instance.CountItem(itemName);
        if (available < amount)
        {
            HUDFeedback.Instance?.ShowWarning(
                "You need " + amount + "x " + itemName + " (have " + available + ").");
            DialogueEventBus.Trigger("TakeItem_Failed_" + itemName);
            // Jump to fail line or stay on current line
            if (failLineIndex >= 0) ShowLine(failLineIndex);
            // else stay — do nothing, dialogue remains on current line
            return;
        }
        // Remove the required amount
        for (int i = 0; i < amount; i++)
        {
            Item item = InventoryGrid.Instance.GetItem(itemName);
            if (item == null) break;
            InventoryGrid.Instance.RemoveItem(item);
        }
        InventoryUIManager.Instance?.RefreshInventoryGrid();
        HUDFeedback.Instance?.ShowInfo("Gave " + amount + "x " + itemName + ".");
        DialogueEventBus.Trigger("TakeItem_Success_" + itemName);
        // Advance past the current line
        DialogueLine line = activeConversation.lines[currentLineIndex];
        int next = line.nextLineIndex;
        if (next < 0) EndDialogue();
        else ShowLine(next);
    }

    public bool HasBeenPlayed(DialogueConversation conv)
        => playedConversations.Contains(conv.name);

    public bool AreConditionsMet(DialogueConversation conv)
    {
        if (conv.conditions == null) return true;
        foreach (var cond in conv.conditions)
        {
            if (!CheckCondition(cond)) return false;
        }
        return true;
    }

    private bool CheckCondition(DialogueCondition cond)
    {
        switch (cond.type)
        {
            case ConditionType.None:
                return true;
            case ConditionType.RelationshipAtLeast:
                var npc = NPCRelationship.Find(cond.parameter);
                return npc != null && npc.IsAtLeast((RelationshipState)cond.intParameter);
            case ConditionType.QuestCompleted:
                var qData = Resources.Load<QuestData>(cond.parameter);
                return QuestManager.Instance != null &&
                       qData != null &&
                       QuestManager.Instance.IsCompleted(qData);
            case ConditionType.QuestActive:
                var qDataA = Resources.Load<QuestData>(cond.parameter);
                return QuestManager.Instance != null &&
                       qDataA != null &&
                       QuestManager.Instance.IsActive(qDataA);
            case ConditionType.HasItem:
                return InventoryGrid.Instance != null &&
                       InventoryGrid.Instance.HasItem(cond.parameter);
            case ConditionType.TalismanCountAtLeast:
                return TalismanManager.Instance != null &&
                       TalismanManager.Instance.CollectedCount >= cond.intParameter;

            default:
                return true;
        }
    }

    private void FreezeParticipants(bool freeze)
    {
        // Freeze player movement
        PlayerController player = PlayerController.LocalInstance;
        if (player != null)
            player.SetMovementEnabled(!freeze);
        // Freeze NPC
        if (frozenNpcRb != null)
            frozenNpcRb.simulated = !freeze;
    }
}