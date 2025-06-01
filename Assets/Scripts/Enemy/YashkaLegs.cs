using UnityEngine;

public class YashkaLegs : MonoBehaviour
{
    private Yashka yashka;
    private Rigidbody2D rb;
    private Animator animator;
    private float targetLegsRotation;
    private float rotationSpeed = 15f; 

    private void Start()
    {
        yashka = GetComponentInParent<Yashka>();
        rb = yashka.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetLegsRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        if (yashka == null || animator == null || rb == null) return;

        // ѕровер€ем состо€ни€, когда ноги не должны двигатьс€
        bool shouldMoveLegs = !yashka.IsAttacking && !yashka.IsDodging;

        bool isMoving = shouldMoveLegs &&
                       rb.linearVelocity.magnitude > 0.1f;

        animator.SetBool("IsMoving", isMoving);

        float parentRotation = yashka.transform.eulerAngles.z;

        if (isMoving)
        {
            Vector2 movementDirection = rb.linearVelocity.normalized;
            float movementAngle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            float angleDifference = Mathf.DeltaAngle(parentRotation, movementAngle);

            targetLegsRotation = Mathf.Abs(angleDifference) <= 70f
                ? movementAngle
                : parentRotation;
        }
        else if (shouldMoveLegs)
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