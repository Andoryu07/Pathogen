using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// Volkov — final boss.
/// Phase 1 (100-66% HP): Swing only
/// Phase 2 (66-33% HP):  Swing + Stump
/// Phase 3 (33-0% HP):   Swing + Stump + Tentacle
[RequireComponent(typeof(Rigidbody2D))]
public class VolkovBoss : MonoBehaviour, IDamageable
{

    [Header("Stats")]
    [SerializeField] private float maxHealth = 2000f;
    [SerializeField] private float moveSpeed = 0.8f;
    [SerializeField] private float detectionRadius = 12f;
    [Header("Swing Attack")]
    [SerializeField] private float swingDamage = 60f;
    [SerializeField] private float swingRadius = 3f;
    [SerializeField] private float swingAngle = 150f;
    [SerializeField] private float swingWindup = 2.0f;
    [SerializeField] private float swingCooldown = 3.0f;
    [SerializeField] private LayerMask playerLayer;
    [Header("Stump Attack")]
    [SerializeField] private float stumpDamage = 45f;
    [SerializeField] private float stumpWindup = 2.5f;
    [SerializeField] private float stumpCooldown = 4.0f;
    [SerializeField] private float stumpFlashTime = 0.8f;   // warning flash before projectile
    [SerializeField] private float stumpProjSpeed = 5f;
    [SerializeField] private float stumpProjRange = 10f;
    [SerializeField] private GameObject stumpProjPrefab;
    [Header("Tentacle Attack")]
    [SerializeField] private float tentacleDamage = 35f;
    [SerializeField] private float tentacleWindup = 3.0f;
    [SerializeField] private float tentacleCooldown = 5.0f;
    [SerializeField] private float tentacleSpeed = 7f;
    [SerializeField] private float tentacleRange = 12f;
    [SerializeField] private float tentacleSpread = 30f;    // degrees between tentacles
    [SerializeField] private GameObject tentaclePrefab;
    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color windupColor = new Color(1f, 0.3f, 0.1f, 1f);
    [SerializeField] private Color phase2Color = new Color(1f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color phase3Color = new Color(0.8f, 0.1f, 0.1f, 1f);
    [SerializeField] private float deathDelay = 2.0f;

    public float CurrentHealth => currentHealth;
    public void SetCurrentHealth(float hp) => currentHealth = Mathf.Clamp(hp, 0f, maxHealth);
    private VolkovAttackVisualizer visualizer;

    [Header("Loot")]
    [SerializeField] private GameObject patheosPrefab;
    [SerializeField] private int patheosReward = 5000;
    [SerializeField] private List<GameObject> keyItemDrops = new List<GameObject>();
    private const float Phase2Threshold = 0.66f;
    private const float Phase3Threshold = 0.33f;
    private enum State { Idle, Chase, Attacking, Dead }
    private enum Phase { One, Two, Three }
    private State state = State.Idle;
    private Phase currentPhase = Phase.One;
    private float currentHealth;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    private Coroutine deathCoroutine;
    private float attackTimer = 0f;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (DifficultyManager.Instance != null)
        {
            maxHealth *= DifficultyManager.Instance.HealthMultiplier;
            swingDamage *= DifficultyManager.Instance.DamageMultiplier;
            stumpDamage *= DifficultyManager.Instance.DamageMultiplier;
            tentacleDamage *= DifficultyManager.Instance.DamageMultiplier;
            currentHealth = maxHealth;
        }
        visualizer = GetComponent<VolkovAttackVisualizer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    void Update()
    {
        if (state == State.Dead) return;
        if (playerTransform == null) return;

        UpdatePhase();

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        switch (state)
        {
            case State.Idle:
                if (dist <= detectionRadius)
                {
                    state = State.Chase;
                    AudioManager.Instance?.PlayBruteSpot();
                }
                break;

            case State.Chase:
                if (!isAttacking)
                {
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0f)
                        ChooseAttack(dist);
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (state != State.Chase || isAttacking || playerTransform == null) return;
        MoveToward(playerTransform.position);
    }

    private void UpdatePhase()
    {
        float ratio = currentHealth / maxHealth;
        Phase newPhase = ratio > Phase2Threshold ? Phase.One :
                         ratio > Phase3Threshold ? Phase.Two : Phase.Three;

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged(currentPhase);
        }
    }

    private void OnPhaseChanged(Phase phase)
    {
        switch (phase)
        {
            case Phase.Two:
                if (sr != null) normalColor = phase2Color;
                HUDFeedback.Instance?.ShowWarning("Volkov is enraged!");
                //CameraShake.Instance?.Shake(0.6f);
                break;
            case Phase.Three:
                if (sr != null) normalColor = phase3Color;
                HUDFeedback.Instance?.ShowWarning("Volkov is unleashing his full power!");
                //CameraShake.Instance?.Shake(0.8f);
                break;
        }
        if (sr != null) sr.color = normalColor;
    }

    private void MoveToward(Vector2 target)
    {
        Vector2 dir = ((Vector2)target - rb.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    private void ChooseAttack(float dist)
    {
        // Build available attacks for current phase
        var available = new List<System.Action>();
        available.Add(StartSwing);
        if (currentPhase >= Phase.Two) available.Add(StartStump);
        if (currentPhase >= Phase.Three) available.Add(StartTentacle);

        // Pick random
        int idx = Random.Range(0, available.Count);
        available[idx]();
    }
    private void StartSwing()
    {
        isAttacking = true;
        attackCoroutine = StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        // Windup
        BlockingSystem.Instance?.NotifyIncomingAttack(swingWindup);
        AudioManager.Instance?.PlayBruteWindup();
        if (sr != null) sr.color = windupColor;
        if (playerTransform != null)
        {
            Vector2 previewDir = ((Vector2)playerTransform.position - rb.position).normalized;
            visualizer?.ShowSwing(swingRadius, swingAngle, previewDir);
        }
        yield return new WaitForSeconds(swingWindup);
        visualizer?.HideAll();
        if (sr != null) sr.color = normalColor;
        // Hit check — arc in player direction
        if (playerTransform != null)
        {
            Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
            float halfAngle = swingAngle * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, swingRadius, playerLayer);
            foreach (var hit in hits)
            {
                Vector2 toTarget = ((Vector2)hit.transform.position - rb.position).normalized;
                if (Vector2.Angle(dir, toTarget) <= halfAngle)
                {
                    bool blocked = BlockingSystem.Instance != null && BlockingSystem.Instance.IsBlocking;
                    if (blocked)
                        HUDFeedback.Instance?.ShowInfo("Swing blocked!");
                    else
                    {
                        PlayerController.LocalInstance?.TakeDamage(swingDamage);
                        HUDFeedback.Instance?.ShowWarning($"Volkov swings! -{swingDamage} HP!");
                    }
                }
            }
        }
        //CameraShake.Instance?.Shake(0.5f);
        AudioManager.Instance?.PlayBruteAttack();
        yield return new WaitForSeconds(swingCooldown);
        isAttacking = false;
        attackCoroutine = null;
        attackTimer = 0f;
        state = State.Chase;
    }

    private void StartStump()
    {
        isAttacking = true;
        attackCoroutine = StartCoroutine(StumpRoutine());
    }

    private IEnumerator StumpRoutine()
    {
        BlockingSystem.Instance?.NotifyIncomingAttack(stumpWindup);
        AudioManager.Instance?.PlayBruteWindup();
        if (sr != null) sr.color = windupColor;
        visualizer?.ShowStump(stumpProjRange);
        StartCoroutine(FlashStumpWarning());
        yield return new WaitForSeconds(stumpWindup);
        visualizer?.HideAll();
        if (sr != null) sr.color = normalColor;
        //CameraShake.Instance?.Shake(0.7f);
        AudioManager.Instance?.PlayBruteAttack();
        // Fire 4 projectiles in + shape
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in dirs)
            SpawnProjectile(stumpProjPrefab, dir, stumpProjSpeed, stumpProjRange, stumpDamage);
        yield return new WaitForSeconds(stumpCooldown);
        isAttacking = false;
        attackCoroutine = null;
        attackTimer = 0f;
        state = State.Chase;
    }
    private IEnumerator FlashStumpWarning()
    {
        // Visual warning — pulse color quickly to indicate + shape danger
        float elapsed = 0f;
        while (elapsed < stumpFlashTime)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
                sr.color = Color.Lerp(windupColor, Color.yellow,
                    Mathf.PingPong(elapsed * 6f, 1f));
            yield return null;
        }
    }
    private void StartTentacle()
    {
        isAttacking = true;
        attackCoroutine = StartCoroutine(TentacleRoutine());
    }

