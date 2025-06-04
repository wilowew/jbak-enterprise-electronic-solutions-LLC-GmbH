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

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float shotVolume = 1f;
    [Range(0f, 1f)] public float emptyMagVolume = 1f;
    [Range(0f, 1f)] public float reloadVolume = 1f;

    [Header("Events")]
    public System.Action OnAmmoLoaded;

    // --- состояние ---
    public int currentBulletsInMag = 0;
    public int reserveMagazineCount = 0;
    private float nextShotTime;
    private AudioSource soundSource;
    private WeaponPickupBase pickupLogic;

    // для инвентаря
    [Header("Type ID")]
    [SerializeField] private string weaponID;
    public string WeaponID => weaponID;
    public WeaponPickupBase PickupLogic => pickupLogic;
    private InputAction reloadAction;

    public static FirearmShooting CurrentEquipped;


    private void OnDisable()
    {
        if (ammoUIText != null)
            ammoUIText.text = "";

        if (reloadAction != null)
            reloadAction.performed -= OnReloadPressed;

        if (pickupLogic != null)
        {
            pickupLogic.OnEquipped -= HandleWeaponEquipped;
            pickupLogic.OnDropped -= HandleWeaponDropped;
        }
    }

    private void OnReloadPressed(InputAction.CallbackContext ctx)
    {
        if (pickupLogic.IsHeld && CurrentEquipped == this)
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
        if (input != null)
        {
            reloadAction = input.actions["Drop"];
            reloadAction.performed += OnReloadPressed;
        }
        UpdateUI();
    }

    private void Update()
    {
        if (!pickupLogic.IsHeld)
        {
            // Гарантированно скрываем UI, если оружие не в руках
            if (CurrentEquipped == this && ammoUIText != null)
            {
                ammoUIText.text = "";
            }
            return;
        }

        HandleFiring();
    }

    private void HandleFiring()
    {
        if (IsPlayerDead() || FindFirstObjectByType<PauseManager>().IsPaused) return;
        if (!Input.GetMouseButton(0) || Time.time < nextShotTime) return;

        nextShotTime = Time.time + 1f / shotsPerSecond;

        if (currentBulletsInMag > 0)
        {
            Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            currentBulletsInMag--;
            PlaySound(shotSound, shotVolume);
        }
        else
        {
            PlaySound(emptyMagSound, emptyMagVolume);
        }

        UpdateUI();
    }

    public void HandleWeaponEquipped(SpriteRenderer playerRenderer)
    {
        CurrentEquipped = this;
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
        if (CurrentEquipped == this)
        {
            if (ammoUIText != null)
                ammoUIText.text = "";

            CurrentEquipped = null;
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && soundSource != null)
            soundSource.PlayOneShot(clip, volume);
    }


    private void UpdateUI()
    {
        // Всегда скрываем UI, если это не текущее экипированное оружие
        if (ammoUIText == null || CurrentEquipped != this)
        {
            // Дополнительная страховка на случай несоответствия
            if (ammoUIText != null && CurrentEquipped != this)
                ammoUIText.text = "";

            return;
        }

        int reserveBullets = reserveMagazineCount * bulletsPerMagazine;
        ammoUIText.text = $"{currentBulletsInMag}/{reserveBullets}";
    }

    /// <summary>
    /// Попытаться «подобрать» одну обойму в запас или сразу перезарядить, если клип пуст.
    /// Возвращает true, если обойма была принята (в запас или в текущую) и объект-пикап уничтожается.
    /// </summary>
    public bool TryAddMagazine()
    {
        if (currentBulletsInMag == 0)
        {
            currentBulletsInMag = bulletsPerMagazine;
            PlaySound(reloadSound, reloadVolume);
            UpdateUI();

            OnAmmoLoaded?.Invoke();
            return true;
        }

        if (reserveMagazineCount < maxReserveMagazines)
        {
            reserveMagazineCount++;
            UpdateUI();

            OnAmmoLoaded?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ручная перезарядка (Q): если клип не полный и есть обоймы в запасе,
    /// переносим одну из запаса в текущий клип и играем звук.
    /// </summary>
    public void ManualReload()
    {
        if (reserveMagazineCount > 0 && currentBulletsInMag < bulletsPerMagazine)
        {
            reserveMagazineCount--;
            currentBulletsInMag = bulletsPerMagazine;
            PlaySound(reloadSound, reloadVolume);
            UpdateUI();
        }
    }
}