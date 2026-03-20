using UnityEngine;
using UnityEngine.UI;
/// Manages the crosshair cursor on the UI canvas
public class CrosshairUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Image crosshairImage;
    [Header("Colors")]
    [SerializeField] private Color colorNormal = new Color(1.00f, 1.00f, 1.00f, 0.90f);
    [SerializeField] private Color colorEnemy = new Color(0.95f, 0.15f, 0.15f, 0.95f);

    private Canvas parentCanvas;
    private bool enemyTargeted = false;

    void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (crosshairImage != null)
        {
            crosshairImage.raycastTarget = false;
            crosshairImage.color = colorNormal;
        }
        gameObject.SetActive(false);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void MoveTo(Vector2 screenPosition)
    {
        if (crosshairRect == null || parentCanvas == null) return;
        float scale = parentCanvas.scaleFactor;
        crosshairRect.anchoredPosition = new Vector2(
            screenPosition.x / scale,
            screenPosition.y / scale);
    }

    public void SetEnemyTarget(bool onEnemy)
    {
        if (enemyTargeted == onEnemy) return;
        enemyTargeted = onEnemy;
        if (crosshairImage != null)
            crosshairImage.color = onEnemy ? colorEnemy : colorNormal;
    }
}