using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum WeaponType { Melee, Ranged }

    [Header("Weapon Settings")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField][Range(0f, 1f)] private float missChance = 0.3f;

    [Header("Combat Settings")]
    [SerializeField] private int health = 1;
    [SerializeField] private int meleeDamage = 1;
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

    private float nextAttackTime;

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

        if (player == null)
        {
            Debug.LogError("Игрок не найден! Убедитесь, что у игрока есть тег 'Player'.");
        }

        nextAttackTime = Time.time;
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
        PlayerVisible = true;
    }

    private void UpdateHostileBehavior()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (weaponType == WeaponType.Ranged)
        {
            stoppingDistance = attackRange * 0.8f;
            if (distanceToPlayer < attackRange) MaintainDistance();
            else ApproachPlayer();
        }
        else
        {
            stoppingDistance = 1f;
            ApproachPlayer();
        }

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackRate;
        }
    }

    private void ApproachPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RotateTowards(direction);

        if (!isStatic && Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            rb.linearVelocity = direction * chaseSpeed;
        }
    }

    private void MaintainDistance()
    {
        Vector2 retreatDirection = (transform.position - player.position).normalized;
        rb.linearVelocity = retreatDirection * chaseSpeed * 0.5f;
    }

    private void Attack()
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                MeleeAttack();
                break;
            case WeaponType.Ranged:
                RangedAttack();
                break;
        }
    }

    private void MeleeAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Нанесение урона игроку
                hit.GetComponent<PlayerHealth>()?.TakeDamage(meleeDamage);
            }
        }
    }

    private void RangedAttack()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        Vector2 targetDirection = (player.position - shootPoint.position).normalized;

        // Добавляем случайное отклонение для промаха
        if (Random.value < missChance)
        {
            float spreadAngle = Random.Range(-15f, 15f);
            targetDirection = Quaternion.Euler(0, 0, spreadAngle) * targetDirection;
        }

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.LookRotation(Vector3.forward, targetDirection)
        );

        Rigidbody2D rbProjectile = projectile.GetComponent<Rigidbody2D>();
        if (rbProjectile != null)
        {
            rbProjectile.linearVelocity = targetDirection * projectileSpeed;
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
        StartCoroutine(DamageFeedback());
    }

    public void ApplyPush(Vector2 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private IEnumerator DamageFeedback()
    {
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = Color.white;
        }
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);

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