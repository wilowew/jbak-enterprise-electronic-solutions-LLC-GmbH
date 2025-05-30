using UnityEngine;

public class KillableNPC : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private GameObject deathPrefab;
    [SerializeField] private Dialogue deathDialogue; 

    public bool IsDead { get; private set; }
    private bool deathProcessed;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead || deathProcessed) return;

        if (other.CompareTag("Projectile"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (deathProcessed) return;
        deathProcessed = true;
        IsDead = true;

        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, transform.position, transform.rotation);
        }

        if (deathDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(deathDialogue);
        }

        Destroy(gameObject);
    }
}