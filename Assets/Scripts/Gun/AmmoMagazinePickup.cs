using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class AmmoMagazinePickup : MonoBehaviour
{
    [Header("��������� ��������")]
    [Tooltip("ID ������ (�� FirearmShooting.WeaponID), ��� �������� ���� �������")]
    [SerializeField] private string targetWeaponID;
    [Tooltip("������� �������� � ���� ��������")]
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

        // �������� E
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        // ������� � ����� ��� FirearmShooting, ��������� ��, ������� ������ � �����
        var heldGun = FindObjectsOfType<FirearmShooting>()
            .FirstOrDefault(fs =>
                fs.PickupLogic.IsHeld &&
                fs.WeaponID == targetWeaponID
            );

        if (heldGun == null)
            return; // ���� ��� ���������� � �����, ���� �� ��� ID

        // �������� �������� �������
        if (heldGun.TryAddMagazine(bulletsInMagazine))
        {
            Destroy(gameObject);
        }
        else
        {
            // ����� ������ � ��� ����� ��������� �������� ������
            Debug.Log("Ammo full");
        }
    }
}
