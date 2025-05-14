using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickupBase : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected Vector3 holdOffset = new Vector3(0.5f, 0.2f, 0);
    [SerializeField] protected float rotationOffset = -90f;
    [SerializeField] protected float throwDistance = 0.7f;

    // для хранения состояния
    protected Rigidbody2D itemBody;
    protected Collider2D itemCollider;
    protected Quaternion baseRotation;
    protected bool inPickupRange;
    protected bool isHeld;
    protected GameObject owner;
    protected static WeaponPickupBase currentHeldItem;

    public bool IsHeld => isHeld;
    public bool InPickupRange => inPickupRange;
    public float RotationOffset => rotationOffset;
    public event System.Action<SpriteRenderer> OnEquipped;
    public event System.Action OnDropped;

    private PlayerInput inputSys;
    private InputAction grabAction;
   
    public void SetRotationOffset(float newOffset)
    {
        rotationOffset = newOffset;
        baseRotation = Quaternion.Euler(0f, 0f, rotationOffset);
    }

    private void Awake()
    {
        itemBody = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        inputSys = FindFirstObjectByType<PlayerInput>();
        grabAction = inputSys.actions["Pickup"];
        grabAction.performed += OnGrab;
    }

    private void OnDisable() => grabAction.performed -= OnGrab;

    private void Update()
    {
        if (isHeld) UpdateHoldPosition();
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player")) inPickupRange = true;
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        if (c.CompareTag("Player")) inPickupRange = false;
    }

    // original pickup/drop — оставляем, но НЕ используем его из инвентаря
    protected virtual void OnGrab(InputAction.CallbackContext ctx)
    {
        if (isHeld) ReleaseItem();
        else if (inPickupRange) TakeItem();
    }

    protected virtual void TakeItem()
    {
        if (currentHeldItem != null) currentHeldItem.ReleaseItem();
        currentHeldItem = this;
        isHeld = true;

        itemBody.simulated = false;
        itemCollider.enabled = false;
        baseRotation = Quaternion.Euler(0, 0, rotationOffset);
        UpdateHoldPosition();

        owner = inputSys.gameObject;
        OnEquipped?.Invoke(owner.GetComponent<SpriteRenderer>());
    }

    public virtual void ReleaseItem()
    {
        isHeld = false;
        currentHeldItem = null;

        itemBody.simulated = true;
        itemCollider.enabled = true;
        transform.SetParent(null);

        var dir = (transform.position - owner.transform.position).normalized;
        transform.position = owner.transform.position + dir * throwDistance;
        OnDropped?.Invoke();
    }

    public virtual void UpdateHoldPosition()
    {
        transform.position = owner.transform.position + (owner.transform.rotation * holdOffset);
        transform.rotation = owner.transform.rotation * baseRotation;
    }

    // === Новое API для инвентаря ===

    /// <summary>Спрятать в инвентарь (отключить рендер/физику)</summary>
    public void StoreInInventory()
    {
        if (isHeld) ReleaseItem();
        gameObject.SetActive(false);
    }

    /// <summary>Достать из инвентаря прямо в руки</summary>
    public void EquipFromInventory(GameObject newOwner)
    {
        owner = newOwner;
        isHeld = true;
        currentHeldItem = this;

        gameObject.SetActive(true);
        itemBody.simulated = false;
        itemCollider.enabled = false;

        baseRotation = Quaternion.Euler(0, 0, rotationOffset);
        UpdateHoldPosition();
        OnEquipped?.Invoke(owner.GetComponent<SpriteRenderer>());
    }

    /// <summary>Выбросить из инвентаря/рук в мир</summary>
    public void DropToWorld(Vector3 dropPos, Vector3 throwDir)
    {
        if (isHeld) ReleaseItem();
        else
        {
            gameObject.SetActive(true);
            itemBody.simulated = true;
            itemCollider.enabled = true;
            transform.SetParent(null);
        }

        transform.position = dropPos + throwDir.normalized * throwDistance;
        OnDropped?.Invoke();
    }
}
