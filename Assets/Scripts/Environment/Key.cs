using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private Door targetDoor;
    [SerializeField] private float pickupRadius = 1.5f;

    private Transform player;
    private bool isInRange;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        isInRange = distance <= pickupRadius;

        Debug.DrawLine(transform.position, player.position, isInRange ? Color.green : Color.red);

        if (isInRange && Input.GetKeyDown(KeyCode.F))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (targetDoor != null)
        {
            targetDoor.AddKey();
            Destroy(gameObject);
            Debug.Log("Key picked up!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}