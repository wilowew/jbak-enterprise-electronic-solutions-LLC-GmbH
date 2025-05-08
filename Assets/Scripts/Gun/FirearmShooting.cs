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
    [SerializeField] private AudioClip shotSound;

    [Header("Visuals")]
    [SerializeField] private Sprite playerHoldingSprite;
    private Sprite originalPlayerSprite;

    [Header("Ammo Settings")]
    [Tooltip("Сколько патронов в одном магазине")]
    [SerializeField] private int bulletsPerMagazine = 30;
    [Tooltip("Сколько магазинов максимум можно иметь в запасе")]
    [SerializeField] private int maxMagazines = 5;
    [Tooltip("UI Text для отображения боезапаса (например, '12/90')")]
    [SerializeField] private Text ammoUIText;

    // Текущий боезапас (в патронах)
    private int currentBullets = 0;
    private float nextShotTime;
    private InputAction fireAction;
    private AudioSource soundSource;
    private WeaponPickupBase pickupLogic;
    private SpriteRenderer playerSpriteRenderer;
    public WeaponPickupBase PickupLogic => pickupLogic;

    // Кроме того, для надёжного сравнения типа оружия:
    [Header("Type ID")]
    [Tooltip("Уникальный идентификатор (например, имя префаба) этого оружия")]
    [SerializeField] private string weaponID;
    public string WeaponID => weaponID;
    private void Awake()
    {
        pickupLogic = GetComponent<WeaponPickupBase>();
        soundSource = GetComponent<AudioSource>();
        if (soundSource == null)
        {
            // если не нашли — создаём автоматически
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
    }

    private void OnDisable()
    {
        pickupLogic.OnEquipped -= HandleWeaponEquipped;
        pickupLogic.OnDropped -= HandleWeaponDropped;
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
        if (currentBullets <= 0) return; // нет патронов

        nextShotTime = Time.time + 1f / shotsPerSecond;
        Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        currentBullets--;
        UpdateUI();

        if (shotSound != null)
            soundSource.PlayOneShot(shotSound);
    }

    private void HandleWeaponEquipped(SpriteRenderer playerRenderer)
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
            playerSpriteRenderer.sprite = originalPlayerSprite;
    }

    /// <summary>
    /// Добавить один магазин (bulletsPerMagazine патронов), 
    /// если не превышен лимит maxMagazines.
    /// Возвращает true, если взяли, false — если запас полный.
    /// </summary>
    public bool TryAddMagazine(int bullets)
    {
        int currentMags = currentBullets / bulletsPerMagazine;
        if (currentMags >= maxMagazines)
            return false;

        currentBullets = Mathf.Min(
            currentBullets + bullets,
            maxMagazines * bulletsPerMagazine
        );
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (ammoUIText != null)
        {
            int magsInReserve = currentBullets / bulletsPerMagazine;
            int bulletsInCurrent = currentBullets % bulletsPerMagazine;
            // Формат "текущие патроны / общий запас"
            ammoUIText.text = $"{bulletsInCurrent}/{currentBullets}";
        }
    }
}
