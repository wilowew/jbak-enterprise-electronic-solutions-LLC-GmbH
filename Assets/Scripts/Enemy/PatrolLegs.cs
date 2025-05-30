using UnityEngine;

public class PatrolLegs : MonoBehaviour
{
    private PathPatrol pathPatrol;
    private Rigidbody2D rb;
    private Animator animator;
    private float targetLegsRotation;
    private float rotationSpeed = 10f;

    private void Start()
    {
        pathPatrol = GetComponentInParent<PathPatrol>();
        rb = GetComponentInParent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetLegsRotation = transform.eulerAngles.z;

        if (pathPatrol == null)
        {
            Debug.LogError("PathPatrol component not found in parent!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (pathPatrol == null || animator == null || rb == null) return;

        bool isMoving = rb.linearVelocity.magnitude > 0.1f && !pathPatrol.IsStopped;
        animator.SetBool("IsMoving", isMoving);

        float parentRotation = transform.parent.eulerAngles.z;

        if (isMoving)
        {
            Vector2 movementDirection = rb.linearVelocity.normalized;
            float movementAngle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            float angleDifference = Mathf.DeltaAngle(parentRotation, movementAngle);

            targetLegsRotation = Mathf.Abs(angleDifference) <= 70f
                ? movementAngle
                : parentRotation;
        }
        else
        {
            float angleDifference = Mathf.DeltaAngle(targetLegsRotation, parentRotation);
            if (Mathf.Abs(angleDifference) > 70f)
            {
                targetLegsRotation = parentRotation;
            }
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, 0, targetLegsRotation),
            Time.deltaTime * rotationSpeed
        );
    }
}