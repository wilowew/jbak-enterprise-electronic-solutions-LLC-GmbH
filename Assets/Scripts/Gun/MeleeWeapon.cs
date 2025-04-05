using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private Vector3 equippedOffset = new Vector3(0.5f, 0.2f, 0);
    [SerializeField] private float angleOffset = -90f;
    [SerializeField] private float dropDistance = 0.7f;

    private GameObject player;
    private bool isEquipped;
    private Quaternion initialRotation;
    private Rigidbody2D weaponRb;
    private Collider2D weaponCollider;

    private void Awake()
    {
        weaponRb = GetComponent<Rigidbody2D>();
        weaponCollider = GetComponent<Collider2D>();
    }

    public void ForceEquip(GameObject playerObject)
    {
        player = playerObject;
        AttachToPlayer();
    }

    private void AttachToPlayer()
    {
        isEquipped = true;
        weaponRb.simulated = false;
        weaponCollider.enabled = false;
        UpdateWeaponPosition();
    }

    private void UpdateWeaponPosition()
    {
        Vector3 rotatedOffset = player.transform.rotation * equippedOffset;
        transform.position = player.transform.position + rotatedOffset;
        transform.rotation = player.transform.rotation * Quaternion.Euler(0, 0, angleOffset);
    }
}