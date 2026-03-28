using UnityEngine;

public enum RelationshipState
{
    Hostile = 0,  
    Neutral = 1,  
    Friendly = 2,   
    Trusting = 3,  
}
/// Tracks an NPC's relationship with the player
public class NPCRelationship : MonoBehaviour
{
    public static System.Action<NPCRelationship> OnRelationshipChanged;

    [Header("Identity")]
    [Tooltip("Unique NPC name — used to look up relationship by name.")]
    [SerializeField] private string npcID = "";
    [Header("Relationship")]
    [SerializeField] private RelationshipState initialState = RelationshipState.Neutral;
    [Header("Hostile Behaviour")]
    [SerializeField] private float hostilePushForce = 5f;
    [SerializeField] private float hostileContactDmg = 10f;
    [SerializeField] private float hostileDmgCooldown = 1.5f;
    private RelationshipState currentState;
    private float lastDmgTime = -99f;
    public string NPCID => npcID;
    public RelationshipState CurrentState => currentState;

    void Awake()
    {
        currentState = initialState;
    }

    public void SetRelationship(RelationshipState newState)
    {
        if (currentState == newState) return;
        RelationshipState old = currentState;
        currentState = newState;
        Debug.Log("[NPC] " + npcID + " relationship: " + old + " → " + newState);
        HUDFeedback.Instance?.ShowInfo(npcID + " is now " + newState + " toward you.");
        OnRelationshipChanged?.Invoke(this);
    }

    public void ImproveRelationship()
    {
        if (currentState < RelationshipState.Trusting)
            SetRelationship(currentState + 1);
    }

    public void WorsenRelationship()
    {
        if (currentState > RelationshipState.Hostile)
            SetRelationship(currentState - 1);
    }

    public bool IsAtLeast(RelationshipState required)
        => currentState >= required;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != RelationshipState.Hostile) return;
        if (!other.CompareTag("Player")) return;
        // Push player away
        Vector2 pushDir = ((Vector2)other.transform.position
                           - (Vector2)transform.position).normalized;
        Rigidbody2D prb = other.GetComponent<Rigidbody2D>();
        if (prb != null) prb.AddForce(pushDir * hostilePushForce, ForceMode2D.Impulse);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState != RelationshipState.Hostile) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDmgTime < hostileDmgCooldown) return;

        lastDmgTime = Time.time;
        PlayerController.LocalInstance?.TakeDamage(hostileContactDmg);
        HUDFeedback.Instance?.ShowWarning(npcID + " is attacking you!");
    }

    public static NPCRelationship Find(string id)
    {
        foreach (var npc in FindObjectsOfType<NPCRelationship>())
            if (npc.npcID == id) return npc;
        return null;
    }
}