    private IEnumerator TentacleRoutine()
    {
        BlockingSystem.Instance?.NotifyIncomingAttack(tentacleWindup);
        AudioManager.Instance?.PlayBruteWindup();
        if (sr != null) sr.color = windupColor;
        if (playerTransform != null)
        {
            Vector2 tentDir = ((Vector2)playerTransform.position - rb.position).normalized;
            visualizer?.ShowTentacle(tentacleRange, tentDir, tentacleSpread);
        }
        yield return new WaitForSeconds(tentacleWindup);
        visualizer?.HideAll();
        if (sr != null) sr.color = normalColor;
        AudioManager.Instance?.PlayBruteAttack();
        // Fire 3 tentacles — center aimed at player, ±spread degrees
        if (playerTransform != null)
        {
            Vector2 baseDir = ((Vector2)playerTransform.position - rb.position).normalized;
            float[] angles = { 0f, tentacleSpread, -tentacleSpread };

            foreach (float angle in angles)
            {
                Vector2 dir = RotateVector(baseDir, angle);
                SpawnProjectile(tentaclePrefab, dir, tentacleSpeed, tentacleRange, tentacleDamage);
            }
        }
        yield return new WaitForSeconds(tentacleCooldown);
        isAttacking = false;
        attackCoroutine = null;
        attackTimer = 0f;
        state = State.Chase;
    }

