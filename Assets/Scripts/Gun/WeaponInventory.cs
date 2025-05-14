using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class WeaponInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Максимальное число слотов")]
    [SerializeField] private int maxSlots = 3;

    private WeaponPickupBase[] slots;
    private int currentIndex = -1;

    private InputAction pickupAction;

    private void Awake()
    {
        slots = new WeaponPickupBase[maxSlots];
    }

    private void OnEnable()
    {
        var inp = GetComponent<PlayerInput>();
        pickupAction = inp.actions["Pickup"];   // F
        pickupAction.performed += _ => OnPickupOrDrop();
    }

    private void OnDisable()
    {
        pickupAction.performed -= _ => OnPickupOrDrop();
    }

    private void Update()
    {
        // колёсико мыши для переключения
        float d = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(d) > 0.1f && slots.Length > 1)
            Cycle(d > 0 ? -1 : 1);
    }

    private void OnPickupOrDrop()
    {
        var candidate = FindNearestInRange();
        if (candidate != null)
        {
            PickUp(candidate);
            return;
        }

        if (currentIndex >= 0 && slots[currentIndex] != null && !slots[currentIndex].IsHeld)
        {
            EquipSlot(currentIndex);
            return;
        }
        DropCurrent();
    }


    private WeaponPickupBase FindNearestInRange()
    {
        var all = Object.FindObjectsOfType<WeaponPickupBase>(true);
        WeaponPickupBase best = null;
        float bestSqr = float.MaxValue;
        Vector3 me = transform.position;

        foreach (var w in all)
        {
            if (!w.IsHeld && w.InPickupRange)
            {
                float sq = ((Vector2)w.transform.position - (Vector2)me).sqrMagnitude;
                if (sq < bestSqr)
                {
                    bestSqr = sq;
                    best = w;
                }
            }
        }
        return best;
    }

    private void PickUp(WeaponPickupBase w)
    {
        // если уже есть в инвентаре — ничего не делаем
        if (System.Array.IndexOf(slots, w) >= 0)
            return;

        // если инвентарь полон — выбрасываем текущее
        if (CountFilled() >= slots.Length)
        {
            DropCurrent();
        }

        // ищем первый пустой слот
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = w;
                // прячем только что подобранное, но не меняем текущее в руках
                w.StoreInInventory();
                return;
            }
        }
    }


    private int CountFilled()
    {
        int c = 0;
        foreach (var s in slots) if (s != null) c++;
        return c;
    }

    private void DropCurrent()
    {
        if (currentIndex < 0 || currentIndex >= slots.Length) return;
        var w = slots[currentIndex];
        if (w == null) return;

        // бросаем в мир вдоль взгляда игрока (transform.right)
        w.DropToWorld(transform.position, transform.right);
        slots[currentIndex] = null;

        // найдём следующий непустой слот
        int next = -1;
        for (int i = 1; i <= slots.Length; i++)
        {
            int idx = (currentIndex + i) % slots.Length;
            if (slots[idx] != null) { next = idx; break; }
        }
        if (next >= 0) EquipSlot(next);
        else currentIndex = -1;
    }

    private void Cycle(int dir)
    {
        if (CountFilled() < 2) return;
        int start = currentIndex;
        int i = currentIndex;
        do
        {
            i = (i + dir + slots.Length) % slots.Length;
        } while (slots[i] == null && i != start);

        if (slots[i] != null)
            EquipSlot(i);
    }

    private void EquipSlot(int idx)
    {
        if (idx == currentIndex) return;

        // 1) спрячем текущее
        if (currentIndex >= 0 && slots[currentIndex] != null)
            slots[currentIndex].StoreInInventory();

        // 2) обновим индекс
        currentIndex = idx;

        // 3) достанем новое
        slots[currentIndex].EquipFromInventory(gameObject);
    }


}
