using UnityEngine;
using UnityEngine.InputSystem;

public class UnarmedCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private double damage = 0.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float combatStanceDuration = 3f;
    [SerializeField] private float attackAngle = 90f; // Угол атаки в градусах

    [Header("Sprites")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite combatStanceSprite;
    [SerializeField] private Sprite attackSprite1;
    [SerializeField] private Sprite attackSprite2;

    [Header("Audio")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleMask;

    private SpriteRenderer playerSprite;
    private WeaponInventory inventory;
    private AudioSource audioSource;
    private float lastAttackTime;
    private bool nextIsSecondAttack;
    private bool isAttacking;
    private float combatStanceTimer;
    private bool inCombatStance;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        inventory = GetComponent<WeaponInventory>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        SetDefaultSprite();
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (IsCombatBlocked()) return;
        UpdateCombatStance();

        if (Mouse.current.leftButton.wasPressedThisFrame &&
        !inventory.HasWeaponEquipped() &&
        !isAttacking)
            TryAttack();
    }

    private void UpdateCombatStance()
    {
        if (inCombatStance)
        {
            combatStanceTimer -= Time.deltaTime;

            if (combatStanceTimer <= 0)
            {
                inCombatStance = false;
                SetDefaultSprite();
            }
        }
    }

    private bool IsCombatBlocked()
    {
        return (PauseManager.Instance != null && PauseManager.Instance.IsPaused) ||
               (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) ||
               (DialogueManager.Instance != null && DialogueManager.Instance.IsInPostDialogueDelay);
    }

    private void TryAttack()
    {
        if (IsCombatBlocked()) return;
        if (inventory.HasWeaponEquipped()) return;
        if (playerHealth != null && playerHealth.IsDead) return;
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;
        isAttacking = true;
        audioSource.PlayOneShot(swingSound);

        ActivateCombatStance();

        if (playerSprite != null)
        {
            playerSprite.sprite = nextIsSecondAttack ? attackSprite2 : attackSprite1;
            nextIsSecondAttack = !nextIsSecondAttack;
        }

        DetectHits();
        Invoke(nameof(ResetAttackState), attackCooldown);
    }

    private void ActivateCombatStance()
    {
        inCombatStance = true;
        combatStanceTimer = combatStanceDuration;

        if (playerSprite.sprite != combatStanceSprite)
        {
            playerSprite.sprite = combatStanceSprite;
        }
    }

    private void ResetAttackState()
    {
        isAttacking = false;

        if (inCombatStance && playerSprite != null && !inventory.HasWeaponEquipped())
        {
            playerSprite.sprite = combatStanceSprite;
        }
    }

    private void SetDefaultSprite()
    {
        if (playerSprite != null && defaultSprite != null)
            playerSprite.sprite = defaultSprite;
    }

    private void DetectHits()
    {
        if (IsCombatBlocked()) return;
        if (inventory.HasWeaponEquipped()) return;
        if (playerHealth != null && playerHealth.IsDead) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        bool hitSuccess = false;

        foreach (Collider2D hit in hits)
        {
            // Проверяем находится ли цель в зоне атаки
            if (!IsTargetInAttackZone(hit.transform.position))
                continue;

            if (CheckObstacle(hit))
                continue;

            if (hit.CompareTag("Enemy") && hit.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hitSuccess = true;
            }

            if (hit.CompareTag("Enemy") && hit.TryGetComponent<Yashka>(out var boss))
            {
                boss.TakeDamage(damage);
                hitSuccess = true;
            }

            if (hit.TryGetComponent<Scarecrow>(out var scarecrow) && !scarecrow.IsDestroyed)
            {
                scarecrow.PlayDestructionEffect();
                hitSuccess = true;
            }
        }

        if (hitSuccess) audioSource.PlayOneShot(hitSound);
    }

    private bool IsTargetInAttackZone(Vector3 targetPosition)
    {
        // Направление от игрока к цели
        Vector2 directionToTarget = (targetPosition - transform.position).normalized;

        // Направление взгляда игрока (в 2D top-down это обычно transform.right)
        Vector2 lookDirection = transform.right;

        // Угол между направлением взгляда и направлением к цели
        float angle = Vector2.Angle(lookDirection, directionToTarget);

        // Проверяем попадает ли цель в сектор атаки
        return angle <= attackAngle / 2f;
    }

    private bool CheckObstacle(Collider2D hit)
    {
        RaycastHit2D obstacleHit = Physics2D.Raycast(
            transform.position,
            (hit.transform.position - transform.position).normalized,
            Vector2.Distance(transform.position, hit.transform.position),
            obstacleMask
        );
        return obstacleHit.collider != null;
    }

    public void CancelAttack()
    {
        isAttacking = false;
        inCombatStance = false;
        SetDefaultSprite();
        CancelInvoke(nameof(ResetAttackState));
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса атаки
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Визуализация сектора атаки
        Gizmos.color = Color.red;
        float halfAngle = attackAngle / 2f;
        Vector2 lookDirection = transform.right;

        Vector2 leftBound = Quaternion.Euler(0, 0, halfAngle) * lookDirection * attackRadius;
        Vector2 rightBound = Quaternion.Euler(0, 0, -halfAngle) * lookDirection * attackRadius;

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftBound);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightBound);
        Gizmos.DrawLine((Vector2)transform.position + leftBound, (Vector2)transform.position + rightBound);
    }
}