    private void SpawnProjectile(GameObject prefab, Vector2 dir, float speed,
                                  float range, float damage)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        VolkovProjectile proj = go.GetComponent<VolkovProjectile>();
        if (proj != null)
            proj.Launch(dir, speed, range, damage);
        else
            Destroy(go);
    }
    public void TakeDamage(float damage)
    {
        if (state == State.Dead) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        if (sr != null) StartCoroutine(DamageFlash());

        HUDFeedback.Instance?.ShowInfo(
            $"Volkov: {Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)} HP");

        if (currentHealth <= 0f && deathCoroutine == null)
            deathCoroutine = StartCoroutine(DieRoutine());
    }

    private IEnumerator DamageFlash()
    {
        if (sr == null) yield break;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = normalColor;
    }

    private IEnumerator DieRoutine()
    {
        state = State.Dead;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
        visualizer?.HideAll();
        if (sr != null) sr.color = Color.white;
        //CameraShake.Instance?.Shake(1.0f);
        AudioManager.Instance?.PlayBruteDeath();

        HUDFeedback.Instance?.ShowInfo("Volkov has been defeated!");

        yield return new WaitForSeconds(deathDelay);

        DropRewards();
        GetComponent<PersistentEnemy>()?.RegisterDeath();
        Destroy(gameObject);
    }

    private void DropRewards()
    {
        // Large Patheos reward
        if (patheosPrefab != null)
        {
            GameObject go = Instantiate(patheosPrefab, transform.position, Quaternion.identity);
            PatheosCurrency pc = go.GetComponent<PatheosCurrency>();
            if (pc != null) pc.SetAmount(patheosReward);
        }

        // Key item drops — scattered around death position
        for (int i = 0; i < keyItemDrops.Count; i++)
        {
            if (keyItemDrops[i] == null) continue;
            Vector2 offset = Random.insideUnitCircle * 1.5f;
            GameObject go = Instantiate(keyItemDrops[i],
                (Vector2)transform.position + offset,
                Quaternion.identity);
            go.SetActive(true);
        }
    }

    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, swingRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}