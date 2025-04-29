using UnityEngine;

public class PathPatrol : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private Transform[] waypoints; // ������ ����� ����
    [SerializeField] private float movementSpeed = 3f; // �������� �����������
    [SerializeField] private bool isCyclic = true; // ��������� �������
    [SerializeField] private float waypointThreshold = 0.1f; // ����� ���������� �����

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateTowardsMovement = true; // ������� � ������� ��������
    [SerializeField] private float rotationSpeed = 5f; // �������� ��������

    [Header("Dialogue Trigger")]
    [SerializeField] private Dialogue triggerDialogue;

    private int currentWaypointIndex = 0;
    private bool isStopped = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (waypoints.Length == 0) Debug.LogError("No waypoints assigned!");

        if (triggerDialogue != null)
        {
            isStopped = true;
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0 || isStopped) return;

        MoveTowardsWaypoint();
        UpdateRotation();
        CheckWaypointProximity();
    }

    private void OnDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == triggerDialogue)
        {
            isStopped = false;
        }
    }

    private void OnDestroy()
    {
        // ������� ��� ����������� �������
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
    }

    private void MoveTowardsWaypoint()
    {
        // ���������������, ���� �������� ����� �������������� ����
        if (!isCyclic && currentWaypointIndex >= waypoints.Length - 1)
        {
            if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) <= waypointThreshold)
            {
                rb.linearVelocity = Vector2.zero;
                isStopped = true;
                return;
            }
        }

        Vector2 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        rb.linearVelocity = direction * movementSpeed;
    }

    private void UpdateRotation()
    {
        if (!rotateTowardsMovement || rb.linearVelocity == Vector2.zero) return;

        Vector2 moveDirection = rb.linearVelocity.normalized;
        float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            rotationSpeed * Time.deltaTime
        );
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
            if (currentWaypointIndex < waypoints.Length - 1)
            {
                currentWaypointIndex++;
            }
        }
    }

    // ������������ ���� � ���������
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