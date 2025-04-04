using UnityEngine;

public class PathPatrol : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private Transform[] waypoints; // Массив точек пути
    [SerializeField] private float movementSpeed = 3f; // Скорость перемещения
    [SerializeField] private bool isCyclic = true; // Зациклить маршрут
    [SerializeField] private float waypointThreshold = 0.1f; // Порог достижения точки

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateTowardsMovement = true; // Поворот в сторону движения
    [SerializeField] private float rotationSpeed = 5f; // Скорость поворота

    private int currentWaypointIndex = 0;
    private bool isMovingForward = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (waypoints.Length == 0) Debug.LogError("No waypoints assigned!");
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        MoveTowardsWaypoint();
        UpdateRotation();
        CheckWaypointProximity();
    }

    private void MoveTowardsWaypoint()
    {
        Vector2 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        rb.linearVelocity = direction * movementSpeed;
    }

    private void UpdateRotation()
    {
        if (!rotateTowardsMovement) return;

        Vector2 moveDirection = rb.linearVelocity.normalized;
        if (moveDirection != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void CheckWaypointProximity()
    {
        float distance = Vector2.Distance(
            transform.position,
            waypoints[currentWaypointIndex].position
        );

        if (distance <= waypointThreshold)
        {
            GetNextWaypoint();
        }
    }

    private void GetNextWaypoint()
    {
        if (isCyclic)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            if (isMovingForward)
            {
                if (currentWaypointIndex >= waypoints.Length - 1)
                {
                    isMovingForward = false;
                    currentWaypointIndex--;
                }
                else
                {
                    currentWaypointIndex++;
                }
            }
            else
            {
                if (currentWaypointIndex <= 0)
                {
                    isMovingForward = true;
                    currentWaypointIndex++;
                }
                else
                {
                    currentWaypointIndex--;
                }
            }
        }
    }

    // Визуализация пути в редакторе
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if (isCyclic)
        {
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}