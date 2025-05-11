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
        camComponent.enabled = false;

        GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamObj != null)
        {
            mainCamera = mainCamObj.GetComponent<Camera>();
            mainCamera.enabled = true;
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }
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
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }
        camComponent.enabled = true;
    }

    public void Deactivate()
    {
        camComponent.enabled = false;
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }
        target = null;
    }
}