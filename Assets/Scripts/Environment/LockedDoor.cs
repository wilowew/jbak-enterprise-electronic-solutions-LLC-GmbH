using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private HingeJoint2D hinge; 
    [SerializeField] private Rigidbody2D rb;    

    [Header("Settings")]
    [SerializeField] private int keysRequired = 1;
    [SerializeField] private float rotationLimit = 90f;
    [SerializeField] private float doorMass = 50f;

    private bool isLocked = true;
    private int collectedKeys;

    private void Start()
    {
        InitializeDoor();
    }

    private void InitializeDoor()
    {
        if (hinge == null) hinge = GetComponent<HingeJoint2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        hinge.useLimits = true;
        hinge.limits = new JointAngleLimits2D
        {
            min = -rotationLimit,
            max = rotationLimit
        };

        rb.mass = doorMass;
        rb.angularDamping = 2f;
        rb.bodyType = RigidbodyType2D.Kinematic; 
    }

    public void AddKey()
    {
        collectedKeys++;
        if (collectedKeys >= keysRequired)
        {
            UnlockDoor();
        }
    }

    private void UnlockDoor()
    {
        isLocked = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        Debug.Log("Door unlocked!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLocked || collision.contactCount == 0) return;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 direction = (Vector2)transform.position - contact.point;
        rb.AddForceAtPosition(direction.normalized * 5f, contact.point, ForceMode2D.Impulse);
    }

}