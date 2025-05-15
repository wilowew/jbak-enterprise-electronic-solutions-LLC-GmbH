using UnityEngine;

public class PlayerWeaponSpriteHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Weapon Sprites")]
    [SerializeField] private Sprite meleeSprite;
    [SerializeField] private Sprite rangedSprite;
    [SerializeField] private Sprite unarmedSprite;

    private void Awake()
    {
        if (unarmedSprite != null)
        {
            playerSpriteRenderer.sprite = unarmedSprite;
        }
    }

    public void UpdateWeaponSprite(WeaponPickupBase weapon)
    {
        if (weapon == null)
        {
            SetUnarmed();
            return;
        }

        bool isMelee = weapon.GetComponent<MeleeWeapon>() != null;
        bool isRanged = weapon.GetComponent<FirearmShooting>() != null;

        playerSpriteRenderer.sprite = isMelee ? meleeSprite :
                                     isRanged ? rangedSprite :
                                     unarmedSprite;
    }

    public void SetUnarmed()
    {
        playerSpriteRenderer.sprite = unarmedSprite;
    }
}