using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private Vector3 deathScale = new Vector3(0.5f, 0.5f, 1f);
    private int currentHealth;
    private Vector3 originalScale;

    private void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Player Died!");

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.SetMovement(false);

        playerSpriteRenderer.sortingOrder = -9;

        if (playerSpriteRenderer != null && deadSprite != null)
        {
            playerSpriteRenderer.sprite = deadSprite;
            transform.localScale = deathScale; 
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; 
        }

        StartCoroutine(RestartSceneAfterDelay(3f));
    }

    private System.Collections.IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}