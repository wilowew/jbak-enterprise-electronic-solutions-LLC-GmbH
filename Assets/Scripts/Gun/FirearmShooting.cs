using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponPickupBase))]
public class FirearmShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shotsPerSecond = 5f;
    [SerializeField] private AudioClip shotSound;

    [Header("Visuals")]
    [SerializeField] private Sprite playerHoldingSprite;
    // ���� Unity ������� ������������ ������ ��� �������
    private Sprite originalPlayerSprite;

    private InputAction fireAction;
    private float nextShotTime;
    private AudioSource soundSource;
    private WeaponPickupBase pickupLogic;
    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        pickupLogic = GetComponent<WeaponPickupBase>();
        soundSource = GetComponent<AudioSource>();
        if (soundSource == null)
            soundSource = gameObject.AddComponent<AudioSource>();

        // ������������� �� ������� �������/������
        pickupLogic.OnEquipped += HandleWeaponEquipped;
        pickupLogic.OnDropped += HandleWeaponDropped;
    }

    private void OnEnable()
    {
        var input = FindFirstObjectByType<PlayerInput>();
        fireAction = input.actions["Shoot"];
    }

    private void OnDisable()
    {
        // ����������� ����������
        pickupLogic.OnEquipped -= HandleWeaponEquipped;
        pickupLogic.OnDropped -= HandleWeaponDropped;
    }

    private void Update()
    {
        if (pickupLogic.IsHeld)
            HandleFiring();
    }

    private void HandleFiring()
    {
        if (FindFirstObjectByType<PauseManager>().IsPaused) return;
        if (!fireAction.IsPressed() || Time.time < nextShotTime) return;

        nextShotTime = Time.time + 1f / shotsPerSecond;
        Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        if (shotSound != null)
            soundSource.PlayOneShot(shotSound);
    }

    private void HandleWeaponEquipped(SpriteRenderer playerRenderer)
    {
        // ��������� ������ �� ������-�������� ������ � ��� ������������ ������
        playerSpriteRenderer = playerRenderer;
        if (playerHoldingSprite != null)
        {
            originalPlayerSprite = playerSpriteRenderer.sprite;
            playerSpriteRenderer.sprite = playerHoldingSprite;
        }
    }

    private void HandleWeaponDropped()
    {
        if (playerSpriteRenderer != null && originalPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = originalPlayerSprite;
        }
    }
}
