using UnityEngine;
using System.Collections;

/// In-flight throwable projectile. Travels in a parabolic arc to target position, then triggers either explosion (pipe bomb) or fire zone (molotov)
public class ThrowableProjectile : MonoBehaviour
{
    private ThrowableItem config;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float travelTime;
    private float elapsed;
    private bool landed;

    [Header("Arc Settings")]
    [SerializeField] private float arcHeight = 1.5f;   // world units above midpoint

    public void Launch(ThrowableItem cfg, Vector2 from, Vector2 to)
    {
        config = cfg;
        startPos = from;
        targetPos = to;
        float dist = Vector2.Distance(from, to);
        travelTime = dist / cfg.throwSpeed;
        elapsed = 0f;
        landed = false;
    }

    void Update()
    {
        if (landed) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / travelTime);
        // Lerp position along arc
        Vector2 linear = Vector2.Lerp(startPos, targetPos, t);
        float arc = arcHeight * Mathf.Sin(t * Mathf.PI);
        transform.position = new Vector3(linear.x, linear.y + arc, 0f);

        // Rotate to follow arc direction
        if (t < 1f)
        {
            Vector2 nextLinear = Vector2.Lerp(startPos, targetPos,
                                              Mathf.Clamp01((elapsed + Time.deltaTime) / travelTime));
            float nextArc = arcHeight * Mathf.Sin(Mathf.Clamp01((elapsed + Time.deltaTime)
                                                                     / travelTime) * Mathf.PI);
            Vector2 velocity = new Vector2(nextLinear.x, nextLinear.y + nextArc)
                                - (Vector2)transform.position;
            if (velocity.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Euler(0, 0,
                    Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
        }

        // Landing
        if (t >= 1f)
        {
            landed = true;
            transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
            StartCoroutine(TriggerAfterFuse());
        }
    }

    private IEnumerator TriggerAfterFuse()
    {
        yield return new WaitForSeconds(config.fuseDelay);

        if (config.throwableType == ThrowableType.PipeBomb)
            Explode();
        else
            SpawnFireZone();

        Destroy(gameObject);
    }

    private void Explode()
    {
        if (config.explosionEffectPrefab != null)
            Instantiate(config.explosionEffectPrefab, transform.position, Quaternion.identity);

        CameraShake.Instance?.Shake(0.3f,0.1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, config.splashRadius);
        foreach (var hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null) target.TakeDamage(config.explosionDamage);

            // Damage player too
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(config.explosionDamage);
        }

        HUDFeedback.Instance?.ShowInfo("BOOM!");
    }

    private void SpawnFireZone()
    {
        if (config.fireZonePrefab == null) return;
        GameObject go = Instantiate(config.fireZonePrefab,
                                       transform.position, Quaternion.identity);
        FireZone zone = go.GetComponent<FireZone>();
        if (zone != null)
            zone.Initialise(config.splashRadius, config.fireDamagePerSec,
                            config.fireDuration);
    }
}