using UnityEngine;
using System.Collections;
/// STALKER anomaly — blind, reacts to sound
[RequireComponent(typeof(Rigidbody2D))]
public class AnomalyStalker : MonoBehaviour, IDamageable
{

    [Header("Stats")]
    [SerializeField] private float maxHealth = 150f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float alertSpeed = 3.5f;   // moving to sound source
    [SerializeField] private float chaseSpeed = 5.5f;   // full aggression speed
    [SerializeField] private float contactDamage = 35f;
    [SerializeField] private float attackWindup = 0.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    [Header("Detection (Sound)")]
    [SerializeField] private float soundRadius = 5f;     // walk detection radius
    [SerializeField] private float sprintSoundRadius = 10f;    // sprint detection radius
    [SerializeField] private float attackRadius = 1.0f;
    [SerializeField] private float loseInterestTime = 4f;     // seconds before returning to patrol
    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 4f;     // wander area around spawn
    [SerializeField] private float patrolWaitMin = 1f;
    [SerializeField] private float patrolWaitMax = 3f;
    [Header("Visuals")]
    [SerializeField] private Color normalColor = new Color(0.4f, 0.1f, 0.5f, 1f); // purple
    [SerializeField] private Color alertColor = new Color(0.8f, 0.5f, 0.1f, 1f); // orange
    [SerializeField] private Color chaseColor = new Color(0.9f, 0.1f, 0.1f, 1f); // red
    [SerializeField] private Color windupColor = Color.white;
    [SerializeField] private float deathDelay = 1.2f;

    private enum State { Patrol, Alert, Chase, Attack, Dead }
    private State state = State.Patrol;
    private float currentHealth;
    private bool isAttacking = false;
    private bool isWinding = false;
    private Coroutine attackCoroutine = null;
    private Coroutine deathCoroutine = null;
    private Coroutine patrolCoroutine = null;
    private float loseInterestTimer = 0f;
    private Vector2 lastSoundPos;
    private Vector2 spawnPos;
    private float passiveTimer = 0f;
    private float passiveInterval = 5f;
    private Transform playerTransform;
    private PlayerController playerController;
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
        // Apply difficulty scaling
        if (DifficultyManager.Instance != null)
        {
            maxHealth *= DifficultyManager.Instance.HealthMultiplier;
            currentHealth = maxHealth;
            contactDamage *= DifficultyManager.Instance.DamageMultiplier;
        }
        playerController = PlayerController.LocalInstance;
        if (playerController != null) playerTransform = playerController.transform;
        spawnPos = transform.position;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        // Passive ambient sound
        passiveTimer -= Time.deltaTime;
        if (passiveTimer <= 0f)
        {
            passiveTimer = passiveInterval + Random.Range(-1.5f, 1.5f);
            if (state == State.Patrol) AudioManager.Instance?.PlayStalkerPassive();
        }
        if (state == State.Dead || playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        switch (state)
        {
            case State.Patrol:
                CheckSoundDetection();
                break;

            case State.Alert:
                // Moving toward last sound position — check again for sound
                CheckSoundDetection();
                // If we've reached the sound pos, look around then return to patrol
                if (Vector2.Distance(transform.position, lastSoundPos) < 0.5f)
                {
                    loseInterestTimer += Time.deltaTime;
                    if (loseInterestTimer >= loseInterestTime)
                        ReturnToPatrol();
                }
                // If player is very close while alert, go full chase
                if (dist <= attackRadius * 2f)
                    EnterChase();
                break;

            case State.Chase:
                CheckSoundDetection(); // refresh last known pos
                if (dist <= attackRadius && !isAttacking)
                {
                    state = State.Attack;
                    isAttacking = true;
                    attackCoroutine = StartCoroutine(AttackRoutine());
                }
                // Lose interest if player is silent and far
                if (!PlayerMakesSound() && dist > soundRadius * 1.5f)
                {
                    loseInterestTimer += Time.deltaTime;
                    if (loseInterestTimer >= loseInterestTime)
                        ReturnToPatrol();
                }
                else
                {
                    loseInterestTimer = 0f;
                }
                break;

            case State.Attack:
                if (!isWinding && !isAttacking && dist > attackRadius)
                    state = State.Chase;
                break;
        }
    }

