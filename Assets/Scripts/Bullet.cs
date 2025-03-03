using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float pushForce = 5f;

    private Rigidbody2D rb;
    private float timer;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * speed;
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.attachedRigidbody != null) {
            Vector2 pushDirection = rb.linearVelocity.normalized;
            collision.attachedRigidbody.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        }
        Destroy(gameObject);
    }
}