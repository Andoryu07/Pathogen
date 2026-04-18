using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Placed by a Molotov on landing. Deals periodic damage to all IDamageable objects that enter or stay inside the radius. Destroys itself after duration
public class FireZone : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color fireColor = new Color(1f, 0.4f, 0f, 0.6f);

    private float damagePerSec;
    private float duration;
    private float radius;
    private HashSet<IDamageable> targets = new HashSet<IDamageable>();
    private HashSet<PlayerController> playerInFire = new HashSet<PlayerController>();

    public void Initialise(float r, float dps, float dur)
    {
        radius = r;
        damagePerSec = dps;
        duration = dur;
        // Scale collider and sprite to match radius
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null) col.radius = radius;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = fireColor;
            sr.sortingOrder = 1;
        }
        float diameter = radius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        float elapsed = 0f;
        HUDFeedback.Instance?.ShowWarning("Fire! Avoid the flames!");
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Damage all current targets
            foreach (var t in new List<IDamageable>(targets))
                t?.TakeDamage(damagePerSec * Time.deltaTime);

            // Player warning
            foreach (var p in new List<PlayerController>(playerInFire))
                if (p != null)
                    HUDFeedback.Instance?.ShowWarning(
                        $"Fire damage! -{Mathf.RoundToInt(damagePerSec * Time.deltaTime)} HP!");

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        PlayerController pl = other.GetComponent<PlayerController>();

        if (dmg != null) targets.Add(dmg);
        if (pl != null) playerInFire.Add(pl);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        PlayerController pl = other.GetComponent<PlayerController>();

        if (dmg != null) targets.Remove(dmg);
        if (pl != null) playerInFire.Remove(pl);
    }
}