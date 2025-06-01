using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [Header("Настройки слежения")]
    public Transform target; 
    public float viewRadius = 5f; 

    [Header("Настройки поворота")]
    public float rotationSpeed = 5f; 
    public bool freezeZRotation = true; 
    public bool lookRight = true; 

    private Vector3 defaultDirection;

    void Start()
    {
        defaultDirection = transform.right * (lookRight ? 1 : -1);
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);
        bool shouldLookAtTarget = distance <= viewRadius;

        Vector3 lookDirection = shouldLookAtTarget ?
            (target.position - transform.position).normalized :
            defaultDirection;

        float targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(
            freezeZRotation ? 0 : transform.rotation.eulerAngles.x,
            freezeZRotation ? 0 : transform.rotation.eulerAngles.y,
            targetAngle
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}