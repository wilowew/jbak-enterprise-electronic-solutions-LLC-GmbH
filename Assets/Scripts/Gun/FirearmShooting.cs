// FirearmShooting.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(WeaponPickupBase))]
public class FirearmShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shotsPerSecond = 5f;

    [Header("Visuals")]
    [SerializeField] private Sprite playerHoldingSprite;
    private Sprite originalPlayerSprite;

    [Header("Ammo Settings")]
    [SerializeField] private int bulletsPerMagazine = 30;
    [SerializeField] private int maxReserveMagazines = 5;
    [SerializeField] private Text ammoUIText;

    [Header("Audio")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip emptyMagSound;
    [SerializeField] private AudioClip reloadSound;

    // --- ��������� ---
    private int currentBulletsInMag = 0;
    private int reserveMagazineCount = 0;
    private float nextShotTime;
    private InputAction fireAction;
    private AudioSource soundSource;
    private WeaponPickupBase pickupLogic;
    private SpriteRenderer playerSpriteRenderer;

    // ��� ���������
    [Header("Type ID")]
    [SerializeField] private string weaponID;
    public string WeaponID => weaponID;
    public WeaponPickupBase PickupLogic => pickupLogic;
    private InputAction reloadAction;

    private void OnDisable()
    {
        reloadAction.performed -= OnReloadPressed;
        pickupLogic.OnEquipped -= HandleWeaponEquipped;
        pickupLogic.OnDropped -= HandleWeaponDropped;
    }

    private void OnReloadPressed(InputAction.CallbackContext ctx)
    {
        if (pickupLogic.IsHeld) // ������ ���� ��� ������ � �����
        {
            ManualReload();
        }
    }

    private void Awake()
    {
        pickupLogic = GetComponent<WeaponPickupBase>();

        soundSource = GetComponent<AudioSource>();
        if (soundSource == null)
        {
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.playOnAwake = false;
        }

        pickupLogic.OnEquipped += HandleWeaponEquipped;
        pickupLogic.OnDropped += HandleWeaponDropped;
    }

    private void OnEnable()
    {
        var input = FindFirstObjectByType<PlayerInput>();
        fireAction = input.actions["Shoot"];
        reloadAction = input.actions["Drop"];
        reloadAction.performed += OnReloadPressed;
    }


    private void Update()
    {
        if (!pickupLogic.IsHeld) return;
        HandleFiring();
    }

    private void HandleFiring()
    {
        if (FindFirstObjectByType<PauseManager>().IsPaused) return;
        if (!fireAction.IsPressed() || Time.time < nextShotTime) return;

        nextShotTime = Time.time + 1f / shotsPerSecond;

        if (currentBulletsInMag > 0)
        {
            // ��������
            Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            currentBulletsInMag--;
            PlaySound(shotSound);
        }
        else
        {
            // ������� ������
            PlaySound(emptyMagSound);
        }

        UpdateUI();
    }

    public void HandleWeaponEquipped(SpriteRenderer playerRenderer)
    {
        playerSpriteRenderer = playerRenderer;
        if (playerHoldingSprite != null)
        {
            originalPlayerSprite = playerSpriteRenderer.sprite;
            playerSpriteRenderer.sprite = playerHoldingSprite;
        }
        UpdateUI();
    }

    private void HandleWeaponDropped()
    {
        if (playerSpriteRenderer != null && originalPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = originalPlayerSprite;
            // ����� ������ �� ������ ������
            playerSpriteRenderer = null;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && soundSource != null)
            soundSource.PlayOneShot(clip);
    }

    private void UpdateUI()
    {
        if (ammoUIText == null) return;
        int reserveBullets = reserveMagazineCount * bulletsPerMagazine;
        ammoUIText.text = $"{currentBulletsInMag}/{reserveBullets}";
    }

    /// <summary>
    /// ���������� ����������� ���� ������ � ����� ��� ����� ������������, ���� ���� ����.
    /// ���������� true, ���� ������ ���� ������� (� ����� ��� � �������) � ������-����� ������������.
    /// </summary>
    public bool TryAddMagazine()
    {
        // ���� � ������� ����� ����� � ����� ���������� ���
        if (currentBulletsInMag == 0)
        {
            currentBulletsInMag = bulletsPerMagazine;
            PlaySound(reloadSound);
            UpdateUI();
            return true;
        }

        // ����� � �������� �������� � �����
        if (reserveMagazineCount < maxReserveMagazines)
        {
            reserveMagazineCount++;
            UpdateUI();
            return true;
        }

        // ������ �����
        return false;
    }

    /// <summary>
    /// ������ ����������� (Q): ���� ���� �� ������ � ���� ������ � ������,
    /// ��������� ���� �� ������ � ������� ���� � ������ ����.
    /// </summary>
    public void ManualReload()
    {
        if (reserveMagazineCount > 0 && currentBulletsInMag < bulletsPerMagazine)
        {
            reserveMagazineCount--;
            currentBulletsInMag = bulletsPerMagazine;
            PlaySound(reloadSound);
            UpdateUI();
        }
    }
}