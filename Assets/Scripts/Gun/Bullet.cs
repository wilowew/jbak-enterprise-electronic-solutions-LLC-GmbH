using UnityEngine;

public class Bullet : MonoBehaviour 
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float pushForce = 5f;

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
        if (timer >= lifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Scarecrow"))
        {
            Scarecrow scarecrow = collision.GetComponent<Scarecrow>();
            if (scarecrow != null)
            {
                scarecrow.PlayDestructionEffect();
            }
        }
        Destroy(gameObject);
    }
}