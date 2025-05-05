// WeaponPickupBase.cs
using UnityEngine.InputSystem;
using UnityEngine;

public class WeaponPickupBase : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected Vector3 holdOffset = new Vector3(0.5f, 0.2f, 0);
    [SerializeField] protected float rotationOffset = -90f;
    [SerializeField] protected float throwDistance = 0.7f;

    // Новый пропертя для доступа к текущему rotationOffset из внешних классов
    public float RotationOffset => rotationOffset;

    // Новый метод для мгновенного смены угла удержания
    public void SetRotationOffset(float newOffset)
    {
        rotationOffset = newOffset;
        // Обновляем внутренний базовый Quaternion, чтобы UpdateHoldPosition сразу рисовал с новым углом
        baseRotation = Quaternion.Euler(0f, 0f, rotationOffset);
    }

    public bool IsHeld => isHeld;
    protected GameObject owner;
    protected bool inPickupRange;
    protected bool isHeld;
    protected Quaternion baseRotation;
    protected Rigidbody2D itemBody;
    protected Collider2D itemCollider;
    protected static WeaponPickupBase currentHeldItem;

    protected PlayerInput inputSystem;
    protected InputAction grabAction;

    public event System.Action<SpriteRenderer> OnEquipped;
    public event System.Action OnDropped;

    private void Awake()
    {
        itemBody = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        inputSystem = FindFirstObjectByType<PlayerInput>();
        grabAction = inputSystem.actions["Pickup"];
        grabAction.performed += OnGrabAction;
    }

    private void OnDisable()
    {
        grabAction.performed -= OnGrabAction;
    }

    private void Update()
    {
        if (isHeld) UpdateHoldPosition();
    }

    public virtual void UpdateHoldPosition()
    {
        Vector3 adjustedOffset = owner.transform.rotation * holdOffset;
        transform.position = owner.transform.position + adjustedOffset;
        transform.rotation = owner.transform.rotation * baseRotation;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            owner = other.gameObject;
            inPickupRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) inPickupRange = false;
    }

    protected virtual void OnGrabAction(InputAction.CallbackContext ctx)
    {
        if (FindFirstObjectByType<PauseManager>().IsPaused) return;

        if (isHeld) ReleaseItem();
        else if (inPickupRange) TakeItem();
    }

    protected virtual void TakeItem()
    {
        if (currentHeldItem != null) currentHeldItem.ReleaseItem();

        currentHeldItem = this;
        isHeld = true;

        itemBody.linearVelocity = Vector2.zero;
        itemBody.angularVelocity = 0f;
        itemBody.simulated = false;
        itemCollider.enabled = false;

        baseRotation = Quaternion.Euler(0, 0, rotationOffset); // задаём Quaternion по rotationOffset
        UpdateHoldPosition();

        OnEquipped?.Invoke(owner.GetComponent<SpriteRenderer>());
    }

    public virtual void ReleaseItem()
    {
        isHeld = false;
        currentHeldItem = null;

        itemBody.simulated = true;
        itemCollider.enabled = true;
        itemCollider.isTrigger = true;
        transform.SetParent(null);

        Vector3 throwDir = (transform.position - owner.transform.position).normalized;
        transform.position = owner.transform.position + throwDir * throwDistance;

        OnDropped?.Invoke();
    }
}
