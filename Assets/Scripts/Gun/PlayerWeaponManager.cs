using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public enum InitialWeaponState { Unarmed, Firearm, Melee }
    public InitialWeaponState initialWeaponState = InitialWeaponState.Unarmed;

    [SerializeField] private WeaponPickup startingFirearm;
    [SerializeField] private WeaponPickup startingMelee;

    void Start()
    {
        ApplyInitialWeaponState();
    }

    private void ApplyInitialWeaponState()
    {
        switch (initialWeaponState)
        {
            case InitialWeaponState.Firearm:
                EquipWeapon(startingFirearm);
                break;
            case InitialWeaponState.Melee:
                EquipWeapon(startingMelee);
                break;
            case InitialWeaponState.Unarmed:
                break;
        }
    }

    private void EquipWeapon(WeaponPickup weapon)
    {
        if (weapon != null)
        {
            weapon.ForceEquip(gameObject);
        }
    }
}