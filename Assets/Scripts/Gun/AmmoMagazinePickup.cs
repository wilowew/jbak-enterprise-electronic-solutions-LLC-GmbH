// AmmoMagazinePickup.cs
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class AmmoMagazinePickup : MonoBehaviour
{
    [Header("Настройки магазина")]
    [Tooltip("ID оружия (из FirearmShooting.WeaponID), для которого этот магазин")]
    [SerializeField] private string targetWeaponID;
    [Tooltip("Настройка: сколько патронов в этом магазине (должно совпадать с bulletsPerMagazine)")]
    [SerializeField] private int bulletsInMagazine = 30;

    private bool inRange;
    private GameObject player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = true;
        player = other.gameObject;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = false;
        player = null;
    }

    private void Update()
    {
        if (!inRange || player == null) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        // Ищем в руках текущее оружие нужного типа
        var held = FindObjectsOfType<FirearmShooting>()
            .FirstOrDefault(fs =>
                fs.PickupLogic.IsHeld &&
                fs.WeaponID == targetWeaponID
            );

        if (held == null) return;

        // Попытка добавить — если true, убиваем объект
        if (held.TryAddMagazine())
            Destroy(gameObject);
    }
}
