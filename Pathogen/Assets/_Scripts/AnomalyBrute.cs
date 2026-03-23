using UnityEngine;
using System.Collections;
/// BRUTE anomaly
[RequireComponent(typeof(Rigidbody2D))]
public class AnomalyBrute : MonoBehaviour, IDamageable
{

    [Header("Stats")]
    [SerializeField] private float maxHealth = 600f; 
    [SerializeField] private float moveSpeed = 1.2f;  
    [SerializeField] private float slamDamage = 60f;    
    [SerializeField] private float slamWindup = 1.8f;   
    [SerializeField] private float slamCooldown = 2.5f;
    [Header("Radii")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float slamRadius = 1.6f;   

    [Header("Screen Shake (hook for camera system)")]
    [SerializeField] private float shakeMagnitude = 0.3f;
    [SerializeField] private float shakeDuration = 0.4f;
    [Header("Visuals")]
    [SerializeField] private Color normalColor = new Color(0.5f, 0.2f, 0.1f, 1f); // dark brown
    [SerializeField] private Color windupColor1 = new Color(0.9f, 0.4f, 0.1f, 1f); // orange pulse
    [SerializeField] private Color windupColor2 = new Color(1.0f, 0.1f, 0.1f, 1f); // red pulse
    [SerializeField] private Color slamColor = Color.white;
    [SerializeField] private float deathDelay = 2.0f;   

    private enum State { Idle, Chase, WindUp, Dead }
    private State state = State.Idle;
    private float currentHealth;
    private bool isAttacking = false;
    private Coroutine attackCoroutine = null;
    private Coroutine deathCoroutine = null;
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

        switch (state)
        {
            case State.Idle:
                if (dist <= detectionRadius)
                {
                    state = State.Chase;
                    HUDFeedback.Instance?.ShowWarning("A Brute is approaching...");
                }
                break;
            case State.Chase:
                if (dist <= slamRadius && !isAttacking)
                {
                    state = State.WindUp;
                    isAttacking = true;
                    attackCoroutine = StartCoroutine(SlamRoutine());
                }
                else if (dist > detectionRadius * 1.5f)
                {
                    state = State.Idle;
                }
                break;
            case State.WindUp:
                // SlamRoutine handles this
                break;
        }
    }

    void FixedUpdate()
    {
        if (state == State.Chase && playerTransform != null)
        {
            Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private IEnumerator SlamRoutine()
    {
        BlockingSystem.Instance?.NotifyIncomingAttack(slamWindup);
        float elapsed = 0f;
        float pulseRate = 4f;
        while (elapsed < slamWindup)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
            {
                float t = Mathf.PingPong(elapsed * pulseRate, 1f);
                sr.color = Color.Lerp(windupColor1, windupColor2, t);
            }
            yield return null;
        }
        // SLAM
        if (sr != null) sr.color = slamColor;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= slamRadius)
        {
            bool blocked = BlockingSystem.Instance != null && BlockingSystem.Instance.IsBlocking;
            if (blocked)
                HUDFeedback.Instance?.ShowInfo("Brute slam blocked!");
            else
            {
                PlayerController.LocalInstance?.TakeDamage(slamDamage);
                HUDFeedback.Instance?.ShowWarning($"BRUTE SLAM! -{slamDamage} HP");
            }
        }
        TriggerScreenShake();
        yield return new WaitForSeconds(0.2f);
        if (sr != null) sr.color = normalColor;
        yield return new WaitForSeconds(slamCooldown);
        isAttacking = false;
        attackCoroutine = null;
        if (state == State.WindUp) state = State.Chase;
    }

    private void TriggerScreenShake()
    {
        CameraShake.Instance?.Shake(shakeMagnitude, shakeDuration);
        Debug.Log($"[Brute] Screen shake triggered — magnitude:{shakeMagnitude} duration:{shakeDuration}");
    }

    public void TakeDamage(float damage)
    {
        if (state == State.Dead) return;
        currentHealth -= damage;

        if (state == State.Idle) state = State.Chase;
        if (sr != null) StartCoroutine(DamageFlash());
        float hpPercent = (currentHealth / maxHealth) * 100f;
        Debug.Log($"[Brute] Took {damage} dmg — {hpPercent:F0}% HP remaining.");
        if (currentHealth <= 0f && deathCoroutine == null)
            deathCoroutine = StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        state = State.Dead;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
        if (sr != null) sr.color = normalColor;
        Vector2 lootPos = transform.position;
        TriggerScreenShake(); 
        if (sr != null)
        {
            float elapsed = 0f;
            Color start = sr.color;
            while (elapsed < deathDelay)
            {
                elapsed += Time.deltaTime;
                // Pulse once before fading
                float pulse = elapsed < 0.5f
                    ? Mathf.PingPong(elapsed * 6f, 1f)
                    : 1f - ((elapsed - 0.5f) / (deathDelay - 0.5f));
                sr.color = new Color(start.r, start.g, start.b, pulse);
                yield return null;
            }
        }
        else yield return new WaitForSeconds(deathDelay);

        Debug.Log("[Brute] Reporting kill — tag:" + gameObject.tag + " QuestManager exists:" + (QuestManager.Instance != null));
        QuestManager.Instance?.ReportEnemyKill(gameObject.tag);
        PersistentEnemy pe = GetComponent<PersistentEnemy>();
        if (pe == null) Debug.LogWarning("[Enemy] PersistentEnemy missing on " + gameObject.name + " — add it and set a Scene ID!");
        pe?.RegisterDeath();
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

    public void ForceRepel(Vector2 pushTarget, float pauseDuration)
    {
        if (state == State.Dead) return;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }

        state = State.Idle;
        StartCoroutine(RepelRoutine(pushTarget, pauseDuration));
    }

    private System.Collections.IEnumerator RepelRoutine(Vector2 pushTarget, float pause)
    {
        float elapsed = 0f;
        Vector2 startPos = rb.position;
        while (elapsed < 0.3f)
        {
            elapsed += Time.fixedDeltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, pushTarget, elapsed / 0.3f));
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(pause);
        state = State.Idle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}