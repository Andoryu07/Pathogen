using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireZone : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color fireColor = new Color(1f, 0.4f, 0f, 0.6f);

    private float damagePerSec;
    private float duration;
    private float radius;
    private HashSet<IDamageable> targets = new HashSet<IDamageable>();
    private PlayerController playerInFire;

    public void Initialise(float r, float dps, float dur)
    {
        radius = r;
        damagePerSec = dps;
        duration = dur;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null) col.radius = 0.5f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        // Generate circle sprite if none assigned
        if (sr.sprite == null) sr.sprite = BuildCircleSprite(64);

        sr.color = fireColor;
        sr.sortingOrder = 10;  // render above ground and items

        float diameter = radius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);
        StartCoroutine(BurnRoutine());
    }

    private static Sprite BuildCircleSprite(int resolution)
    {
        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = resolution * 0.5f;
        float rSq = center * center;
        for (int x = 0; x < resolution; x++)
            for (int y = 0; y < resolution; y++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                tex.SetPixel(x, y, (dx * dx + dy * dy) <= rSq ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
                             new Vector2(0.5f, 0.5f), resolution);
    }

    private IEnumerator BurnRoutine()
    {
        float elapsed = 0f;
        HUDFeedback.Instance?.ShowWarning("Fire! Avoid the flames!");
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            List<IDamageable> toRemove = new List<IDamageable>();

            foreach (var t in targets)
            {
                if (t == null || t.Equals(null))
                {
                    toRemove.Add(t);
                }
                else
                {
                    t.TakeDamage(damagePerSec * Time.deltaTime);
                }
            }

            foreach (var deadTarget in toRemove)
                targets.Remove(deadTarget);

            if (playerInFire != null && playerInFire.CurrentHealth > 0f)
            {
                playerInFire.TakeDamage(damagePerSec * Time.deltaTime);
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null) { targets.Add(dmg); return; }

        PlayerController pl = other.GetComponent<PlayerController>();
        if (pl != null) playerInFire = pl;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null) { targets.Remove(dmg); return; }

        PlayerController pl = other.GetComponent<PlayerController>();
        if (pl != null) playerInFire = null;
    }
}