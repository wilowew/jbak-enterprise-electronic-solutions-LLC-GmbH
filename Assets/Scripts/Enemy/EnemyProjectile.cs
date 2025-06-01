using UnityEngine;

public class EnemyProjectile : MonoBehaviour
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
        rb.linearVelocity = transform.up * speed;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}

