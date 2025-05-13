// AmmoMagazinePickup.cs
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class AmmoMagazinePickup : MonoBehaviour
{
    [Header("��������� ��������")]
    [Tooltip("ID ������ (�� FirearmShooting.WeaponID), ��� �������� ���� �������")]
    [SerializeField] private string targetWeaponID;
    [Tooltip("���������: ������� �������� � ���� �������� (������ ��������� � bulletsPerMagazine)")]
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

        // ���� � ����� ������� ������ ������� ����
        var held = FindObjectsOfType<FirearmShooting>()
            .FirstOrDefault(fs =>
                fs.PickupLogic.IsHeld &&
                fs.WeaponID == targetWeaponID
            );

        if (held == null) return;

        // ������� �������� � ���� true, ������� ������
        if (held.TryAddMagazine())
            Destroy(gameObject);
    }
}
