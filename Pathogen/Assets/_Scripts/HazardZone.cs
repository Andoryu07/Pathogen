using UnityEngine;

public class HazardZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HazardMaskController.Instance?.EnterHazardZone();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HazardMaskController.Instance?.ExitHazardZone();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.1f, 0.8f, 0.1f, 0.20f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(transform.position,
                col is BoxCollider2D box
                    ? (Vector3)box.size
                    : new Vector3(2f, 2f, 0f));
    }
}