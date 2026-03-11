using UnityEngine;

public enum LockType
{
    KeyItem,
    Code,
    Puzzle,
    AlwaysOpen
}

public class Lock : InteractableBase
{
    [Header("Lock Settings")]
    [SerializeField] private LockType lockType = LockType.KeyItem;
    [SerializeField] private bool isLocked = true;

    [Header("Key Item Settings")]
    [SerializeField] private string requiredItemName;
    [SerializeField] private bool consumeItem = true;

    [Header("Code Settings")]
    [SerializeField] private string correctCode = "1971";
    [SerializeField] private bool caseSensitive = false;

    [Header("Target")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool deactivateTargetOnUnlock = true;

    void Start()
    {
        UpdatePromptMessage();
    }

    private void UpdatePromptMessage()
    {
        switch (lockType)
        {
            case LockType.KeyItem:
                promptMessage = isLocked ? $"E - Open(Requires: {requiredItemName})" : "E - Open";
                break;
            case LockType.Code:
                promptMessage = isLocked ? "E - Enter code" : "E - Open";
                break;
            case LockType.Puzzle:
                promptMessage = isLocked ? "E - Inspect" : "E - Open";
                break;
            case LockType.AlwaysOpen:
                promptMessage = "E - Open";
                break;
        }
    }

    public override void Interact()
    {
        if (!isLocked)
        {
            Unlock();
            return;
        }

        switch (lockType)
        {
            case LockType.KeyItem:
                TryKeyItemUnlock();
                break;
            case LockType.Code:
                if (CodePadUI.Instance != null)
                    CodePadUI.Instance.OpenPad(this);
                else
                    Debug.LogWarning("[Lock] CodePadUI not found in scene!");
                break;
            case LockType.Puzzle:
                Debug.Log("Not implemented yet");
                break;
            case LockType.AlwaysOpen:
                Unlock();
                break;
        }
    }

    private void TryKeyItemUnlock()
    {
        if (string.IsNullOrEmpty(requiredItemName))
        {
            Debug.LogError("No item set for this key !");
            return;
        }
        bool hasReadable = ReadableManager.Instance.HasReadable(requiredItemName);
        bool hasItem = InventoryGrid.Instance.HasItem(requiredItemName);

        if (hasReadable || hasItem)
        {
            Debug.Log($"Unlocked using: {requiredItemName}");

            if (consumeItem && hasItem)
            {

                Item item = InventoryGrid.Instance.GetItem(requiredItemName);
                InventoryGrid.Instance.RemoveItem(item);
            }

            isLocked = false;
            Unlock();
        }
        else
        {
            Debug.Log($"You need: {requiredItemName}");
        }
    }

    ///Returns true if the code is correct and unlocks the lock
    public bool TryCodeUnlock(string enteredCode)
    {
        if (lockType != LockType.Code) return false;

        bool codeMatches = caseSensitive ?
            enteredCode == correctCode :
            enteredCode.ToLower() == correctCode.ToLower();

        if (codeMatches)
        {
            Debug.Log("[Lock] Correct code!");
            isLocked = false;
            Unlock();
            return true;
        }
        else
        {
            Debug.Log("[Lock] Incorrect code.");
            return false;
        }
    }

    private void Unlock()
    {
        Debug.Log("Unlocked!");

        if (targetObject != null)
        {
            if (deactivateTargetOnUnlock)
            {
                targetObject.SetActive(false);
            }
            else
            {
                Collider2D col = targetObject.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null) myCol.enabled = false;
        promptMessage = "Unlocked";
    }
}