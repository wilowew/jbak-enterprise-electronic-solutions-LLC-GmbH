using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class AmmoMagazinePickup : MonoBehaviour
{
    [Header("Настройки магазина")]
    [Tooltip("ID оружия (из FirearmShooting.WeaponID), для которого этот магазин")]
    [SerializeField] private string targetWeaponID;
    [Tooltip("Сколько патронов в этом магазине")]
    [SerializeField] private int bulletsInMagazine = 30;

    private bool inRange;
    private GameObject player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            player = null;
        }
    }

    private void Update()
    {
        if (!inRange || player == null) return;

        // Нажимаем E
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        // Находим в сцене ВСЕ FirearmShooting, фильтруем то, которое сейчас в руках
        var heldGun = FindObjectsOfType<FirearmShooting>()
            .FirstOrDefault(fs =>
                fs.PickupLogic.IsHeld &&
                fs.WeaponID == targetWeaponID
            );

        if (heldGun == null)
            return; // либо нет огнестрела в руках, либо не тот ID

        // Пытаемся добавить магазин
        if (heldGun.TryAddMagazine(bulletsInMagazine))
        {
            Destroy(gameObject);
        }
        else
        {
            // Запас полный — тут можно визуально сообщить игроку
            Debug.Log("Ammo full");
        }
    }
}
