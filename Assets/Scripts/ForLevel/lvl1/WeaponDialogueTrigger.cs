using UnityEngine;

public class WeaponDialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue pickupDialogue;
    private bool hasTriggered = false;

    private void OnEnable()
    {
        WeaponPickup.OnWeaponEquipped += HandleWeaponPickup;
    }

    private void OnDisable()
    {
        WeaponPickup.OnWeaponEquipped -= HandleWeaponPickup;
    }

    private void HandleWeaponPickup(WeaponPickup weapon)
    {
        if (weapon == GetComponent<WeaponPickup>())
        {
            hasTriggered = true;
            DialogueManager.Instance.StartDialogue(pickupDialogue);

            WeaponPickup.OnWeaponEquipped -= HandleWeaponPickup;
        }
    }
}
