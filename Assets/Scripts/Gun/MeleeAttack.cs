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
        if (PauseManager.Instance != null && (PauseManager.Instance.IsPaused || DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive))
            return;

        Vector2 attackPosition = (Vector2)transform.position + attackOffset;
        bool hitDetected = false;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, attackRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy") && hitCollider.TryGetComponent<Yashka>(out var yashka))
            {
                yashka.TakeDamage(attackDamage, DeathType.Melee);
                hitDetected = true;
            }
            else if (hitCollider.CompareTag("Enemy") && hitCollider.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(attackDamage, DeathType.Melee);
                hitDetected = true;
            }
            else if (hitCollider.CompareTag("Scarecrow") && hitCollider.TryGetComponent<Scarecrow>(out var scarecrow))
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