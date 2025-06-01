using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickupBase : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected Vector3 holdOffset = new Vector3(0.5f, 0.2f, 0);
    [SerializeField] protected float rotationOffset = -90f;
    [SerializeField] protected float throwDistance = 0.7f;

    protected Rigidbody2D itemBody;
    protected Collider2D itemCollider;
    protected Quaternion baseRotation;
    [SerializeField] protected GameObject owner;
    protected static WeaponPickupBase currentHeldItem;


    [Header("Visuals")]
    [SerializeField] private Sprite equippedPlayerSprite;
    public Sprite EquippedPlayerSprite => equippedPlayerSprite;

    public GameObject Owner => owner;

    // Изменено: Используем свойство для автоматического обновления baseRotation
    public float RotationOffset
    {
        get => rotationOffset;
        set
        {
            rotationOffset = value;
            baseRotation = Quaternion.Euler(0f, 0f, rotationOffset);
        }
    }

    public bool IsHeld { get; private set; }
    public bool InPickupRange { get; private set; }

    public event System.Action<SpriteRenderer> OnEquipped;
    public event System.Action OnDropped;

    private PlayerInput playerInput;

    public Vector3 HoldOffset
    {
        get => holdOffset;
        set
        {
            holdOffset = value;
            UpdateHoldPosition(); // Обновляем позицию при изменении
        }
    }

    private void Awake()
    {
        itemBody = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        baseRotation = Quaternion.Euler(0f, 0f, rotationOffset);
    }

    private void OnEnable()
    {
        // Ищем PlayerInput только если не установлен
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
    }

    private void Update()
    {
        if (IsHeld)
            UpdateHoldPosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            InPickupRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            InPickupRange = false;
    }

    public void TryPickup(PlayerInput newOwnerInput)
    {
        if (!IsHeld && InPickupRange)
            TakeItem(newOwnerInput);
    }

    protected virtual void TakeItem(PlayerInput newOwnerInput)
    {
        if (currentHeldItem != null)
            currentHeldItem.ReleaseItem();

        currentHeldItem = this;
        IsHeld = true;
        owner = newOwnerInput.gameObject;
        playerInput = newOwnerInput;

        itemBody.simulated = false;
        itemCollider.enabled = false;

        UpdateHoldPosition();
        OnEquipped?.Invoke(owner.GetComponent<SpriteRenderer>());
    }

    public virtual void ReleaseItem()
    {
        IsHeld = false;
        currentHeldItem = null;

        itemBody.simulated = true;
        itemCollider.enabled = true;
        transform.SetParent(null);

        if (owner != null)
        {
            Vector3 dir = (transform.position - owner.transform.position).normalized;
            transform.position = owner.transform.position + dir * throwDistance;
        }

        OnDropped?.Invoke();
        owner = null;
        playerInput = null;
    }

    public virtual void UpdateHoldPosition()
    {
        if (owner != null)
        {
            transform.position = owner.transform.position + (owner.transform.rotation * holdOffset);
            transform.rotation = owner.transform.rotation * baseRotation;
        }
    }

    // === Inventory API ===
    public void StoreInInventory()
    {
        if (IsHeld)
            ReleaseItem();

        gameObject.SetActive(false);
    }

    public void EquipFromInventory(GameObject newOwner)
    {
        owner = newOwner;
        IsHeld = true;
        currentHeldItem = this;

        gameObject.SetActive(true);
        itemBody.simulated = false;
        itemCollider.enabled = false;

        UpdateHoldPosition();
        OnEquipped?.Invoke(owner.GetComponent<SpriteRenderer>());

    }

    public virtual void RestoreState()
    {
        StoreInInventory();
        gameObject.SetActive(false);
    }

    public void DropToWorld(Vector3 dropPos, Vector3 throwDir)
    {
        if (IsHeld)
            ReleaseItem();
        else
        {
            gameObject.SetActive(true);
            itemBody.simulated = true;
            itemCollider.enabled = false;
        }

        transform.position = dropPos + throwDir.normalized * throwDistance;
        OnDropped?.Invoke();
    }
}