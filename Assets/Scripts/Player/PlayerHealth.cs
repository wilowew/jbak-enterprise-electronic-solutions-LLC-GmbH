using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private GameObject deathPrefab; 
    [SerializeField] private float regenInterval = 10f;
    [SerializeField] private Image healthOverlay;
    [SerializeField] private float maxOverlayAlpha = 0.7f;
    [SerializeField] private CanvasGroup deathScreenGroup;
    [SerializeField] private CursorChanger cursorChanger;
    [SerializeField] private float deathEffectDuration = 0.5f; 

    private int currentHealth;
    private float regenTimer = 0f;
    private bool isDead = false;
    public bool IsDead => isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthOverlay != null)
            healthOverlay.color = new Color(healthOverlay.color.r, healthOverlay.color.g, healthOverlay.color.b, 0);
    }

    private void Update()
    {
        if (isDead) return; 

        UpdateHealthRegeneration();
        UpdateHealthOverlay();
    }

    private void UpdateHealthRegeneration()
    {
        if (currentHealth < maxHealth)
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
        if (healthOverlay == null) return;

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
        if (isDead) return;

        currentHealth -= damage;
        regenTimer = 0f;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");

        DisablePlayerComponents();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogError("Death prefab is not assigned in PlayerHealth!");
        }

        StartCoroutine(DeathEffect());
    }

    private void DisablePlayerComponents()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        foreach (Transform child in transform)
        {
            SpriteRenderer childSr = child.GetComponent<SpriteRenderer>();
            if (childSr != null) childSr.enabled = false;

            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            Animator animator = child.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        UnarmedCombat combat = GetComponent<UnarmedCombat>();
        if (combat != null) combat.enabled = false;

        WeaponInventory inventory = GetComponent<WeaponInventory>();
        if (inventory != null) inventory.enabled = false;
    }

    private System.Collections.IEnumerator DeathEffect()
    {
        if (cursorChanger != null) cursorChanger.SetPauseCursor();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathScreenGroup != null)
        {
            deathScreenGroup.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < deathEffectDuration)
            {
                deathScreenGroup.alpha = Mathf.Lerp(0f, 1f, timer / deathEffectDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            deathScreenGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}