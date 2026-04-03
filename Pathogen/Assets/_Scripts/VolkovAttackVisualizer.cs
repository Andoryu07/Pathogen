using UnityEngine;
/// Draws attack area previews for Volkov during windup
public class VolkovAttackVisualizer : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color swingColor = new Color(1f, 0.4f, 0f, 0.5f);
    [SerializeField] private Color stumpColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color tentacleColor = new Color(0.6f, 0f, 1f, 0.5f);
    [SerializeField] private float lineWidth = 0.15f;

    private LineRenderer swingArc;
    private LineRenderer[] stumpLines = new LineRenderer[4];
    private LineRenderer[] tentLines = new LineRenderer[3];

    void Awake()
    {
        swingArc = CreateLine("SwingArc", swingColor, 32);

        for (int i = 0; i < 4; i++)
            stumpLines[i] = CreateLine("StumpLine_" + i, stumpColor, 2);

        for (int i = 0; i < 3; i++)
            tentLines[i] = CreateLine("TentacleLine_" + i, tentacleColor, 2);

        HideAll();
    }

    public void ShowSwing(float radius, float angleDeg, Vector2 direction)
    {
        HideAll();
        DrawArc(swingArc, radius, angleDeg, direction);
        swingArc.gameObject.SetActive(true);
    }

    public void ShowStump(float range)
    {
        HideAll();
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        for (int i = 0; i < 4; i++)
        {
            DrawLine(stumpLines[i], Vector2.zero, dirs[i] * range);
            stumpLines[i].gameObject.SetActive(true);
        }
    }

    public void ShowTentacle(float range, Vector2 baseDir, float spreadDeg)
    {
        HideAll();
        float[] angles = { 0f, spreadDeg, -spreadDeg };
        for (int i = 0; i < 3; i++)
        {
            Vector2 dir = RotateVector(baseDir, angles[i]);
            DrawLine(tentLines[i], Vector2.zero, dir * range);
            tentLines[i].gameObject.SetActive(true);
        }
    }

    public void HideAll()
    {
        if (swingArc != null) swingArc.gameObject.SetActive(false);
        foreach (var l in stumpLines) if (l != null) l.gameObject.SetActive(false);
        foreach (var l in tentLines) if (l != null) l.gameObject.SetActive(false);
    }

    private void DrawArc(LineRenderer lr, float radius, float angleDeg, Vector2 dir)
    {
        int points = lr.positionCount;
        float startAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - angleDeg * 0.5f;

        for (int i = 0; i < points; i++)
        {
            float angle = (startAngle + angleDeg * i / (points - 1)) * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius,
                                          Mathf.Sin(angle) * radius, 0f));
        }
    }

    private void DrawLine(LineRenderer lr, Vector2 from, Vector2 to)
    {
        lr.SetPosition(0, new Vector3(from.x, from.y, 0f));
        lr.SetPosition(1, new Vector3(to.x, to.y, 0f));
    }

    private LineRenderer CreateLine(string name, Color color, int pointCount)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = pointCount;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.loop = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, 0f); // fade out at end
        lr.sortingOrder = 10;
        return lr;
    }

    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            Mathf.Cos(rad) * v.x - Mathf.Sin(rad) * v.y,
            Mathf.Sin(rad) * v.x + Mathf.Cos(rad) * v.y);
    }
}