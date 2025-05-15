using UnityEngine;
using UnityEngine.InputSystem;

public class Key : MonoBehaviour
{
    [SerializeField] private Door targetDoor;
    [SerializeField] private float pickupRadius = 1.5f;

    private bool isInRange;
    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        ((CircleCollider2D)triggerCollider).radius = pickupRadius;
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isInRange = false;
    }

    public void PickUp()
    {
        if (!isInRange) return;

        if (targetDoor != null)
        {
            targetDoor.AddKey();
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}