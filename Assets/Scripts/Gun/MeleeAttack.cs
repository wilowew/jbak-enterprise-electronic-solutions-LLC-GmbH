using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Vector2 attackOffset = Vector2.right;
    [SerializeField] private AudioClip attackSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Attack()
    {
        Vector2 attackPosition = (Vector2)transform.position + attackOffset;

        // Получаем все коллайдеры в радиусе атаки
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, attackRange);

        bool hitDetected = false;

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Boss") && hitCollider.TryGetComponent<Yashka>(out var yashka))
            {
                yashka.TakeDamage(attackDamage);
                hitDetected = true;
                Debug.Log("Попал по Яшке!");
                continue;
            }

            if (hitCollider.CompareTag("Enemy") && hitCollider.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(attackDamage);
                hitDetected = true;
                continue;
            }
            if (hitCollider.CompareTag("Scarecrow") && hitCollider.TryGetComponent<Scarecrow>(out var scarecrow))
            {
                if (!scarecrow.IsDestroyed)
                {
                    scarecrow.PlayDestructionEffect();
                    hitDetected = true;
                }
            }
        }

        if (attackSound != null && hitDetected)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + attackOffset, attackRange);
    }
}