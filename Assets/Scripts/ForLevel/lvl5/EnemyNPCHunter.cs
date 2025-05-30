using UnityEngine;

public class EnemyNPCHunter : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private LayerMask obstacleLayer; // Новое поле для слоя препятствий

    [Header("Shooting Settings")]
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 15f;

    private KillableNPC currentTarget;
    private float nextAttackTime;
    private bool targetVisible;

    void Start()
    {
        nextAttackTime = Time.time;

        // Если слой препятствий не назначен, используем слой по умолчанию
        if (obstacleLayer.value == 0)
        {
            obstacleLayer = LayerMask.GetMask("Default");
        }
    }

    void FixedUpdate()
    {
        FindTarget();

        if (currentTarget != null && targetVisible && Time.time >= nextAttackTime)
        {
            ShootAtTarget();
            nextAttackTime = Time.time + attackRate;
        }
    }

    private void FindTarget()
    {
        currentTarget = null;
        targetVisible = false;

        KillableNPC[] allNPCs = FindObjectsOfType<KillableNPC>();

        foreach (KillableNPC npc in allNPCs)
        {
            if (npc.IsDead) continue;

            if (IsNPCVisible(npc))
            {
                currentTarget = npc;
                targetVisible = true;
                break;
            }
        }
    }

    private bool IsNPCVisible(KillableNPC npc)
    {
        Vector2 directionToNPC = (Vector2)npc.transform.position - (Vector2)transform.position;
        float distanceToNPC = directionToNPC.magnitude;

        if (distanceToNPC > viewDistance) return false;

        float angleToNPC = Vector2.Angle(transform.right, directionToNPC.normalized);
        if (angleToNPC > fieldOfViewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToNPC.normalized,
            distanceToNPC,
            obstacleLayer
        );

        return hit.collider == null;
    }

    private void ShootAtTarget()
    {
        if (projectilePrefab == null || shootPoint == null || currentTarget == null) return;

        Vector2 targetPosition = currentTarget.transform.position;
        Vector2 shootPosition = shootPoint.position;
        Vector2 direction = (targetPosition - shootPosition).normalized;

        float distanceToTarget = Vector2.Distance(shootPosition, targetPosition);
        RaycastHit2D hit = Physics2D.Raycast(
            shootPosition,
            direction,
            distanceToTarget,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPosition,
            Quaternion.Euler(0, 0, angle)
        );

        projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = targetVisible ? Color.red : Color.yellow;
        Vector3 origin = transform.position;
        Vector3 leftDir = Quaternion.Euler(0, 0, fieldOfViewAngle * 0.5f) * transform.right;
        Vector3 rightDir = Quaternion.Euler(0, 0, -fieldOfViewAngle * 0.5f) * transform.right;

        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
    }
}