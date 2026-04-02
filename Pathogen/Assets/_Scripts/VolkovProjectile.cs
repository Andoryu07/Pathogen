using UnityEngine;
/// Projectile used by Volkov's stump and tentacle attacks.
/// Travels in a straight line, damages player on contact, destroys on range or hit
public class VolkovProjectile : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color projectileColor = new Color(0.5f, 0.1f, 0.8f, 1f);
    [SerializeField] private float scale = 0.4f;

    // Set by VolkovBoss.SpawnProjectile
    private float damage;
    private float range;
    private float speed;
    private Vector2 direction;
    private float travelled;
    private bool hit = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = projectileColor;
            sr.sortingOrder = 5;
        }
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void Launch(Vector2 dir, float spd, float rng, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        range = rng;
        damage = dmg;

        if (rb != null) rb.linearVelocity = direction * speed;

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (hit) return;
        travelled += speed * Time.deltaTime;
        if (travelled >= range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hit) return;
        if (!other.CompareTag("Player")) return;

        hit = true;
        bool blocked = BlockingSystem.Instance != null && BlockingSystem.Instance.IsBlocking;
        if (blocked)
            HUDFeedback.Instance?.ShowInfo("Volkov's attack blocked!");
        else
        {
            PlayerController.LocalInstance?.TakeDamage(damage);
            HUDFeedback.Instance?.ShowWarning($"Volkov hits you! -{damage} HP!");
        }
        Destroy(gameObject);
    }
}