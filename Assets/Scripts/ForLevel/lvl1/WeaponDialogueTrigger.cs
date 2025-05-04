using UnityEngine;

public class WeaponDialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue pickupDialogue;
    private bool hasTriggered = false;
    private WeaponPickupBase weaponPickup;

    private void Awake()
    {
        weaponPickup = GetComponent<WeaponPickupBase>();
        if (weaponPickup != null)
        {
            weaponPickup.OnEquipped += HandleWeaponPickup;
        }
    }

    private void OnDestroy()
    {
        if (weaponPickup != null)
        {
            weaponPickup.OnEquipped -= HandleWeaponPickup;
        }
    }

    private void HandleWeaponPickup(SpriteRenderer sr)
    {
        if (hasTriggered) return;

        hasTriggered = true;
        DialogueManager.Instance.StartDialogue(pickupDialogue);

        weaponPickup.OnEquipped -= HandleWeaponPickup;
    }
}
