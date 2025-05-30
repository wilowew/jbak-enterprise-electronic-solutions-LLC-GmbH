using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 5;

    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * speed; 
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Scarecrow"))
        {
            Scarecrow scarecrow = collision.GetComponent<Scarecrow>();
            if (scarecrow != null)
                scarecrow.PlayDestructionEffect();

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}