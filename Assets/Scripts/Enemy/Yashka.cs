using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class Yashka : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float desiredDistance = 4f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float distanceDeadZone = 0.5f; 
    [SerializeField] private float movementSmoothFactor = 5f; 

    [Header("Attack Settings")]
    [SerializeField] private float lungeSpeed = 10f;
    [SerializeField] private float attackWindup = 0.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private int attackDamage = 2;
    [SerializeField][Range(0f, 1f)] private float attackProbability = 0.3f;

    [Header("Dodge Settings")]
    [SerializeField][Range(0f, 1f)] private float dodgeChance = 0.5f;
    [SerializeField] private float dodgeSpeed = 8f;
    [SerializeField] private float dodgeDuration = 0.3f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float damageFlashTime = 0.1f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Collider2D weaponCollider;
    [SerializeField] private float swingAngle = 90f;
    [SerializeField] private float swingSpeed = 360f;
    [SerializeField] private float returnSpeed = 180f;
    [SerializeField] private float attackActiveAngle = 45f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Death Settings")]
    [SerializeField] private GameObject deathPrefab;

    private Rigidbody2D rb;

    private AudioSource audioSource;

    private Quaternion originalWeaponRotation;
    private bool isSwinging = false;

    private int currentHealth;
    private bool isAttacking = false;
    private bool isDodging = false;
    private float nextAttackTime = 0f;
    private Vector2 movementDirection;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        originalWeaponRotation = weaponPivot.localRotation;
        weaponCollider.enabled = false;
    }

    private void Update()
    {
        if (player == null || isDodging) return;

        HandleDistanceManagement();
        RotateTowardsPlayer();

        if (CanAttack() && Random.value <= attackProbability)
        {
            StartCoroutine(LungeAttackSequence());
        }
    }

    private IEnumerator LungeAttackSequence()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        rb.linearVelocity = Vector2.zero;
        yield return StartCoroutine(SwingWeapon(-swingAngle, swingSpeed));

        Vector2 lungeDirection = (player.position - transform.position).normalized;
        rb.linearVelocity = lungeDirection * lungeSpeed;
        StartCoroutine(PerformAttackSwing());
        audioSource.PlayOneShot(swingSound);

        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(ReturnWeapon());
        isAttacking = false;
    }

    private IEnumerator PerformAttackSwing()
    {
        weaponCollider.enabled = true;
        yield return StartCoroutine(SwingWeapon(swingAngle, swingSpeed * 2f));
        weaponCollider.enabled = false;
    }

    private IEnumerator SwingWeapon(float targetAngle, float speed)
    {
        isSwinging = true;
        Quaternion startRotation = weaponPivot.localRotation;
        Quaternion targetRotation = originalWeaponRotation * Quaternion.Euler(0, 0, targetAngle);

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * speed / Mathf.Abs(targetAngle);
            weaponPivot.localRotation = Quaternion.Lerp(
                startRotation,
                targetRotation,
                Mathf.Clamp01(progress)
            );
            yield return null;
        }
        isSwinging = false;
    }

    private IEnumerator ReturnWeapon()
    {
        Quaternion startRotation = weaponPivot.localRotation;
        float angleDifference = Quaternion.Angle(startRotation, originalWeaponRotation);

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * returnSpeed / angleDifference;
            weaponPivot.localRotation = Quaternion.Lerp(
                startRotation,
                originalWeaponRotation,
                Mathf.Clamp01(progress)
            );
            yield return null;
        }
    }

    private void UpdateWeaponPosition()
    {
        if (!isAttacking && !isSwinging)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void FixedUpdate()
    {
        if (isDodging || isAttacking || player == null) return;

        Vector2 avoidance = CalculateObstacleAvoidance();
        Vector2 targetVelocity = (movementDirection + avoidance).normalized * moveSpeed;

        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * movementSmoothFactor
        );

        UpdateWeaponPosition();
    }

    private bool CanAttack()
    {
        return Time.time >= nextAttackTime &&
                Vector2.Distance(transform.position, player.position) <= attackRadius &&
                HasLineOfSight();
    }

    private void HandleDistanceManagement()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (Mathf.Abs(distance - desiredDistance) < distanceDeadZone)
        {
            movementDirection = Vector2.zero;
        }
        else if (distance > desiredDistance)
        {
            movementDirection = toPlayer.normalized;
        }
        else
        {
            movementDirection = -toPlayer.normalized;
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private void TryDamagePlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    private Vector2 CalculateObstacleAvoidance()
    {
        Vector2 avoidance = Vector2.zero;
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, 2f, obstacleLayer);

        foreach (var obstacle in obstacles)
        {
            Vector2 toObstacle = (Vector2)transform.position - (Vector2)obstacle.transform.position;
            avoidance += toObstacle.normalized / toObstacle.magnitude;
        }

        return avoidance;
    }

    public void TakeDamage(int damage)
    {
        if (isDodging) return;

        if (Random.value <= dodgeChance)
        {
            StartCoroutine(PerformDodge());
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color original = renderer.color;
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.color = original;
        }
    }

    private IEnumerator PerformDodge()
    {
        isDodging = true;
        Vector2 dodgeDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.linearVelocity = dodgeDirection * dodgeSpeed;

        yield return new WaitForSeconds(dodgeDuration);

        rb.linearVelocity = Vector2.zero;
        isDodging = false;
    }

    private bool HasLineOfSight()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            player.position - transform.position,
            Vector2.Distance(transform.position, player.position),
            obstacleLayer
        );
        return hit.collider == null;
    }

    private void Die()
    {
        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>().radius);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Collided with: {other.name}");

        if (weaponCollider.enabled && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                if (hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}