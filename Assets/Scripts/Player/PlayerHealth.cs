using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private Vector3 deathScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private float regenInterval = 10f;
    [SerializeField] private Image healthOverlay;
    [SerializeField] private float maxOverlayAlpha = 0.7f;
    [SerializeField] private CanvasGroup deathScreenGroup;
    [SerializeField] private CursorChanger cursorChanger;

    private int currentHealth;
    private Vector3 originalScale;
    private float regenTimer = 0f;

    private bool isDead = false;
    public bool IsDead => isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;
        if (healthOverlay != null)
            healthOverlay.color = new Color(healthOverlay.color.r, healthOverlay.color.g, healthOverlay.color.b, 0);
    }

    private void Update()
    {
        UpdateHealthRegeneration();
        UpdateHealthOverlay();
    }

    private void UpdateHealthRegeneration()
    {
        if (!isDead && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
                regenTimer = 0f;
            }
        }
        else
        {
            regenTimer = 0f;
        }
    }

    private void UpdateHealthOverlay()
    {
        if (healthOverlay == null || isDead) return;

        if (currentHealth < maxHealth)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            float targetAlpha = (1 - healthPercentage) * maxOverlayAlpha;

            Color newColor = healthOverlay.color;
            newColor.a = Mathf.Lerp(healthOverlay.color.a, targetAlpha, Time.deltaTime * 5f);
            healthOverlay.color = newColor;
        }
        else
        {
            Color newColor = healthOverlay.color;
            newColor.a = Mathf.Lerp(healthOverlay.color.a, 0f, Time.deltaTime * 5f);
            healthOverlay.color = newColor;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        regenTimer = 0f; 
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");

        if (cursorChanger != null)
        {
            cursorChanger.SetPauseCursor();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathScreenGroup != null)
        {
            deathScreenGroup.gameObject.SetActive(true);
            StartCoroutine(FadeInDeathScreen());
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

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

    private System.Collections.IEnumerator FadeInDeathScreen()
    {
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            deathScreenGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        deathScreenGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}