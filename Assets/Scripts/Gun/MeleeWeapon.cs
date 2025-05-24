using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponPickupBase))]
public class MeleeWeapon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Rotation Offsets")]
    [Tooltip("Угол удержания по умолчанию (скопируйте из WeaponPickupBase для инициализации)")]
    [SerializeField] private float defaultRotationOffset = -90f;
    [Tooltip("Угол, на который повернётся бита при ударе")]
    [SerializeField] private float swingRotationOffset = -45f;

    [Header("Audio")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleMask;

    private AudioSource audioSource;
    private WeaponPickupBase pickupBase;
    private float lastAttackTime;

    private void Awake()
    {
        pickupBase = GetComponent<WeaponPickupBase>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // Сохраняем дефолтный угол из базового скрипта
        defaultRotationOffset = pickupBase.RotationOffset;

        // Подписываемся на события подбора/сброса
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
        audioSource.PlayOneShot(swingSound);

        pickupBase.RotationOffset = swingRotationOffset;
        pickupBase.UpdateHoldPosition();
        Invoke(nameof(ResetRotation), attackCooldown);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        bool hitSuccess = false;

        foreach (Collider2D hit in hits)
        {
            Vector2 origin = transform.position;
            Vector2 targetPos = hit.transform.position;
            Vector2 direction = targetPos - origin;
            float distance = direction.magnitude;

            RaycastHit2D obstacleHit = Physics2D.Raycast(
                origin,
                direction.normalized,
                distance,
                obstacleMask
            );

            if (obstacleHit.collider != null)
            {
                continue;
            }

            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    hitSuccess = true;
                }
            }

            if (hit.CompareTag("Boss"))
            {
                var yashka = hit.GetComponent<Yashka>();
                if (yashka != null)
                {
                    yashka.TakeDamage(damage);
                    hitSuccess = true;
                }
            }

            Scarecrow scarecrow = hit.GetComponent<Scarecrow>();
            if (scarecrow != null && !scarecrow.IsDestroyed)
            {
                scarecrow.PlayDestructionEffect();
                hitSuccess = true;
            }
        }

        if (hitSuccess)
            audioSource.PlayOneShot(hitSound);
    }

    private bool IsOwnerDead()
    {
        if (pickupBase == null || pickupBase.Owner == null) return false;
        PlayerHealth health = pickupBase.Owner.GetComponent<PlayerHealth>();
        return health != null && health.IsDead;
    }

    private void ResetRotation()
    {
        pickupBase.RotationOffset = defaultRotationOffset;
        pickupBase.UpdateHoldPosition();
    }

    private void HandleWeaponEquipped(SpriteRenderer playerSpriteRenderer)
    {

    }

    private void HandleWeaponDropped()
    {

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