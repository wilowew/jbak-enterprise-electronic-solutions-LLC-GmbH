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

    [Header("Obstacle Avoidance")]
    [SerializeField] private float avoidanceCheckRadius = 1f;
    [SerializeField] private float avoidanceForce = 1f;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Chase Settings")]
    [SerializeField] private float chaseDuration = 5f;
    //[SerializeField] private float searchRadius = 3f;  

    private Vector2 lastKnownPlayerPosition;
    private bool isChasing;
    private float chaseTimer;

    [Header("Melee Attack Settings")]
    [SerializeField] private float meleeAttackRadius = 1.5f;
    [SerializeField] private AudioClip meleeSwingSound;
    [SerializeField] private AudioClip meleeHitSound;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float weaponSwingAngle = -45f;

    [Header("Slowdown Settings")]
    [SerializeField] private float slowdownFactor = 0.7f;
    [SerializeField] private float slowdownDuration = 10f;

    private float originalChaseSpeed;
    private float originalWanderSpeed;
    private int originalHealth;
    private Coroutine recoverCoroutine;

    private AudioSource audioSource;
    private float defaultWeaponRotation;

    public bool IsStatic => isStatic;
    public bool IsHostile => isHostile;
    public bool PlayerVisible { get; private set; }

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 initialPosition;
    private Vector2 currentWanderTarget;
    private float wanderTimer;

    private float nextAttackTime;

    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialPosition = transform.position;
        SetNewWanderTarget();

        originalChaseSpeed = chaseSpeed;
        originalWanderSpeed = wanderSpeed;
        originalHealth = health;

        if (isStatic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            GetComponent<Collider2D>().isTrigger = true;
        }

        if (player == null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        nextAttackTime = Time.time;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (weaponTransform != null)
        {
            defaultWeaponRotation = weaponTransform.localEulerAngles.z;
        }

        chaseTimer = chaseDuration;
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

        Vector2 directionToPlayer = (Vector2)player.position - (Vector2)transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
        {
            PlayerVisible = false;
            return;
        }

        float angleToPlayer = Vector2.Angle(transform.right, directionToPlayer.normalized);
        if (angleToPlayer > fieldOfViewAngle / 2f)
        {
            PlayerVisible = false;
            return;
        }

        int obstacleLayer = LayerMask.NameToLayer("Obstacles");
        int playerLayer = LayerMask.NameToLayer("Player");
        int layerMask = (1 << obstacleLayer) | (1 << playerLayer);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer.normalized,
            distanceToPlayer,
            layerMask
        );

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
            PlayerVisible = (ph != null && !ph.IsDead);
        }
        else
        {
            PlayerVisible = false;
        }

        if (PlayerVisible)
        {
            lastKnownPlayerPosition = player.position;
            chaseTimer = chaseDuration;
            isChasing = true;
        }
    }

    private void UpdateHostileBehavior()
    {
        if (player == null) return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            isChasing = false;
            PlayerVisible = false;
            if (!isStatic) WanderAround();
            return;
        }

        CheckPlayerVisibility();

        if (PlayerVisible || isChasing)
        {
            if (PlayerVisible)
            {
                HandlePlayerChase(player.position);
            }
            else
            {
                HandlePlayerChase(lastKnownPlayerPosition);
                chaseTimer -= Time.fixedDeltaTime;

                if (chaseTimer <= 0)
                {
                    isChasing = false;
                    if (!isStatic) WanderAround();
                }
            }
        }
        else
        {
            if (!isStatic) WanderAround();
        }
    }

    private void HandlePlayerChase(Vector2 targetPosition)
    {
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (distanceToTarget <= attackRange && Time.time >= nextAttackTime && PlayerVisible)
        {
            Attack();
            nextAttackTime = Time.time + attackRate;
        }

        if (weaponType == WeaponType.Ranged)
        {
            stoppingDistance = attackRange * 0.8f;
            if (distanceToTarget < attackRange) MaintainDistance();
            else ApproachTarget(targetPosition);
        }
        else
        {
            stoppingDistance = 1f;
            ApproachTarget(targetPosition);
        }

        if (distanceToTarget <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackRate;
        }
    }

    private void ApproachTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 avoidance = CalculateObstacleAvoidance();
        direction = (direction + avoidance).normalized;

        RotateTowards(direction);

        if (!isStatic && Vector2.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            rb.linearVelocity = direction * chaseSpeed;
        }
    }

    private Vector2 CalculateObstacleAvoidance()
    {
        Vector2 avoidance = Vector2.zero;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, avoidanceCheckRadius, obstacleLayerMask);

        foreach (var hit in hits)
        {
            Vector2 closestPoint = hit.ClosestPoint(transform.position);
            Vector2 dirToObstacle = (Vector2)transform.position - closestPoint;
            float distance = dirToObstacle.magnitude;

            if (distance > 0)
            {
                avoidance += dirToObstacle.normalized * (avoidanceForce / distance);
            }
        }

        return avoidance;
    }

    private void MaintainDistance()
    {
        Vector2 desiredDirection = (transform.position - player.position).normalized;
        Vector2 avoidance = CalculateObstacleAvoidance();
        Vector2 combinedDirection = (desiredDirection + avoidance).normalized;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float speedMultiplier = Mathf.Clamp01((distanceToPlayer - stoppingDistance) / (attackRange - stoppingDistance));

        RotateTowards(combinedDirection);
        rb.linearVelocity = combinedDirection * chaseSpeed * 0.5f * speedMultiplier;
    }

    private void Attack()
    {
        if (!PlayerVisible) return;
        if (playerHealth != null && playerHealth.IsDead) return;

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
        if (!HasClearPathToPlayer(meleeAttackRadius))
        {
            return;
        }

        if (meleeSwingSound != null)
        {
            audioSource.PlayOneShot(meleeSwingSound);
        }

        if (weaponTransform != null)
        {
            weaponTransform.localRotation = Quaternion.Euler(0, 0, weaponSwingAngle);
            Invoke(nameof(ResetWeaponRotation), attackRate * 0.5f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeAttackRadius);
        bool hitConnected = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null && !health.IsDead)
                {
                    health.TakeDamage(meleeDamage);
                    hitConnected = true;
                }
            }
        }

        if (hitConnected && meleeHitSound != null)
        {
            audioSource.PlayOneShot(meleeHitSound);
        }
    }

    private void ResetWeaponRotation()
    {
        if (weaponTransform != null)
        {
            weaponTransform.localRotation = Quaternion.Euler(0, 0, defaultWeaponRotation);
        }
    }

    private void RangedAttack()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        if (!HasClearPathToPlayer(meleeAttackRadius))
        {
            return;
        }

        Vector2 targetDirection = (player.position - shootPoint.position).normalized;

        // ƒобавл€ем случайное отклонение дл€ промаха
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

    private bool HasClearPathToPlayer(float maxDistance)
    {
        if (player == null) return false;

        Vector2 directionToPlayer = (Vector2)player.position - (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer.normalized,
            maxDistance,
            LayerMask.GetMask("Obstacles")
        );

        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            return false;
        }

        return true;
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
        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (recoverCoroutine != null)
            {
                StopCoroutine(recoverCoroutine);
            }
            recoverCoroutine = StartCoroutine(SlowDownAndRecover());
            StartCoroutine(DamageFeedback());
        }
    }

    private IEnumerator SlowDownAndRecover()
    {
        chaseSpeed = originalChaseSpeed * slowdownFactor;
        wanderSpeed = originalWanderSpeed * slowdownFactor;

        yield return new WaitForSeconds(slowdownDuration);

        chaseSpeed = originalChaseSpeed;
        wanderSpeed = originalWanderSpeed;
        health = originalHealth;
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
        FindObjectOfType<BackgroundMusic>()?.AddKillPoint();
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