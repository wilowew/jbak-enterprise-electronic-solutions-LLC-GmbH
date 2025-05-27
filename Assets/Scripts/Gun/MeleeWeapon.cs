using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponPickupBase))]
public class MeleeWeapon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackAngle = 180;

    [Header("Attack Offsets")]
    [SerializeField] private Vector3 attackHoldOffset = new Vector3(0.3f, 0.3f, 0);
    private Vector3 originalHoldOffset;
    private float originalRotationOffset;

    [Header("Visuals")]
    [SerializeField] private Sprite attackSprite;
    private Sprite originalPlayerSprite;
    private SpriteRenderer playerSprite;

    [Header("Audio")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleMask;

    private WeaponPickupBase pickupBase;
    private AudioSource audioSource;
    private float lastAttackTime;

    private void Awake()
    {
        pickupBase = GetComponent<WeaponPickupBase>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // Сохраняем оригинальные параметры из WeaponPickupBase
        originalHoldOffset = pickupBase.HoldOffset;
        originalRotationOffset = pickupBase.RotationOffset;
    }

    private void Start()
    {
        pickupBase.OnEquipped += HandleWeaponEquipped;
        pickupBase.OnDropped += HandleWeaponDropped;
    }

    private void Update()
    {
        if (pickupBase.IsHeld && Mouse.current.leftButton.wasPressedThisFrame)
            TryAttack();
    }

    private void TryAttack()
    {
        if (IsOwnerDead() || Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        ApplyAttackTransform();
        audioSource.PlayOneShot(swingSound);

        if (playerSprite != null && attackSprite != null)
            playerSprite.sprite = attackSprite;

        Invoke(nameof(ResetWeaponTransform), attackCooldown);
        DetectHits();
    }

    private void ApplyAttackTransform()
    {
        // Временное изменение параметров позиции и поворота
        pickupBase.HoldOffset = attackHoldOffset;
        pickupBase.RotationOffset = originalRotationOffset + attackAngle; // Резкий поворот
        pickupBase.UpdateHoldPosition(); // Принудительное обновление
    }

    private void ResetWeaponTransform()
    {
        // Восстановление оригинальных параметров
        pickupBase.HoldOffset = originalHoldOffset;
        pickupBase.RotationOffset = originalRotationOffset;
        pickupBase.UpdateHoldPosition();

        if (playerSprite != null)
            playerSprite.sprite = originalPlayerSprite;
    }

    private void DetectHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        bool hitSuccess = false;

        foreach (Collider2D hit in hits)
        {
            if (CheckObstacle(hit)) continue;

            if (hit.CompareTag("Enemy") && hit.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hitSuccess = true;
            }

            if (hit.CompareTag("Boss") && hit.TryGetComponent<Yashka>(out var boss))
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

    private void HandleWeaponEquipped(SpriteRenderer playerSpriteRenderer)
    {
        playerSprite = playerSpriteRenderer;
        originalPlayerSprite = pickupBase.EquippedPlayerSprite; // Используем спрайт из WeaponPickupBase
    }

    private void HandleWeaponDropped()
    {
        if (playerSprite != null)
            playerSprite.sprite = originalPlayerSprite;
    }

    private bool IsOwnerDead()
    {
        return pickupBase.Owner != null &&
             pickupBase.Owner.TryGetComponent<PlayerHealth>(out var health) &&
             health.IsDead;
    }

    private void OnDestroy()
    {
        if (pickupBase != null)
        {
            pickupBase.OnEquipped -= HandleWeaponEquipped;
            pickupBase.OnDropped -= HandleWeaponDropped;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}