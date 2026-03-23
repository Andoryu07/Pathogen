using UnityEngine;
using System.Collections;
/// Handles the player's block/parry mechanic
public class BlockingSystem : MonoBehaviour
{
    public static BlockingSystem Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float blockWindow = 0.3f;  // seconds before impact the block window opens
    [SerializeField] private float blockCooldown = 0.8f;  // seconds before player can block again

    private bool isBlockWindowOpen = false;
    private bool isBlocking = false;
    private bool onCooldown = false;
    private float blockWindowTimer = 0f;
    public bool IsBlocking => isBlocking;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TryBlock();

        // Count down the block window
        if (isBlockWindowOpen)
        {
            blockWindowTimer -= Time.deltaTime;
            if (blockWindowTimer <= 0f)
            {
                isBlockWindowOpen = false;
                isBlocking = false;
            }
        }
    }
    //  Called by enemy AttackRoutine at the START of windup
    public void NotifyIncomingAttack(float attackWindup)
    {
        float delay = Mathf.Max(0f, attackWindup - blockWindow);
        StartCoroutine(OpenBlockWindowAfterDelay(delay));
    }

    private IEnumerator OpenBlockWindowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isBlockWindowOpen = true;
        blockWindowTimer = blockWindow;
        HUDFeedback.Instance?.ShowInfo("Block!");  
    }

    private void TryBlock()
    {
        if (onCooldown) return;
        if (!HasCrowbar()) return;

        if (isBlockWindowOpen)
        {
            // Successful block
            isBlocking = true;
            isBlockWindowOpen = false;
            HUDFeedback.Instance?.ShowInfo("Blocked!");
            Debug.Log("[Block] Successful block!");
            StartCoroutine(BlockCooldownRoutine());
        }
        // If window not open, do nothing — no penalty for pressing early/late
    }

    private IEnumerator BlockCooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(blockCooldown);
        onCooldown = false;
        isBlocking = false;
    }

    private bool HasCrowbar()
    {
        if (InventoryGrid.Instance == null) return false;
        return InventoryGrid.Instance.HasItem("Crowbar");
    }
}