    void FixedUpdate()
    {
        if (state == State.Dead) return;

        switch (state)
        {
            case State.Alert:
                MoveToward(lastSoundPos, alertSpeed);
                break;
            case State.Chase:
                MoveToward(playerTransform.position, chaseSpeed);
                break;
        }
    }

    private void CheckSoundDetection()
    {
        if (playerController == null) return;
        if (!PlayerMakesSound()) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        float hearingRange = playerController.IsSprinting ? sprintSoundRadius : soundRadius;

        if (dist <= hearingRange)
        {
            lastSoundPos = playerTransform.position;
            loseInterestTimer = 0f;

            if (state == State.Patrol || state == State.Alert)
            {
                AudioManager.Instance?.PlayStalkerSpot();
                state = State.Alert;
                if (sr != null) sr.color = alertColor;
                if (patrolCoroutine != null)
                {
                    StopCoroutine(patrolCoroutine);
                    patrolCoroutine = null;
                }
            }
        }
    }

    private bool PlayerMakesSound()
    {
        if (playerController == null) return false;
        // Crouching = silent. Standing still = silent. Walking/sprinting = sound.
        return playerController.IsMoving && !playerController.IsCrouching;
    }

    private void EnterChase()
    {
        state = State.Chase;
        loseInterestTimer = 0f;
        if (sr != null) sr.color = chaseColor;
        HUDFeedback.Instance?.ShowWarning("Stalker detected you!");
    }

    private void ReturnToPatrol()
    {
        state = State.Patrol;
        if (sr != null) sr.color = normalColor;
        loseInterestTimer = 0f;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (state == State.Patrol)
        {
            // Pick a random point near spawn
            Vector2 target = spawnPos + Random.insideUnitCircle * patrolRadius;
            float wait = Random.Range(patrolWaitMin, patrolWaitMax);

            // Walk toward target
            float timeout = 3f;
            while (state == State.Patrol &&
                   Vector2.Distance(transform.position, target) > 0.3f &&
                   timeout > 0f)
            {
                rb.MovePosition(rb.position +
                    ((Vector2)target - rb.position).normalized * patrolSpeed * Time.fixedDeltaTime);
                timeout -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            // Wait at point
            float waited = 0f;
            while (state == State.Patrol && waited < wait)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 dir = (target - rb.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    private IEnumerator AttackRoutine()
    {
        BlockingSystem.Instance?.NotifyIncomingAttack(attackWindup);
        isWinding = true;
        AudioManager.Instance?.PlayStalkerWindup();
        if (sr != null) sr.color = windupColor;
        yield return new WaitForSeconds(attackWindup);
        if (sr != null) sr.color = chaseColor;
        isWinding = false;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= attackRadius)
        {
            bool blocked = BlockingSystem.Instance != null && BlockingSystem.Instance.IsBlocking;
            if (blocked) HUDFeedback.Instance?.ShowInfo("Attack blocked!");
            else
            {
                AudioManager.Instance?.PlayStalkerAttack();
                PlayerController.LocalInstance?.TakeDamage(contactDamage);
                HUDFeedback.Instance?.ShowWarning($"Stalker hit! -{contactDamage} HP");
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        attackCoroutine = null;
        if (state == State.Attack) state = State.Chase;
    }

    public float CurrentHealth => currentHealth;
    public void SetCurrentHealth(float hp)
    {
        currentHealth = Mathf.Clamp(hp, 0f, maxHealth);
    }
    public void ResetToFullHealth() => currentHealth = maxHealth;

    public void TakeDamage(float damage)
    {
        if (state == State.Dead) return;
        currentHealth -= damage;

        // Taking damage alerts the Stalker regardless of sound
        if (state == State.Patrol || state == State.Alert)
            EnterChase();

        if (sr != null) StartCoroutine(DamageFlash());
        if (currentHealth <= 0f && deathCoroutine == null)
            deathCoroutine = StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        state = State.Dead;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
        if (patrolCoroutine != null) { StopCoroutine(patrolCoroutine); patrolCoroutine = null; }
        if (sr != null) sr.color = normalColor;

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
        if (patrolCoroutine != null) { StopCoroutine(patrolCoroutine); patrolCoroutine = null; }
        state = State.Patrol;
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
        state = State.Patrol;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sprintSoundRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}