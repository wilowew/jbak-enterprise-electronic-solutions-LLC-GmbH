using UnityEngine;

public class CutsceneCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cameraZ = -10f;

    [Header("Debug")]
    [SerializeField] private Transform target;

    private Camera camComponent;
    private Camera mainCamera;

    private void Awake()
    {
        camComponent = GetComponent<Camera>();
        mainCamera = Camera.main;
        Deactivate();
    }

    private void LateUpdate()
    {
        if (target != null && camComponent.enabled)
        {
            Vector3 newPosition = target.position;
            newPosition.z = cameraZ;
            transform.position = newPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void Activate()
    {
        mainCamera.enabled = false;
        camComponent.enabled = true;
    }

    public void Deactivate()
    {
        camComponent.enabled = false;
        mainCamera.enabled = true;
    }
}