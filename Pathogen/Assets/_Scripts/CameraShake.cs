using UnityEngine;
using System.Collections;

/// Trauma-based camera shake
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float maxAngle = 3f;    // max rotation during shake
    [SerializeField] private float maxOffset = 0.3f;  
    private Vector3 originPos;
    private Quaternion originRot;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        originPos = transform.localPosition;
        originRot = transform.localRotation;
    }

    public void Shake(float magnitude, float duration)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(magnitude, duration));
    }

    private IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Trauma decreases over time
            float trauma = 1f - (elapsed / duration);

            float offsetX = Random.Range(-1f, 1f) * maxOffset * magnitude * trauma;
            float offsetY = Random.Range(-1f, 1f) * maxOffset * magnitude * trauma;
            float angle = Random.Range(-1f, 1f) * maxAngle * magnitude * trauma;

            transform.localPosition = originPos + new Vector3(offsetX, offsetY, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }
        // Reset to origin
        transform.localPosition = originPos;
        transform.localRotation = originRot;
        shakeCoroutine = null;
    }
}