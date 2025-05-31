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
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearVelocity = transform.up * speed;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if (collision.collider.CompareTag("Scarecrow"))
        {
            Scarecrow scarecrow = collision.collider.GetComponent<Scarecrow>();
            if (scarecrow != null)
                scarecrow.PlayDestructionEffect();
            Destroy(gameObject);
        }
        else if (collision.collider.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.collider.GetComponent<EnemyAI>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
