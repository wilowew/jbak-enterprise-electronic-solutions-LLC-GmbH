using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class WeaponInventory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private float pickupRadius = 2f;

    private WeaponPickupBase[] slots;
    private int currentIndex = -1;
    private List<WeaponPickupBase> nearbyWeapons = new List<WeaponPickupBase>();
    private PlayerInput playerInput;
    private Collider2D detectionCollider;

    private void Awake()
    {
        slots = new WeaponPickupBase[maxSlots];
        playerInput = GetComponent<PlayerInput>();

        // Настройка триггера для обнаружения оружия
        GameObject detectionArea = new GameObject("PickupDetection");
        detectionArea.transform.SetParent(transform);
        detectionArea.transform.localPosition = Vector3.zero;
        detectionCollider = detectionArea.AddComponent<CircleCollider2D>();
        ((CircleCollider2D)detectionCollider).radius = pickupRadius;
        detectionCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        InputAction grabAction = playerInput.actions["Pickup"];
        grabAction.performed += _ => HandlePickupDrop();
    }

    private void OnDisable()
    {
        InputAction grabAction = playerInput.actions["Pickup"];
        grabAction.performed -= _ => HandlePickupDrop();
    }

    private void Update()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.1f)
            CycleSlots(scroll > 0 ? 1 : -1);
    }

    private void HandlePickupDrop()
    {
        if (TryPickupNearest())
            return;

        if (currentIndex >= 0 && slots[currentIndex] != null)
            DropCurrent();
    }

    private bool TryPickupNearest()
    {
        if (nearbyWeapons.Count == 0)
            return false;

        WeaponPickupBase closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (WeaponPickupBase weapon in nearbyWeapons)
        {
            if (weapon.IsHeld || !weapon.InPickupRange)
                continue;

            float distance = Vector2.Distance(transform.position, weapon.transform.position);
            if (distance < closestDistance)
            {
                closest = weapon;
                closestDistance = distance;
            }
        }

        if (closest != null)
        {
            AddToInventory(closest);
            return true;
        }

        return false;
    }

    private void AddToInventory(WeaponPickupBase weapon)
    {
        if (System.Array.IndexOf(slots, weapon) != -1)
            return;

        int emptySlot = FindEmptySlot();
        if (emptySlot == -1)
            DropCurrent();

        emptySlot = FindEmptySlot();
        if (emptySlot != -1)
        {
            slots[emptySlot] = weapon;
            weapon.StoreInInventory();
            EquipSlot(emptySlot);
        }
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null)
                return i;
        return -1;
    }

    private void CycleSlots(int direction)
    {
        if (CountActiveSlots() < 2) return;

        int startIndex = currentIndex;
        int newIndex = currentIndex;
        int attempts = 0;

        do
        {
            newIndex = (newIndex + direction + slots.Length) % slots.Length;
            attempts++;
        } while (slots[newIndex] == null && attempts < slots.Length);

        if (slots[newIndex] != null && newIndex != currentIndex)
        {
            EquipSlot(newIndex);
        }
    }

    private void EquipSlot(int index)
    {
        if (index < 0 || index >= slots.Length || slots[index] == null) return;

        // Деактивируем предыдущее оружие
        if (currentIndex >= 0 && currentIndex != index && slots[currentIndex] != null)
        {
            slots[currentIndex].StoreInInventory();
        }

        currentIndex = index;

        // Активируем новое
        slots[currentIndex].EquipFromInventory(gameObject);

        Debug.Log($"Equipped slot {currentIndex}");
        if (slots[currentIndex].TryGetComponent<FirearmShooting>(out var firearm))
        {
            firearm.HandleWeaponEquipped(GetComponent<SpriteRenderer>());
        }
    }

    private void DropCurrent()
    {
        if (currentIndex < 0 || slots[currentIndex] == null)
            return;

        WeaponPickupBase currentWeapon = slots[currentIndex];
        slots[currentIndex] = null;

        // Явно вызываем ReleaseItem для триггера OnDropped
        currentWeapon.ReleaseItem();
        currentWeapon.DropToWorld(transform.position, transform.right);

        FindNextValidSlot();
    }

    private void FindNextValidSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int checkIndex = (currentIndex + i) % slots.Length;
            if (slots[checkIndex] != null)
            {
                EquipSlot(checkIndex);
                return;
            }
        }
        currentIndex = -1;
    }

    private int CountActiveSlots()
    {
        int count = 0;
        foreach (WeaponPickupBase slot in slots)
            if (slot != null)
                count++;
        return count;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        WeaponPickupBase weapon = other.GetComponent<WeaponPickupBase>();
        if (weapon != null && !nearbyWeapons.Contains(weapon))
            nearbyWeapons.Add(weapon);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        WeaponPickupBase weapon = other.GetComponent<WeaponPickupBase>();
        if (weapon != null)
            nearbyWeapons.Remove(weapon);
    }
}