using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("��������� �����")]
    [SerializeField] private int attackDamage = 1; // ���� �����
    [SerializeField] private float attackRange = 0.5f; // ��������� �������� �����
    [SerializeField] private AudioClip attackSound; // ���� ��� �����

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ����� ��� ���������� �����
    public void Attack()
    {
        // �������� ���� ������ � ������� �����
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

        // ������������� ���� �����, ���� �� ����
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    // ��� ������������ ������� ����� � ���������
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}