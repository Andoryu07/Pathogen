using UnityEngine;
using UnityEngine.UI;

/// Manages the crosshair cursor on the UI canvas
/// Follows the mouse when aiming. Turns red when over an enemy
public class CrosshairUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Image crosshairImage;
    [Header("Crosshair Appearance")]
    [SerializeField] private Color colorNormal = new Color(1.00f, 1.00f, 1.00f, 0.90f);
    [SerializeField] private Color colorEnemy = new Color(0.95f, 0.15f, 0.15f, 0.95f);
    [SerializeField] private float size = 32f;
    [Header("Custom Sprite (optional)")]
    [Tooltip("Leave empty to use the built-in procedural crosshair.")]
    [SerializeField] private Sprite crosshairSprite;

    private Canvas parentCanvas;
    private bool enemyTargeted = false;
    void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (crosshairRect != null)
            crosshairRect.sizeDelta = new Vector2(size, size);
        if (crosshairImage != null)
        {
            crosshairImage.raycastTarget = false;
            if (crosshairSprite != null)
                crosshairImage.sprite = crosshairSprite;
            else
                crosshairImage.sprite = BuildCrosshairTexture();
        }

        gameObject.SetActive(false);
    }
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
    ///Move crosshair to screen position (pixels)
    public void MoveTo(Vector2 screenPosition)
    {
        if (crosshairRect == null || parentCanvas == null) return;
        float scale = parentCanvas.scaleFactor;
        crosshairRect.anchoredPosition = new Vector2(
            screenPosition.x / scale,
            screenPosition.y / scale
        );
    }

    ///Pass true when cursor is over an enemy — turns crosshair red
    public void SetEnemyTarget(bool onEnemy)
    {
        if (enemyTargeted == onEnemy) return;
        enemyTargeted = onEnemy;
        if (crosshairImage != null)
            crosshairImage.color = onEnemy ? colorEnemy : colorNormal;
    }

    private Sprite BuildCrosshairTexture()
    {
        int res = 32;
        int thick = 2;                    // line thickness in pixels
        int gapHalf = 4;                    // gap around centre
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        // Fill transparent
        Color clear = Color.clear;
        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
                tex.SetPixel(x, y, clear);

        int c = res / 2;

        // Horizontal arms
        for (int x = 0; x < res; x++)
        {
            if (x >= c - gapHalf && x <= c + gapHalf) continue;
            for (int t = -thick / 2; t <= thick / 2; t++)
            {
                int py = c + t;
                if (py >= 0 && py < res) tex.SetPixel(x, py, Color.white);
            }
        }

        // Vertical arms
        for (int y = 0; y < res; y++)
        {
            if (y >= c - gapHalf && y <= c + gapHalf) continue;
            for (int t = -thick / 2; t <= thick / 2; t++)
            {
                int px = c + t;
                if (px >= 0 && px < res) tex.SetPixel(px, y, Color.white);
            }
        }
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, res, res),
            new Vector2(0.5f, 0.5f),
            res);
    }
}