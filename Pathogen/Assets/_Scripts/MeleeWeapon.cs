using UnityEngine;
/// Melee weapon component
public class MeleeWeapon : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackAngle = 120f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private LayerMask enemyLayer;
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private float lastAttackTime = -999f;

    public static MeleeWeapon Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    /// Attempts a melee swing toward direction
    public bool TryAttack(Vector2 direction)
    {
        if (Time.time - lastAttackTime < attackCooldown) return false;
        lastAttackTime = Time.time;
        PerformAttack(direction);
        return true;
    }

    private void PerformAttack(Vector2 direction)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, attackRange, enemyLayer);

        float halfAngle = attackAngle * 0.5f;
        bool hitAnything = false;

        foreach (var hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position
                               - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(direction, toEnemy);
            if (angle > halfAngle) continue;

            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                    enemyRb.AddForce(toEnemy * knockbackForce, ForceMode2D.Impulse);
                Debug.Log($"[Melee] Hit {hit.name} for {damage} dmg.");
                hitAnything = true;
            }
        }

        if (!hitAnything)
            Debug.Log("[Melee] Swung but hit nothing.");
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}