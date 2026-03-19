using UnityEngine;
using System.Collections;

/// LEAPER anomaly
[RequireComponent(typeof(Rigidbody2D))]
public class AnomalyLeaper : MonoBehaviour, IDamageable
{

    [Header("Stats")]
    [SerializeField] private float maxHealth = 45f;    
    [SerializeField] private float scurrySpeed = 7f;  
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private float attackWindup = 0.25f;  
    [SerializeField] private float attackCooldown = 0.8f;
    [Header("Radii")]
    [SerializeField] private float detectionRadius = 10f;   
    [SerializeField] private float lungeRadius = 4f;    
    [SerializeField] private float attackRadius = 0.8f;
    [Header("Lunge")]
    [SerializeField] private float lungeSpeed = 18f;   
    [SerializeField] private float lungeDuration = 0.25f; 
    [SerializeField] private float lungeCooldown = 2f;
    [Header("Erratic Movement")]
    [SerializeField] private float directionChangeInterval = 0.3f; 
    [SerializeField] private float erraticStrength = 0.4f; // 0=straight, 1=very erratic
    [Header("Visuals")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.6f, 0.2f, 1f); // green
    [SerializeField] private Color lungeColor = new Color(1f, 1f, 0.1f, 1f); // yellow flash
    [SerializeField] private Color windupColor = Color.white;
    [SerializeField] private float deathDelay = 0.8f;

    private enum State { Idle, Scurry, Lunge, Attack, Dead }
    private State state = State.Idle;
    private float currentHealth;
    private bool isAttacking = false;
    private bool isWinding = false;
    private bool isLunging = false;
    private float lungeCooldownTimer = 0f;
    private float dirChangeTimer = 0f;
    private Vector2 moveDir;
    private Coroutine attackCoroutine = null;
    private Coroutine deathCoroutine = null;
    private Coroutine lungeCoroutine = null;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AnomalyLootTable lootTable;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        lootTable = GetComponent<AnomalyLootTable>();
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        if (sr != null) sr.color = normalColor;
    }

    void Start()
    {
        playerTransform = PlayerController.LocalInstance?.transform;
    }

    void Update()
    {
        if (state == State.Dead || playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        lungeCooldownTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                if (dist <= detectionRadius) state = State.Scurry;
                break;
            case State.Scurry:
                if (dist > detectionRadius) { state = State.Idle; break; }
                // Close enough to attack
                if (dist <= attackRadius && !isAttacking)
                {
                    state = State.Attack;
                    isAttacking = true;
                    attackCoroutine = StartCoroutine(AttackRoutine());
                    break;
                }
                // In lunge range and cooldown ready
                if (dist <= lungeRadius && dist > attackRadius && lungeCooldownTimer <= 0f && !isLunging)
                {
                    state = State.Lunge;
                    lungeCoroutine = StartCoroutine(LungeRoutine());
                    break;
                }
                // Erratic direction update
                dirChangeTimer -= Time.deltaTime;
                if (dirChangeTimer <= 0f)
                {
                    dirChangeTimer = directionChangeInterval;
                    Vector2 toPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
                    Vector2 random = Random.insideUnitCircle.normalized;
                    moveDir = Vector2.Lerp(toPlayer, random, erraticStrength).normalized;
                }
                break;
            case State.Lunge:
                // Handled by LungeRoutine
                break;
            case State.Attack:
                if (!isWinding && !isAttacking && dist > attackRadius)
                    state = State.Scurry;
                break;
        }
    }
    void FixedUpdate()
    {
        if (state == State.Dead) return;
        if (state == State.Scurry && !isLunging)
            rb.MovePosition(rb.position + moveDir * scurrySpeed * Time.fixedDeltaTime);
    }

    private IEnumerator LungeRoutine()
    {
        isLunging = true;
        if (sr != null) sr.color = lungeColor;
        Vector2 lungeDir = ((Vector2)playerTransform.position - rb.position).normalized;
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            rb.MovePosition(rb.position + lungeDir * lungeSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();

            // If we've reached attack range mid-lunge, stop and attack
            if (Vector2.Distance(transform.position, playerTransform.position) <= attackRadius)
                break;
        }
        if (sr != null) sr.color = normalColor;
        isLunging = false;
        lungeCooldownTimer = lungeCooldown;
        lungeCoroutine = null;
        // Check if we're close enough to attack after lunge
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRadius
            && !isAttacking)
        {
            state = State.Attack;
            isAttacking = true;
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
        else
        {
            state = State.Scurry;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isWinding = true;
        if (sr != null) sr.color = windupColor;
        yield return new WaitForSeconds(attackWindup);
        if (sr != null) sr.color = normalColor;
        isWinding = false;

        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRadius)
        {
            PlayerController.LocalInstance?.TakeDamage(contactDamage);
            HUDFeedback.Instance?.ShowWarning($"Leaper hit! -{contactDamage} HP");
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        attackCoroutine = null;
        if (state == State.Attack) state = State.Scurry;
    }

    public void TakeDamage(float damage)
    {
        if (state == State.Dead) return;
        currentHealth -= damage;
        if (state == State.Idle) state = State.Scurry;
        if (sr != null) StartCoroutine(DamageFlash());
        if (currentHealth <= 0f && deathCoroutine == null)
            deathCoroutine = StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        state = State.Dead;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
        if (lungeCoroutine != null) { StopCoroutine(lungeCoroutine); lungeCoroutine = null; }

        Vector2 lootPos = transform.position;

        if (sr != null)
        {
            float elapsed = 0f;
            Color start = sr.color;
            while (elapsed < deathDelay)
            {
                elapsed += Time.deltaTime;
                sr.color = new Color(start.r, start.g, start.b,
                                     Mathf.Lerp(1f, 0f, elapsed / deathDelay));
                yield return null;
            }
        }
        else yield return new WaitForSeconds(deathDelay);

        QuestManager.Instance?.ReportEnemyKill(gameObject.tag);
        Debug.Log("[" + gameObject.name + "] Reporting kill — tag:" + gameObject.tag);
        QuestManager.Instance?.ReportEnemyKill(gameObject.tag);
        lootTable?.DropAll(lootPos);
        Destroy(gameObject);
    }

    private IEnumerator DamageFlash()
    {
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (state != State.Dead) sr.color = original;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lungeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}