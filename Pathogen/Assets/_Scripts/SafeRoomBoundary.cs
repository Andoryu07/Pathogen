using UnityEngine;
using System.Collections;
/// Safe room boundary — blocks enemies using a solid collider on a dedicated layer
/// Enemies that get close are pushed back and return to idle
public class SafeRoomBoundary : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionWidth = 2f;   // how wide to scan for nearby enemies
    [SerializeField] private float detectionHeight = 1.5f;
    [SerializeField] private float checkInterval = 0.05f; // seconds between checks (fast enough to catch movement)
    [Header("Repel Settings")]
    [SerializeField] private float pushBackDistance = 1.5f;
    [SerializeField] private float pauseDuration = 1.2f;
    [Header("Enemy Layer")]
    [SerializeField] private LayerMask enemyLayer;

    private static readonly string[] EnemyTags = { "Enemy", "Infected", "Brute", "Stalker", "Leaper" };

    void Start()
    {
        StartCoroutine(DetectionLoop());
    }

    private IEnumerator DetectionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckNearbyEnemies();
        }
    }

    private void CheckNearbyEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position,new Vector2(detectionWidth, detectionHeight),0f);

        foreach (var hit in hits)
        {
            if (!IsEnemy(hit)) continue;
            // Push direction = away from boundary centre
            Vector2 pushDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (pushDir == Vector2.zero) pushDir = Vector2.right;
            // Teleport enemy outside detection box immediately
            Vector2 pushTarget = (Vector2)transform.position + pushDir * (detectionWidth * 0.5f + pushBackDistance);
            // Directly move the enemy's Rigidbody2D position
            Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
            if (enemyRb != null) enemyRb.position = pushTarget;
            else hit.transform.position = pushTarget;
            RepelEnemy(hit, pushTarget);
        }
    }

    private void RepelEnemy(Collider2D col, Vector2 pushTarget)
    {
        EnemyInfected infected = col.GetComponent<EnemyInfected>();
        if (infected != null) { infected.ForceRepel(pushTarget, pauseDuration); return; }
        AnomalyStalker stalker = col.GetComponent<AnomalyStalker>();
        if (stalker != null) { stalker.ForceRepel(pushTarget, pauseDuration); return; }
        AnomalyLeaper leaper = col.GetComponent<AnomalyLeaper>();
        if (leaper != null) { leaper.ForceRepel(pushTarget, pauseDuration); return; }
        AnomalyBrute brute = col.GetComponent<AnomalyBrute>();
        if (brute != null) { brute.ForceRepel(pushTarget, pauseDuration); return; }
    }

    private bool IsEnemy(Collider2D col)
    {
        foreach (var tag in EnemyTags)
            if (col.CompareTag(tag)) return true;
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.3f);
        Gizmos.DrawCube(transform.position,new Vector3(detectionWidth, detectionHeight, 0f));
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.8f);
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionWidth, detectionHeight, 0f));
    }
}