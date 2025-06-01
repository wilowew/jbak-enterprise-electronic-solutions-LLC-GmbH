using UnityEngine;

public class PlayerWeaponSpriteHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Weapon Sprites")]
    [SerializeField] private Sprite meleeSprite;
    [SerializeField] private Sprite rangedSprite;
    [SerializeField] private Sprite unarmedSprite;

    private bool needsRefresh = true;

    private void Awake()
    {
        needsRefresh = true;
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

    public void LateUpdate()
    {
        if (needsRefresh)
        {
            RefreshSprite();
            needsRefresh = false;
        }
    }

    private void RefreshSprite()
    {
        WeaponInventory inventory = GetComponent<WeaponInventory>();
        if (inventory != null && inventory.HasWeaponEquipped())
        {
            WeaponPickupBase weapon = inventory.GetEquippedWeapon();

            if (weapon.EquippedPlayerSprite != null)
            {
                playerSpriteRenderer.sprite = weapon.EquippedPlayerSprite;
            }
            else
            {
                playerSpriteRenderer.sprite = unarmedSprite;
            }
        }
        else
        {
            playerSpriteRenderer.sprite = unarmedSprite;
        }
    }

    public void ForceUpdateWeaponSprite() => needsRefresh = true;
    public void SetUnarmed() => needsRefresh = true;

    public void SetUnarmedSprite(Sprite sprite)
    {
        unarmedSprite = sprite;
        if (!GetComponent<WeaponInventory>().HasWeaponEquipped())
            playerSpriteRenderer.sprite = unarmedSprite;
    }

}