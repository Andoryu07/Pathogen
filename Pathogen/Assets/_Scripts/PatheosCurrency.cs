using UnityEngine;
/// World pickup for Patheos currency
public class PatheosCurrency : InteractableBase
{
    [Header("Currency")]
    [SerializeField] private int amount = 100;
    [Header("Visuals")]
    [SerializeField] private Sprite worldSprite;
    [SerializeField] private Color spriteColor = new Color(0.9f, 0.75f, 0.2f, 1f);//gold

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            if (worldSprite != null) sr.sprite = worldSprite;
            sr.color = spriteColor;
            transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        }

        UpdatePrompt();
    }
    ///Called by EnemyInfected after instantiation to set the drop value
    public void SetAmount(int value)
    {
        amount = value;
        UpdatePrompt();
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