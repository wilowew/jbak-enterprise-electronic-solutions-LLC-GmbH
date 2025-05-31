using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class WeaponInventory : MonoBehaviour
{
    [Header("Starting Weapons")]
    [SerializeField] private WeaponPickupBase[] startingWeapons;

    [SerializeField] private PlayerWeaponSpriteHandler spriteHandler;

    [Header("Settings")]
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private float pickupRadius = 2f;

    private WeaponPickupBase[] slots;
    private int currentIndex = -1;
    private List<WeaponPickupBase> nearbyWeapons = new List<WeaponPickupBase>();
    private PlayerInput playerInput;
    private Collider2D detectionCollider;

    private List<Key> nearbyKeys = new List<Key>();

    private void Awake()
    {
        slots = new WeaponPickupBase[maxSlots];
        playerInput = GetComponent<PlayerInput>();

        InitializeDetectionCollider();

        if (spriteHandler == null)
            spriteHandler = GetComponent<PlayerWeaponSpriteHandler>();
    }

    private void Start()
    {
        InitializeStartingWeapons();
    }

    private void InitializeDetectionCollider()
    {
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

    private void InitializeStartingWeapons()
    {
        if (startingWeapons != null && startingWeapons.Length > 0)
        {
            foreach (WeaponPickupBase weaponPrefab in startingWeapons)
            {
                WeaponPickupBase weaponInstance = Instantiate(
                    weaponPrefab,
                    transform.position,
                    Quaternion.identity
                );
                weaponInstance.gameObject.SetActive(false);
                AddStartingWeapon(weaponInstance);
            }

            if (currentIndex != -1 && spriteHandler != null)
            {
                spriteHandler.UpdateWeaponSprite(slots[currentIndex]);
            }
        }
    }

    private void AddStartingWeapon(WeaponPickupBase weapon)
    {
        int emptySlot = FindEmptySlot();
        if (emptySlot == -1)
        {
            Debug.LogWarning("No empty slot for starting weapon.");
            return;
        }

        slots[emptySlot] = weapon;
        weapon.StoreInInventory();

        if (currentIndex == -1)
        {
            EquipSlot(emptySlot);
        }
        else
        {
            if (spriteHandler != null)
                spriteHandler.UpdateWeaponSprite(slots[currentIndex]);
        }
    }

    private void HandlePickupDrop()
    {
        if (TryPickupKey())
            return;

        if (TryPickupNearest())
            return;

        if (currentIndex >= 0 && slots[currentIndex] != null)
            DropCurrent();
    }

    private bool TryPickupKey()
    {
        if (nearbyKeys.Count == 0)
            return false;

        Key closestKey = null;
        float closestDistance = Mathf.Infinity;

        foreach (Key key in nearbyKeys)
        {
            if (key == null) continue;

            float distance = Vector2.Distance(transform.position, key.transform.position);
            if (distance < closestDistance)
            {
                closestKey = key;
                closestDistance = distance;
            }
        }

        if (closestKey != null)
        {
            closestKey.PickUp();
            nearbyKeys.Remove(closestKey);
            return true;
        }

        return false;
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
        {
            Debug.Log("Weapon already in inventory");
            return;
        }

        UnarmedCombat unarmed = GetComponent<UnarmedCombat>();
        if (unarmed != null) unarmed.CancelAttack();

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

        if (slots[currentIndex].TryGetComponent<MeleeWeapon>(out var melee))
        {
            melee.HandleWeaponEquipped(GetComponent<SpriteRenderer>());
        }

        UnarmedCombat unarmed = GetComponent<UnarmedCombat>();
        if (unarmed != null) unarmed.CancelAttack();

        if (spriteHandler != null)
        {
            spriteHandler.ForceUpdateWeaponSprite();
        }
    }

    private void DropCurrent()
    {
        if (currentIndex < 0 || slots[currentIndex] == null)
            return;

        WeaponPickupBase currentWeapon = slots[currentIndex];
        slots[currentIndex] = null;

        currentWeapon.ReleaseItem();
        currentWeapon.DropToWorld(transform.position, transform.right);

        FindNextValidSlot();

        UnarmedCombat unarmed = GetComponent<UnarmedCombat>();
        if (unarmed != null) unarmed.CancelAttack();

        if (currentIndex == -1 && spriteHandler != null)
            spriteHandler.SetUnarmed();
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

        if (spriteHandler != null)
            spriteHandler.SetUnarmed();
    }

    private int CountActiveSlots()
    {
        int count = 0;
        foreach (WeaponPickupBase slot in slots)
            if (slot != null)
                count++;
        return count;
    }

    public WeaponPickupBase GetEquippedWeapon()
    {
        return (currentIndex >= 0) ? slots[currentIndex] : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        WeaponPickupBase weapon = other.GetComponent<WeaponPickupBase>();
        if (weapon != null && !nearbyWeapons.Contains(weapon))
            nearbyWeapons.Add(weapon);

        Key key = other.GetComponent<Key>();
        if (key != null && !nearbyKeys.Contains(key))
            nearbyKeys.Add(key);
    }

    public WeaponPickupBase[] GetWeaponsCopy()
    {
        WeaponPickupBase[] copy = new WeaponPickupBase[slots.Length];
        System.Array.Copy(slots, copy, slots.Length);
        return copy;
    }

    public void RestoreWeapons(WeaponPickupBase[] savedWeapons)
    {
        slots = savedWeapons;
        currentIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].StoreInInventory();
                slots[i].gameObject.SetActive(false);

                if (currentIndex == -1)
                {
                    EquipSlot(i);
                }
            }
        }

        if (currentIndex == -1 && spriteHandler != null)
        {
            spriteHandler.SetUnarmed();
        }
    }

    public bool HasWeaponEquipped()
    {
        return currentIndex >= 0 && slots[currentIndex] != null;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        WeaponPickupBase weapon = other.GetComponent<WeaponPickupBase>();
        if (weapon != null)
            nearbyWeapons.Remove(weapon);

        Key key = other.GetComponent<Key>();
        if (key != null)
            nearbyKeys.Remove(key);
    }
}