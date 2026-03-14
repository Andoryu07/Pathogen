using UnityEngine;

/// World pickup for Patheos currency.
/// NOT an Item — does not go into the inventory grid.
public class PatheosCurrency : InteractableBase
{
    [Header("Currency")]
    [SerializeField] private int amount = 100;
    [Header("Visuals")]
    [SerializeField] private Sprite worldSprite;
    [SerializeField] private Color spriteColor = new Color(0.9f, 0.75f, 0.2f, 1f); // gold
    [SerializeField] private bool showWorldSprite = false;   // off by default — currency is invisible on ground
    [SerializeField] private float worldScale = 0.25f;

    private SpriteRenderer sr;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdatePrompt();
    }

    void Start()
    {
        if (sr != null)
        {
            if (showWorldSprite && worldSprite != null)
            {
                sr.sprite = worldSprite;
                sr.color = spriteColor;
                sr.enabled = true;
                transform.localScale = new Vector3(worldScale, worldScale, 1f);
            }
            else
            {
                sr.enabled = false;
            }
        }
    }

    ///Called by EnemyInfected immediately after Instantiate to set the drop value
    public void SetAmount(int value)
    {
        amount = value;
        UpdatePrompt();
        // Also update SR color in case Start hasn't run yet — will be overwritten by Start safely
        if (sr != null) sr.color = spriteColor;
    }

    public override void Interact()
    {
        WalletManager.Instance?.Add(amount);
        gameObject.SetActive(false);
    }

    private void UpdatePrompt()
    {
        promptMessage = $"E - Pick up {amount} Patheos";
    }
}