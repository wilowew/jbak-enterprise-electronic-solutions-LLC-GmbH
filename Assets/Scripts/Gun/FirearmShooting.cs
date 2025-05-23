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

    // ��� ���������
    [Header("Type ID")]
    [SerializeField] private string weaponID;
    public string WeaponID => weaponID;
    public WeaponPickupBase PickupLogic => pickupLogic;
    private InputAction reloadAction;

    private void OnDisable()
    {
        if (ammoUIText != null)
            ammoUIText.text = "";

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

        if (ammoUIText != null)
            ammoUIText.text = "";
    }

    private void OnEnable()
    {
        var input = FindFirstObjectByType<PlayerInput>();
        fireAction = input.actions["Shoot"];
        reloadAction = input.actions["Drop"];
        reloadAction.performed += OnReloadPressed;
        UpdateUI();
    }

    private void Update()
    {
        if (!pickupLogic.IsHeld) return;
        HandleFiring();
    }

    private void HandleFiring()
    {
        if(IsPlayerDead() || FindFirstObjectByType<PauseManager>().IsPaused) return;
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
        if (ammoUIText != null)
            ammoUIText.text = "";

        UpdateUI();
    }

    private bool IsPlayerDead()
    {
        if (pickupLogic == null || pickupLogic.Owner == null) return false;
        PlayerHealth health = pickupLogic.Owner.GetComponent<PlayerHealth>();
        return health != null && health.IsDead;
    }

    private void HandleWeaponDropped()
    {
        if (ammoUIText != null)
            ammoUIText.text = "";
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && soundSource != null)
            soundSource.PlayOneShot(clip);
    }

    private void UpdateUI()
    {
        if (ammoUIText == null || !pickupLogic.IsHeld)
        {
            if (ammoUIText != null)
                ammoUIText.text = "";
            return;
        }

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