using UnityEngine;

public class DarkZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        LighterController.Instance?.EnterDarkZone();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        LighterController.Instance?.ExitDarkZone();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0f, 0f, 0.25f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(transform.position,
                col is BoxCollider2D box
                    ? (Vector3)box.size
                    : new Vector3(2f, 2f, 0f));
    }
}