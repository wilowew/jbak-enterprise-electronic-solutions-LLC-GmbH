using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private int attackDamage = 1; // Сила удара
    [SerializeField] private float attackRange = 0.5f; // Дальность действия атаки
    [SerializeField] private AudioClip attackSound; // Звук при атаке

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Метод для выполнения атаки
    public void Attack()
    {
        // Получаем всех врагов в радиусе атаки
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                EnemyAI enemy = hitCollider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }
        }

        // Воспроизводим звук атаки, если он есть
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    // Для визуализации радиуса атаки в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}