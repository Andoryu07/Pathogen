using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// Basic Infected enemy
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyInfected : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 80f;
    [SerializeField] private float moveSpeed = 2.0f;  
    [SerializeField] private float contactDamage = 15f;    
    [SerializeField] private float attackWindup = 0.7f;   
    [SerializeField] private float attackCooldown = 1.5f;   
    [Header("Radii")]
    [SerializeField] private float detectionRadius = 7f;
    [SerializeField] private float attackRadius = 0.9f;
    [Header("Loot")]
    [SerializeField] private GameObject patheosPrefab;              
    [SerializeField] private int patheosMinAmount = 100;
    [SerializeField] private int patheosMaxAmount = 1500;
    [SerializeField] private GameObject ammoDropPrefab;            
    [SerializeField]
    [Range(0f, 1f)]
    private float ammoDropChance = 0.20f;                         
    [SerializeField] private int ammoDropMin = 2;
    [SerializeField] private int ammoDropMax = 8;
    [Header("Attack Visuals")]
    [SerializeField] private Color windupColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float deathDelay = 1.0f;
    [Header("Grab Attack")]
    [SerializeField]
    [Range(0f, 1f)]
    private float grabChance = 0.25f;
    [SerializeField] private float grabEscapeWindow = 1.2f;   // seconds before damage starts (escape window)
    [SerializeField] private float grabDuration = 3.5f;   // total grab duration
    [SerializeField] private float grabDamageRate = 0.6f;   // seconds between ticks
    [SerializeField] private float grabDamage = 10f;
    [SerializeField] private int grabEscapeHits = 5;      // E presses needed to escape

    private enum State { Idle, Chase, Attack, Dead }
    private State state = State.Idle;
    private float currentHealth;
    private float attackTimer = 0f;
    private bool isWinding = false;
    private bool isAttacking = false;
    private bool isGrabbing = false;
    private int grabEscapeCounter = 0;
    private Coroutine attackCoroutine = null;
    private Coroutine deathCoroutine = null;
    private Coroutine grabCoroutine = null;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // Kinematic so the enemy moves by velocity but never physically
        // pushes or gets pushed by other rigidbodies (player, projectiles etc.)
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
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
        PlayerController player = PlayerController.LocalInstance;
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (state == State.Dead || playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        switch (state)
        {
            case State.Idle:
                if (dist <= detectionRadius) state = State.Chase;
                break;

            case State.Chase:
                if (dist <= attackRadius && !isAttacking)
                {
                    state = State.Attack;
                    isAttacking = true;
                    attackCoroutine = StartCoroutine(AttackRoutine());
                }
                else if (dist > detectionRadius)
                {
                    state = State.Idle;
                }
                break;

            case State.Attack:
                // AttackRoutine handles this state; return to Chase if player leaves radius
                if (!isWinding && !isAttacking && dist > attackRadius)
                    state = State.Chase;
                break;
        }
    }

    void FixedUpdate()
    {
        if (state == State.Chase && playerTransform != null)
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isWinding = true;
        attackTimer = 0f;
        // Notify blocking system so player can parry
        BlockingSystem.Instance?.NotifyIncomingAttack(attackWindup);
        // Tint red during windup to signal incoming attack
        if (sr != null) sr.color = windupColor;
        yield return new WaitForSeconds(attackWindup);
        if (sr != null) sr.color = normalColor;
        isWinding = false;
        // Only deal damage if player is still in range AND not blocking
        if (playerTransform != null &&
            Vector2.Distance(transform.position, playerTransform.position) <= attackRadius)
        {
            PlayerController player = PlayerController.LocalInstance;
            if (player != null)
            {
                bool blocked = BlockingSystem.Instance != null
                               && BlockingSystem.Instance.IsBlocking;
                if (blocked)
                {
                    HUDFeedback.Instance?.ShowInfo("Attack blocked!");
                    Debug.Log("[EnemyInfected] Attack blocked by player.");
                }
                else
                {
                    // Roll for grab attack
                    if (!isGrabbing && Random.value < grabChance)
                    {
                        grabCoroutine = StartCoroutine(GrabRoutine(player));
                    }
                    else
                    {
                        player.TakeDamage(contactDamage);
                        InfectionManager.Instance?.RegisterInfectedHit();
                        HUDFeedback.Instance?.ShowWarning($"Hit! -{contactDamage} HP — infected!");
                    }
                }
            }
        }

        // Cooldown before next attack attempt
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        attackCoroutine = null;
        if (state == State.Attack) state = State.Chase;
    }

    public void TakeDamage(float damage)
    {
        if (state == State.Dead) return;

        currentHealth -= damage;
        Debug.Log($"[EnemyInfected] Took {damage} dmg — {currentHealth}/{maxHealth} HP remaining.");

        // Flash white briefly to show damage received
        if (sr != null) StartCoroutine(DamageFlash());

        if (currentHealth <= 0f && deathCoroutine == null)
            deathCoroutine = StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        state = State.Dead;
        isAttacking = false;
        isWinding = false;

        // Stop attack coroutine via stored reference (nameof doesn't work for stored coroutines)
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // Reset sprite color in case we died mid-windup
        if (sr != null) sr.color = normalColor;

        Debug.Log($"[EnemyInfected] Died.");

        // Capture position NOW before the GO moves or is affected by anything
        Vector2 lootPosition = transform.position;

        // Fade out over deathDelay seconds (animation placeholder)
        if (sr != null)
        {
            float elapsed = 0f;
            Color start = sr.color;
            while (elapsed < deathDelay)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / deathDelay);
                sr.color = new Color(start.r, start.g, start.b, alpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(deathDelay);
        }

        QuestManager.Instance?.ReportEnemyKill(gameObject.tag);
        PersistentEnemy pe = GetComponent<PersistentEnemy>();
        Debug.Log("[EnemyInfected] PersistentEnemy component found: " + (pe != null) +
                  (pe != null ? " ID: " + pe.SceneID : " — add PersistentEnemy component!"));
        pe?.RegisterDeath();
        DropLoot(lootPosition);
        Destroy(gameObject);
    }

    private void DropLoot(Vector2 position)
    {
        Vector2 dropPos = position + Random.insideUnitCircle * 0.4f;
        Debug.Log($"[EnemyInfected] DropLoot called. " +
                  $"patheosPrefab={(patheosPrefab != null ? patheosPrefab.name : "NULL")}, " +
                  $"ammoDropPrefab={(ammoDropPrefab != null ? ammoDropPrefab.name : "NULL")}, " +
                  $"ammoDropChance={ammoDropChance}");
        // Always drop Patheos currency (random amount in range)
        if (patheosPrefab != null)
        {
            int amount = Random.Range(patheosMinAmount, patheosMaxAmount + 1);
            GameObject p = Instantiate(patheosPrefab, dropPos, Quaternion.identity);
            p.SetActive(true);
            PatheosCurrency pc = p.GetComponent<PatheosCurrency>();
            if (pc != null) pc.SetAmount(amount);
            Debug.Log($"[EnemyInfected] Spawned Patheos drop: {amount}.");
        }
        else
        {
            Debug.LogWarning("[EnemyInfected] patheosPrefab is not assigned — no currency dropped.");
        }
        // Ammo drop — single pickup with stacked count
        float roll = Random.value;
        Debug.Log($"[EnemyInfected] Ammo roll: {roll:F2} vs chance {ammoDropChance:F2}");
        if (ammoDropPrefab != null && roll <= ammoDropChance)
        {
            int ammoCount = Random.Range(ammoDropMin, ammoDropMax + 1);
            Vector2 offset = (Vector2)Random.insideUnitCircle * 0.25f;
            GameObject a = Instantiate(ammoDropPrefab, dropPos + offset, Quaternion.identity);
            a.SetActive(true);
            // Set the stack count so one pickup gives all rounds
            Item itemComp = a.GetComponent<Item>();
            if (itemComp != null) itemComp.SetWorldStackCount(ammoCount);
            Debug.Log($"[EnemyInfected] Spawned {ammoCount}x ammo as single pickup.");
        }
        else if (ammoDropPrefab == null)
        {
            Debug.LogWarning("[EnemyInfected] ammoDropPrefab is not assigned — no ammo dropped.");
        }
    }

    private IEnumerator DamageFlash()
    {
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (state != State.Dead) sr.color = original;
    }

    private IEnumerator GrabRoutine(PlayerController player)
    {
        isGrabbing = true;
        grabEscapeCounter = 0;
        player.SetMovementEnabled(false);
        //Escape window (no damage yet)
        float escapeElapsed = 0f;
        HUDFeedback.Instance?.ShowWarning(
            $"GRABBED! Mash E to escape! ({grabEscapeHits} times)");
        while (escapeElapsed < grabEscapeWindow && state != State.Dead)
        {
            escapeElapsed += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.E))
            {
                grabEscapeCounter++;
                int remaining = grabEscapeHits - grabEscapeCounter;
                if (remaining > 0)
                    HUDFeedback.Instance?.ShowWarning($"Struggling... {remaining} more!");
                if (grabEscapeCounter >= grabEscapeHits)
                {
                    // Escaped during window!
                    player.SetMovementEnabled(true);
                    isGrabbing = false;
                    grabCoroutine = null;
                    HUDFeedback.Instance?.ShowInfo("Escaped the grab!");
                    Debug.Log("[EnemyInfected] Player escaped grab.");
                    yield break;
                }
            }
            yield return null;
        }

        //Damage phase(escape still possible but harder)
        float elapsed = 0f;
        float nextDamageTick = grabDamageRate;
        HUDFeedback.Instance?.ShowWarning("Couldn't escape in time!");
        while (elapsed < grabDuration && state != State.Dead)
        {
            elapsed += Time.deltaTime;
            nextDamageTick -= Time.deltaTime;

            // Still allow escape during damage phase
            if (Input.GetKeyDown(KeyCode.E))
            {
                grabEscapeCounter++;
                if (grabEscapeCounter >= grabEscapeHits * 2)  // harder — double the hits
                {
                    player.SetMovementEnabled(true);
                    isGrabbing = false;
                    grabCoroutine = null;
                    HUDFeedback.Instance?.ShowInfo("Broke free!");
                    yield break;
                }
            }

            if (nextDamageTick <= 0f)
            {
                player.TakeDamage(grabDamage);
                InfectionManager.Instance?.RegisterInfectedHit();
                HUDFeedback.Instance?.ShowWarning($"Grabbed! -{grabDamage} HP!");
                nextDamageTick = grabDamageRate;
            }
            yield return null;
        }

        player.SetMovementEnabled(true);
        isGrabbing = false;
        grabCoroutine = null;
        Debug.Log("[EnemyInfected] Grab ended naturally.");
    }

    /// <summary>Called by SafeRoomBoundary — pushes enemy back and returns to idle.</summary>
    public void ForceRepel(Vector2 pushTarget, float pauseDuration)
    {
        if (state == State.Dead) return;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
        isAttacking = false;
        isWinding = false;
        state = State.Idle;
        StartCoroutine(RepelRoutine(pushTarget, pauseDuration));
    }

    private System.Collections.IEnumerator RepelRoutine(Vector2 pushTarget, float pause)
    {
        // Move back to push target over 0.3s
        float elapsed = 0f;
        Vector2 startPos = rb.position;
        while (elapsed < 0.3f)
        {
            elapsed += Time.fixedDeltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, pushTarget, elapsed / 0.3f));
            yield return new WaitForFixedUpdate();
        }
        // Pause at boundary
        yield return new WaitForSeconds(pause);
        // Return to idle
        state = State.Idle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}