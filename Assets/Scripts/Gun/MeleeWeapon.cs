// MeleeWeapon.cs
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

    [Header("Visuals")]
    [SerializeField] private Sprite playerHoldingSprite;
    [SerializeField] private Sprite originalPlayerSprite;

    [Header("Audio")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    private AudioSource audioSource;
    private WeaponPickupBase pickupBase;
    private float lastAttackTime;
    private SpriteRenderer playerSprite;

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
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        audioSource.PlayOneShot(swingSound);

        // Сразу меняем угол удержания
        pickupBase.SetRotationOffset(swingRotationOffset);
        // Обновляем позицию/ротацию тут же, чтобы увидеть «срезанный» угол
        pickupBase.UpdateHoldPosition();

        // Вернём исходный угол после завершения кд
        Invoke(nameof(ResetRotation), attackCooldown);

        // Ищем все коллайдеры в радиусе атаки
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRadius
        );

        bool hitSuccess = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    hitSuccess = true;
                }
            }
        }

        if (hitSuccess)
            audioSource.PlayOneShot(hitSound);
    }

    private void ResetRotation()
    {
        pickupBase.SetRotationOffset(defaultRotationOffset);
        pickupBase.UpdateHoldPosition();
    }

    private void HandleWeaponEquipped(SpriteRenderer playerSpriteRenderer)
    {
        playerSprite = playerSpriteRenderer;
        if (playerHoldingSprite != null)
        {
            originalPlayerSprite = playerSprite.sprite;
            playerSprite.sprite = playerHoldingSprite;
        }
    }

    private void HandleWeaponDropped()
    {
        if (playerSprite != null && originalPlayerSprite != null)
        {
            playerSprite.sprite = originalPlayerSprite;
        }
        // При сбросе тоже вернём дефолтный угол
        pickupBase.SetRotationOffset(defaultRotationOffset);
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
