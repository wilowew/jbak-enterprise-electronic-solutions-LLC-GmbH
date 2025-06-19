using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(WeaponPickupBase))]
public class MeleeWeapon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float weaponSwingAngle = 180; // Угол поворота оружия
    [SerializeField] private float attackAngle = 90f; // Угол атаки в градусах

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

    [Header("Trail Effect")]
    [SerializeField] private Sprite[] trailFrames; // 6 кадров для следа
    [SerializeField] private float frameDuration = 0.05f; // Длительность каждого кадра
    [SerializeField] private Vector3 trailOffset = new Vector3(0, 0.1f, 0); // Смещение следа относительно оружия
    [SerializeField] private Color trailColor = Color.white;
    [SerializeField] private Material trailMaterial;
    [SerializeField] private float trailScale = 1f;

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
        if (PauseManager.Instance != null && (PauseManager.Instance.IsPaused || DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive))
            return;

        if (IsOwnerDead() || Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        ApplyAttackTransform();
        audioSource.PlayOneShot(swingSound);

        if (playerSprite != null && attackSprite != null)
            playerSprite.sprite = attackSprite;

        // Запуск эффекта следа
        StartCoroutine(PlayTrailEffect());

        Invoke(nameof(ResetWeaponTransform), attackCooldown);
        DetectHits();
    }
    private IEnumerator PlayTrailEffect()
    {
        if (trailFrames == null || trailFrames.Length == 0) yield break;

        // Создаем объект для следа
        GameObject trailObject = new GameObject("WeaponTrail");
        SpriteRenderer trailRenderer = trailObject.AddComponent<SpriteRenderer>();

        // Настройка рендерера
        trailRenderer.sortingOrder = 10; // Поверх других спрайтов
        trailRenderer.color = trailColor;
        if (trailMaterial != null) trailRenderer.material = trailMaterial;
        trailObject.transform.localScale = Vector3.one * trailScale;

        // Позиционирование
        trailObject.transform.position = transform.position + transform.TransformDirection(trailOffset);
        trailObject.transform.rotation = transform.rotation;

        // Проигрываем анимацию кадров
        for (int i = 0; i < trailFrames.Length; i++)
        {
            if (trailFrames[i] == null) continue;

            trailRenderer.sprite = trailFrames[i];
            yield return new WaitForSeconds(frameDuration);
        }

        // Уничтожаем объект следа после завершения
        Destroy(trailObject);
    }

    private void ApplyAttackTransform()
    {
        // Временное изменение параметров позиции и поворота
        pickupBase.HoldOffset = attackHoldOffset;
        pickupBase.RotationOffset = originalRotationOffset + weaponSwingAngle;
        pickupBase.UpdateHoldPosition();
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
            // Проверяем находится ли цель в зоне атаки
            if (!IsTargetInAttackZone(hit.transform.position))
                continue;

            if (CheckObstacle(hit)) continue;

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

        // Направление взгляда игрока
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

    public void HandleWeaponEquipped(SpriteRenderer playerSpriteRenderer)
    {
        playerSprite = playerSpriteRenderer;
        originalPlayerSprite = pickupBase.EquippedPlayerSprite;
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
        // Визуализация радиуса атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Визуализация сектора атаки
        Gizmos.color = Color.yellow;
        float halfAngle = attackAngle / 2f;
        Vector2 lookDirection = transform.right;

        Vector2 leftBound = Quaternion.Euler(0, 0, halfAngle) * lookDirection * attackRadius;
        Vector2 rightBound = Quaternion.Euler(0, 0, -halfAngle) * lookDirection * attackRadius;

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftBound);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightBound);
        Gizmos.DrawLine((Vector2)transform.position + leftBound, (Vector2)transform.position + rightBound);
    }
}