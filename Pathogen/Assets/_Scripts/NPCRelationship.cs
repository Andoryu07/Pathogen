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

    private RelationshipState currentState;
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
    public static NPCRelationship Find(string id)
    {
        foreach (var npc in FindObjectsOfType<NPCRelationship>())
            if (npc.npcID == id) return npc;
        return null;
    }
}