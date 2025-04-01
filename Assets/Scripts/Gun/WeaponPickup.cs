using UnityEngine.InputSystem;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private Vector3 equippedOffset = new Vector3(0.5f, 0.2f, 0);
    [SerializeField] private float angleOffset = -90f;
    [SerializeField] private float dropDistance = 0.7f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;

    private GameObject player;
    private bool canPickup;
    private bool isEquipped;
    private Quaternion initialRotation;
    private Rigidbody2D weaponRb;
    private Collider2D weaponCollider;
    private static WeaponPickup currentEquippedWeapon;
    private float nextFireTime;
    private AudioSource audioSource;

    private PlayerInput playerInput;
    private InputAction pickupAction;
    private InputAction shootAction;

    private void Awake()
    {
        weaponRb = GetComponent<Rigidbody2D>();
        weaponCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();

        pickupAction = playerInput.actions["Pickup"];
        shootAction = playerInput.actions["Shoot"];

        pickupAction.performed += OnPickupPerformed;
    }

    private void OnDisable()
    {
        pickupAction.performed -= OnPickupPerformed;
    }

    private void Update()
    {
        if (isEquipped)
        {
            UpdateWeaponPosition();
            HandleShooting();
        }
    }

    private void HandleShooting()
    {
        if (FindFirstObjectByType<PauseManager>().IsPaused)
        {
            return;
        }

        if (shootAction.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            if (shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    private void OnPickupPerformed(InputAction.CallbackContext context)
    {
        if (FindFirstObjectByType<PauseManager>().IsPaused)
        {
            return;
        }
        if (isEquipped)
        {
            DropWeapon();
        }
        else if (canPickup)
        {
            if (currentEquippedWeapon != null)
            {
                currentEquippedWeapon.DropWeapon();
            }
            AttachToPlayer();
        }
    }

    private void UpdateWeaponPosition()
    {
        Vector3 rotatedOffset = player.transform.rotation * equippedOffset;
        transform.position = player.transform.position + rotatedOffset;
        transform.rotation = player.transform.rotation * initialRotation;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            canPickup = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
        }
    }

    private void AttachToPlayer()
    {
        isEquipped = true;
        currentEquippedWeapon = this;

        weaponRb.linearVelocity = Vector2.zero;
        weaponRb.angularVelocity = 0f;
        weaponRb.simulated = false;
        weaponCollider.enabled = true;

        initialRotation = Quaternion.Euler(0, 0, angleOffset);
        UpdateWeaponPosition();
    }

    public void DropWeapon()
    {
        isEquipped = false;
        currentEquippedWeapon = null;

        weaponRb.simulated = true;
        weaponCollider.enabled = false;
        transform.SetParent(null);

        Vector3 dropDirection = (transform.position - player.transform.position).normalized;
        transform.position = player.transform.position + dropDirection * dropDistance;
    }
}