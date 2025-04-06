using UnityEngine;

public class EnemyLegs : MonoBehaviour
{
    private EnemyAI enemyAI;
    private Rigidbody2D rb;
    private Animator animator;
    private float targetLegsRotation;
    private float rotationSpeed = 10f;

    private void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        rb = enemyAI.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetLegsRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        if (enemyAI == null || animator == null || rb == null) return;

        bool isMoving = rb.linearVelocity.magnitude > 0.1f && !enemyAI.IsStatic;
        animator.SetBool("IsMoving", isMoving);

        float parentRotation = enemyAI.transform.eulerAngles.z;

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