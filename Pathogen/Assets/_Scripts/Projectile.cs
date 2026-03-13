using UnityEngine;
/// Fired by AimSystem toward the cursor. Travels in a straight line, damages the first IDamageable it hits, then destroys itself
public class Projectile : MonoBehaviour
{
    // Set by AimSystem.FireProjectile() — no Inspector wiring needed
    [HideInInspector] public float damage;
    [HideInInspector] public float range;
    [HideInInspector] public LayerMask enemyLayer;
    [Header("Visuals")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private Color bulletColor = new Color(1f, 0.95f, 0.4f, 1f); // yellow

    private Vector2 direction;
    private Vector2 origin;
    private float travelledDistance = 0f;
    private bool hit = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = BuildCircleSprite(8);
            sr.color = bulletColor;
            transform.localScale = new Vector3(0.12f, 0.12f, 1f);
        }
    }

    ///Called immediately after instantiation by AimSystem
    public void Launch(Vector2 dir, float spd = -1f)
    {
        direction = dir.normalized;
        origin = transform.position;
        if (spd > 0f) speed = spd;
        if (rb != null) rb.linearVelocity = direction * speed;
    }

    void Update()
    {
        if (hit) return;

        travelledDistance += speed * Time.deltaTime;
        if (travelledDistance >= range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hit) return;

        // Accept hit if: layer matches the enemy layer mask, OR tagged "Enemy"
        bool layerMatch = enemyLayer.value != 0 && (enemyLayer.value & (1 << other.gameObject.layer)) != 0;
        bool tagMatch = other.CompareTag("Enemy");
        if (!layerMatch && !tagMatch) return;
        hit = true;
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log($"[Projectile] Hit {other.name} for {damage} dmg.");
        }
        else
        {
            Debug.Log($"[Projectile] Hit {other.name} but no IDamageable found.");
        }

        Destroy(gameObject);
    }

    private static Sprite BuildCircleSprite(int radius)
    {
        int size = radius * 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = Color.clear;
        Color white = Color.white;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                tex.SetPixel(x, y, (dx * dx + dy * dy) <= radius * radius ? white : clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}