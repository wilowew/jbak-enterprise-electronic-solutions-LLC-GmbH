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
    private float idleTimer; // Таймер ожидания

    [Header("Combat Settings")]
    [SerializeField] private double health = 1;
    [SerializeField] private int meleeDamage = 1;
    [SerializeField] private GameObject redPuddlePrefab;

    [Header("Vision Settings")]
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private bool drawGizmos = true;

    [Header("Movement Settings")]
    [SerializeField] private bool isStatic = false;
    [SerializeField] private bool isHostile = true;
    [SerializeField] private float baseWanderRadius = 5f; // Базовый радиус блуждания
    [SerializeField] private float wanderRadiusVariation = 2f; // Вариация радиуса (+- это значение)
    [SerializeField] private float baseWanderDelay = 0.1f; // Базовая задержка между точками
    [SerializeField] private float maxRandomWanderDelay = 1f; // Максимальная случайная добавка к задержке
    [SerializeField] private float rotationThreshold = 5f; // Угол для завершения поворота
    [SerializeField] private float minTurnAngle = 60f;

    private float previousTargetAngle;

    [Header("Speed Settings")]
    public float chaseSpeed = 3f;
    public float wanderSpeed = 1f;
    public float rotationSpeed = 120f;
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
    [Range(0f, 1f)][SerializeField] private float meleeSwingSoundVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float meleeHitSoundVolume = 1f;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] public float weaponSwingAngle = -45f;

    [Header("Slowdown Settings")]
    [SerializeField] private float slowdownFactor = 0.7f;
    [SerializeField] private float slowdownDuration = 10f;

    private float originalChaseSpeed;
    private float originalWanderSpeed;
    private double originalHealth;
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
    private float currentWanderRadius; // Индивидуальный радиус для этого NPC
    private float currentWanderDelay; // Индивидуальная задержка для этого NPC
    private bool isRotatingToTarget;

    private float nextAttackTime;

    private PlayerHealth playerHealth;

    [Header("Weapon Attack Settings")]
    [SerializeField] private Vector3 weaponAttackOffset = new Vector3(0.3f, 0.3f, 0); // Смещение оружия при атаке
    private Vector3 originalWeaponPosition; // Исходная позиция оружия
    private Quaternion originalWeaponRotation; // Исходный поворот оружия

    [Header("Attack Sprite")]
    [SerializeField] private Sprite attackSprite; // Спрайт для атаки
    private Sprite originalSprite; // Оригинальный спрайт
    private SpriteRenderer spriteRenderer;

    [Header("Ranged Attack Settings")]
    [SerializeField] private AudioClip rangedShotSound; // Звук выстрела для оружия дальнего боя
    [Range(0f, 1f)][SerializeField] private float rangedShotVolume = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialPosition = transform.position;
        SetNewWanderTarget();

        originalChaseSpeed = chaseSpeed;
        originalWanderSpeed = wanderSpeed;
        originalHealth = health;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }

        // Сохранение исходных параметров оружия
        if (weaponTransform != null)
        {
            originalWeaponPosition = weaponTransform.localPosition;
            originalWeaponRotation = weaponTransform.localRotation;
        }

        if (isStatic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            GetComponent<Collider2D>().isTrigger = true;
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        previousTargetAngle = transform.eulerAngles.z;
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

        currentWanderRadius = baseWanderRadius + Random.Range(-wanderRadiusVariation, wanderRadiusVariation);
        currentWanderDelay = baseWanderDelay + Random.Range(0, maxRandomWanderDelay);

        chaseTimer = chaseDuration;
    }

    void FixedUpdate()
    {
        if (isHostile)
        {
            CheckPlayerVisibility();
            UpdateHostileBehavior();

            if (PlayerVisible)
            {
                Vector2 dirToPlayer = (Vector2)player.position - (Vector2)transform.position;
                RotateTowards(dirToPlayer);
            }
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

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

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

        if (PlayerVisible && distanceToTarget <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackRate;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (weaponType == WeaponType.Melee)
        {
            if (PlayerVisible && distanceToTarget > stoppingDistance)
            {
                ApproachTarget(player.position);
            }
            else if (isChasing && distanceToTarget > stoppingDistance)
            {
                ApproachTarget(targetPosition);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (PlayerVisible)
            {
                Vector2 dirToPlayer = (Vector2)player.position - (Vector2)transform.position;
                RotateTowards(dirToPlayer);
            }
        }

        if (weaponType == WeaponType.Ranged && PlayerVisible)
        {
            Vector2 dirToPlayer = (Vector2)player.position - (Vector2)transform.position;
            RotateTowards(dirToPlayer);
        }

        if (PlayerVisible)
        {
            if (weaponType == WeaponType.Ranged)
            {
                if (distanceToTarget < attackRange * 0.8f)
                {
                    MaintainDistance(); 
                }
            }
        }

    }

    private void ApproachTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 avoidance = CalculateObstacleAvoidance();

        Vector2 moveDirection = (direction + avoidance).normalized;

        RotateTowards(moveDirection);

        float currentSpeed = isChasing ? chaseSpeed : wanderSpeed;
        rb.linearVelocity = moveDirection * currentSpeed;
    }

    private Vector2 CalculateObstacleAvoidance()
    {
        Vector2 avoidance = Vector2.zero;
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            avoidanceCheckRadius,
            obstacleLayerMask
        );

        foreach (var hit in hits)
        {
            Vector2 closestPoint = hit.ClosestPoint(transform.position);
            Vector2 dirToObstacle = (Vector2)transform.position - closestPoint;
            float distance = dirToObstacle.magnitude;

            if (distance > 0)
            {
                float forceFactor = Mathf.Clamp01(1 - (distance / avoidanceCheckRadius));
                avoidance += dirToObstacle.normalized * (avoidanceForce * forceFactor);
            }
        }

        return avoidance;
    }

    private void MaintainDistance()
    {
        if (!PlayerVisible)
        {
            ApproachTarget(lastKnownPlayerPosition);
            return;
        }

        Vector2 desiredDirection = (transform.position - player.position).normalized;
        Vector2 avoidance = CalculateObstacleAvoidance();

        float avoidanceMagnitude = avoidance.magnitude;
        if (avoidanceMagnitude > 0 && avoidanceMagnitude < 0.3f)
        {
            avoidance = Vector2.zero;
        }

        Vector2 combinedDirection = (desiredDirection + avoidance).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float speedMultiplier = Mathf.Clamp01((distanceToPlayer - stoppingDistance) / (attackRange - stoppingDistance));

        if (distanceToPlayer > stoppingDistance && distanceToPlayer < stoppingDistance + 0.5f)
        {
            speedMultiplier *= 0.3f;
        }

        RotateTowards(combinedDirection);
        rb.linearVelocity = combinedDirection * chaseSpeed * 0.5f * speedMultiplier;
    }

    private void Attack()
    {
        if (!PlayerVisible) return;
        if (playerHealth != null && playerHealth.IsDead) return;

        // Проверка угла к игроку
        Vector2 dirToPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (Vector2.Angle(transform.right, dirToPlayer) > 30f)
        {
            RotateTowards(dirToPlayer);
            return; // Не атакуем, пока не повернулись
        }

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
            audioSource.PlayOneShot(meleeSwingSound, meleeSwingSoundVolume);
        }


        // Применение изменений для атаки
        ApplyAttackTransform();

        // Вызов сброса через половину времени атаки
        Invoke(nameof(ResetAfterAttack), attackRate * 0.5f);


        if (meleeSwingSound != null)
        {
            audioSource.PlayOneShot(meleeSwingSound, meleeSwingSoundVolume);
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
            audioSource.PlayOneShot(meleeHitSound, meleeHitSoundVolume);
        }
    }

    private void ApplyAttackTransform()
    {
        // Смена спрайта NPC
        if (spriteRenderer != null && attackSprite != null)
        {
            spriteRenderer.sprite = attackSprite;
        }

        // Изменение позиции и поворота оружия
        if (weaponTransform != null)
        {
            weaponTransform.localPosition = weaponAttackOffset;
            weaponTransform.localRotation = Quaternion.Euler(0, 0, weaponSwingAngle);
        }
    }
    private void ResetAfterAttack()
    {
        // Восстановление спрайта NPC
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }

        // Восстановление параметров оружия
        if (weaponTransform != null)
        {
            weaponTransform.localPosition = originalWeaponPosition;
            weaponTransform.localRotation = originalWeaponRotation;
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
        if (!HasClearPathToPlayer(attackRange)) return;
        if (!IsFacingPlayer(15f)) return;

        // Воспроизведение звука выстрела
        if (rangedShotSound != null)
        {
            audioSource.PlayOneShot(rangedShotSound, rangedShotVolume);
        }

        Vector2 targetDirection = ((Vector2)player.position - (Vector2)shootPoint.position).normalized;

        if (Random.value < missChance)
        {
            float spreadAngle = Random.Range(-15f, 15f);
            targetDirection = Quaternion.Euler(0, 0, spreadAngle) * targetDirection;
        }

        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.Euler(0, 0, angle)
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
        // Если NPC ещё не повернулся к цели
        Vector2 directionToTarget = (currentWanderTarget - (Vector2)transform.position).normalized;
        float angleToTarget = Vector2.Angle(transform.right, directionToTarget);

        if (angleToTarget > rotationThreshold)
        {
            RotateTowards(directionToTarget);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Если NPC на месте или время ожидания истекло
        if (Vector2.Distance(transform.position, currentWanderTarget) < 0.1f || wanderTimer <= 0)
        {
            if (idleTimer <= 0)
            {
                SetNewWanderTarget();
                wanderTimer = currentWanderDelay;
                idleTimer = currentWanderDelay;
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                idleTimer -= Time.fixedDeltaTime;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        // Движение к цели
        rb.linearVelocity = directionToTarget * wanderSpeed;
        wanderTimer -= Time.fixedDeltaTime;
    }

    private void SetNewWanderTarget()
    {
        int attempts = 0;
        bool validTargetFound = false;
        Vector2 newTarget = Vector2.zero;
        float newAngle = 0f;

        while (attempts < 5 && !validTargetFound)
        {
            // Генерируем новую точку
            newTarget = initialPosition + Random.insideUnitCircle * currentWanderRadius;

            // Вычисляем угол к новой точке
            Vector2 direction = newTarget - (Vector2)transform.position;
            newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Проверяем угол поворота
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(previousTargetAngle, newAngle));
            if (angleDifference >= minTurnAngle)
            {
                validTargetFound = true;
            }

            attempts++;
        }

        if (validTargetFound)
        {
            currentWanderTarget = newTarget;
            previousTargetAngle = newAngle;
        }
        else
        {
            // Если не нашли подходящую точку - берем любую
            currentWanderTarget = initialPosition + Random.insideUnitCircle * currentWanderRadius;
            previousTargetAngle = transform.eulerAngles.z;
        }
    }

    private bool IsFacingPlayer(float tolerance = 30f)
    {
        Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 forward = transform.right;
        float angle = Vector2.Angle(forward, toPlayer);
        return angle < tolerance;
    }

    private void RotateTowards(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    public void TakeDamage(double damage)
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
        // Отмена запланированного сброса
        CancelInvoke(nameof(ResetAfterAttack));

        // Вызов дропа патронов
        AmmoDropper dropper = GetComponent<AmmoDropper>();
        if (dropper != null)
        {
            dropper.DropAmmo();
        }

        // Оригинальный код смерти
        if (redPuddlePrefab != null)
        {
            Instantiate(redPuddlePrefab, transform.position, transform.rotation);
        }
        FindAnyObjectByType<BackgroundMusic>()?.AddKillPoint();
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