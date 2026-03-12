using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform targetToFollow; // What should the camera follow
    [SerializeField] private float smoothTime = 0.3f;  // How smoothly does the camera follow selected object
    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component is missing!", this);
        }
    }
    //Runs after everything else has moved
    void LateUpdate()
    {
        if (targetToFollow != null)
        {
            Vector3 targetPosition = new Vector3(targetToFollow.position.x, targetToFollow.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    public void SetTarget(Transform target)
    {
        targetToFollow = target;
    }
}

