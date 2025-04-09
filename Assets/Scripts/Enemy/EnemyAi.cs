using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private int health = 1;
    [SerializeField] private GameObject redPuddlePrefab;

    [Header("Vision Settings")]
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private bool drawGizmos = true;

    [Header("Movement Settings")]
    [SerializeField] private bool isStatic = false;
    [SerializeField] private bool isHostile = true;
    [SerializeField] private float wanderRadius = 3f;   
    [SerializeField] private float wanderDelay = 2f;

    [Header("Speed Settings")]
    public float chaseSpeed = 3f;   
    public float wanderSpeed = 1f;    
    public float rotationSpeed = 10f;
    public float stoppingDistance = 1f;

    public bool IsStatic => isStatic;
    public bool IsHostile => isHostile;
    public bool PlayerVisible { get; private set; }

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 initialPosition;    
    private Vector2 currentWanderTarget; 
    private float wanderTimer;           

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialPosition = transform.position;
        SetNewWanderTarget();

        if (isStatic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            GetComponent<Collider2D>().isTrigger = true;
        }
    }

    void FixedUpdate()
    {
        if (isHostile)
        {
            CheckPlayerVisibility();
            UpdateHostileBehavior();
        }
        else if (!isStatic) 
        {
            WanderAround();
        }
    }

    private void CheckPlayerVisibility()
    {
        if (player == null)
        {
            PlayerVisible = false;
            return;
        }

        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector2.Angle(transform.right, directionToPlayer);

        PlayerVisible = distanceToPlayer <= viewDistance &&
                      angleToPlayer < fieldOfViewAngle * 0.5f;
    }

    private void UpdateHostileBehavior()
    {
        if (player == null) return;

        if (PlayerVisible)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            RotateTowards(direction);

            if (!isStatic)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance < stoppingDistance)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    rb.linearVelocity = direction * chaseSpeed;
                }
            }
        }
        else if (!isStatic)
        {
            WanderAround();
        }
    }

    private void WanderAround()
    {
        wanderTimer -= Time.fixedDeltaTime;

        if (Vector2.Distance(transform.position, currentWanderTarget) < 0.1f || wanderTimer <= 0)
        {
            SetNewWanderTarget();
            wanderTimer = wanderDelay;
        }

        Vector2 direction = (currentWanderTarget - (Vector2)transform.position).normalized;
        RotateTowards(direction);
        rb.linearVelocity = direction * wanderSpeed;
    }

    private void SetNewWanderTarget()
    {
        currentWanderTarget = initialPosition + Random.insideUnitCircle * wanderRadius;
    }

    private void RotateTowards(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, 0, targetAngle),
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    private void Die()
    {
        if (redPuddlePrefab != null)
        {
            Instantiate(redPuddlePrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !Application.isPlaying) return;

        Gizmos.color = PlayerVisible ? Color.red : Color.yellow;
        Vector3 origin = transform.position;
        Vector3 leftDir = Quaternion.Euler(0, 0, fieldOfViewAngle * 0.5f) * transform.right;
        Vector3 rightDir = Quaternion.Euler(0, 0, -fieldOfViewAngle * 0.5f) * transform.right;

        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
        Gizmos.DrawWireSphere(origin, viewDistance);
    }
}