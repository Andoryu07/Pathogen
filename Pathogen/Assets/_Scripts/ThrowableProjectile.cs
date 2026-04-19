using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThrowableProjectile : MonoBehaviour
{
    private ThrowableItem config;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float travelTime;
    private float elapsed;
    private bool landed;

    [Header("Arc Settings")]
    [SerializeField] private float arcHeight = 1.5f;

    [Header("Damage")]
    [Tooltip("Layers that can receive splash damage (Enemy + Player layers).")]
    [SerializeField] private LayerMask damageLayer;

    public void Launch(ThrowableItem cfg, Vector2 from, Vector2 to)
    {
        config = cfg;
        startPos = from;
        targetPos = to;
        float dist = Vector2.Distance(from, to);
        travelTime = dist / cfg.throwSpeed;
        elapsed = 0f;
        landed = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    void Update()
    {
        if (landed) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / travelTime);
        Vector2 linear = Vector2.Lerp(startPos, targetPos, t);
        float arc = arcHeight * Mathf.Sin(t * Mathf.PI);
        transform.position = new Vector3(linear.x, linear.y + arc, 0f);

        if (t < 1f)
        {
            Vector2 nextLinear = Vector2.Lerp(startPos, targetPos, Mathf.Clamp01((elapsed + Time.deltaTime) / travelTime));
            float nextArc = arcHeight * Mathf.Sin(Mathf.Clamp01((elapsed + Time.deltaTime) / travelTime) * Mathf.PI);
            Vector2 velocity = new Vector2(nextLinear.x, nextLinear.y + nextArc) - (Vector2)transform.position;
            if (velocity.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
        }

        if (t >= 1f)
        {
            landed = true;
            transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
                rb.linearVelocity = Vector2.zero;
            }
            StartCoroutine(TriggerAfterFuse());
        }
    }

    private IEnumerator TriggerAfterFuse()
    {
        GameObject warningRing = new GameObject("WarningRing");
        warningRing.transform.position = transform.position;
        LineRenderer lr = warningRing.AddComponent<LineRenderer>();
        lr.startWidth = 0.05f; lr.endWidth = 0.05f;
        lr.startColor = new Color(1f, 0.2f, 0.2f, 0.5f);
        lr.endColor = new Color(1f, 0.2f, 0.2f, 0.5f);
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.loop = true;
        int segments = 36;
        lr.positionCount = segments;
        lr.useWorldSpace = true;
        Vector3 center = transform.position;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            lr.SetPosition(i, center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * config.splashRadius);
        }

        yield return new WaitForSeconds(config.fuseDelay);
        Destroy(warningRing);

        if (config.throwableType == ThrowableType.PipeBomb) Explode();
        else SpawnFireZone();
        Destroy(gameObject);
    }

    private void Explode()
    {
        if (config.explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(config.explosionEffectPrefab, transform.position, Quaternion.identity);
            //Scale the explosion visual effect to match the splash radius
            fx.transform.localScale = new Vector3(config.splashRadius * 2f, config.splashRadius * 2f, 1f);
        }
        CameraShake.Instance?.Shake(0.6f, 0.6f);
        // Use layer mask so only enemies and player are hit, not walls/UI/self
        LayerMask mask = damageLayer.value != 0 ? damageLayer : Physics2D.AllLayers;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, config.splashRadius, mask);
        HashSet<GameObject> damagedRoots = new HashSet<GameObject>();

        foreach (var hit in hits)
        {
            // Use root object to prevent double-damage from multi-collider enemies
            GameObject root = hit.transform.root.gameObject;
            if (damagedRoots.Contains(root)) continue;
            damagedRoots.Add(root);

            IDamageable target = hit.GetComponent<IDamageable>()
                              ?? hit.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(config.explosionDamage);
                continue;
            }
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage(config.explosionDamage);
        }
        HUDFeedback.Instance?.ShowInfo("BOOM!");
    }

    private void SpawnFireZone()
    {
        if (config.fireZonePrefab == null) return;
        GameObject go = Instantiate(config.fireZonePrefab, transform.position, Quaternion.identity);
        FireZone zone = go.GetComponent<FireZone>();
        if (zone != null)
            zone.Initialise(config.splashRadius, config.fireDamagePerSec, config.fireDuration);
    }
}