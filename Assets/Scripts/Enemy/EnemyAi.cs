using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private int health = 1;
    [SerializeField] private GameObject redPuddlePrefab;
    [SerializeField] private bool isStatic = false;

    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 1f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (isStatic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; 
            GetComponent<Collider2D>().isTrigger = true; 
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, 0, targetAngle),
            rotationSpeed * Time.deltaTime
        );

        if (isStatic) return; 

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = direction * moveSpeed;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    private void Die()
    {
        if (redPuddlePrefab != null)
        {
            Instantiate(redPuddlePrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}