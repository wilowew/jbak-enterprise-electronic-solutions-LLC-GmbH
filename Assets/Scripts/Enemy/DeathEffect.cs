using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [Header("Death Sprites")]
    [SerializeField] private Sprite meleeDeathSprite;
    [SerializeField] private Sprite rangedDeathSprite;
    [SerializeField] private Sprite defaultDeathSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetDeathType(DeathType deathType)
    {
        switch (deathType)
        {
            case DeathType.Melee:
                if (meleeDeathSprite != null)
                    spriteRenderer.sprite = meleeDeathSprite;
                break;

            case DeathType.Ranged:
                if (rangedDeathSprite != null)
                    spriteRenderer.sprite = rangedDeathSprite;
                break;

            default:
                if (defaultDeathSprite != null)
                    spriteRenderer.sprite = defaultDeathSprite;
                break;
        }
    }
}