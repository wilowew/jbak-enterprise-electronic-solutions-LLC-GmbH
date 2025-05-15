using UnityEngine;

public class FriendlyNPC : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; 
    [SerializeField] private float rotationOffset = 90f; 

    void Update()
    {
        if (playerTransform != null)
        {
            RotateTowardsPlayer();
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector2 direction = playerTransform.position - transform.position;
        direction.Normalize(); 

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle - rotationOffset);
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
}