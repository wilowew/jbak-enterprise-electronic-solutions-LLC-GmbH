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

        bool isMelee = weapon.GetComponentInChildren<MeleeWeapon>(includeInactive: true) != null;
        bool isRanged = weapon.GetComponentInChildren<FirearmShooting>(includeInactive: true) != null;

        playerSpriteRenderer.sprite = isMelee ? meleeSprite :
                                     isRanged ? rangedSprite :
                                     unarmedSprite;
    }

    public void LateUpdate()
    {
        if (needsRefresh)
        {
            WeaponInventory inventory = GetComponent<WeaponInventory>();
            if (inventory != null && inventory.HasWeaponEquipped())
            {
                UpdateWeaponSprite(inventory.GetEquippedWeapon());
            }
            needsRefresh = false;
        }
    }

    public void ForceUpdateWeaponSprite()
    {
        WeaponInventory inventory = GetComponent<WeaponInventory>();
        if (inventory != null && inventory.HasWeaponEquipped())
        {
            UpdateWeaponSprite(inventory.GetEquippedWeapon());
        }
        else
        {
            SetUnarmed();
        }
    }

    public void SetUnarmedSprite(Sprite sprite)
    {
        unarmedSprite = sprite;
        if (!GetComponent<WeaponInventory>().HasWeaponEquipped())
            playerSpriteRenderer.sprite = unarmedSprite;
    }

    public void SetUnarmed()
    {
        playerSpriteRenderer.sprite = unarmedSprite;
    }
}