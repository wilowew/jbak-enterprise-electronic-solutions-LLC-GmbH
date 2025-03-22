using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // ����������
    private Rigidbody2D rb;
    public bool isMoving = false;

    public Rigidbody2D Rb 
    {
        get { return rb; }
    }

    private PauseManager pauseManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ������� ��������� Rigidbody2D
        pauseManager = Object.FindFirstObjectByType<PauseManager>(); // ����� ����� Unity �� ������ �������
        // �������� ������ �� ���� ��������� �������
        PlayerLegs child = GetComponentInChildren<PlayerLegs>();
        if (child != null)
        {
            child.SetParent(this);
        }
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // ����������� ��������� � ������ ����������
        float moveY = Input.GetAxisRaw("Vertical");

        if (pauseManager != null && pauseManager.IsPaused)
        {
            rb.linearVelocity = Vector2.zero; // ������������� ��������, ���� �����
            isMoving = false;
            return;
        }

        // ���������, ���� �� ��������
        if (moveX != 0 || moveY != 0)
        {
            Vector2 movement = new Vector2(moveX, moveY).normalized; // ������ ��������������� ������, �� �������� ���������� ����������� ���������
            rb.linearVelocity = movement * moveSpeed;
            isMoving = true; // �������� ���������
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // ������������� ��������
            isMoving = false; // �������� ����� �� �����
        }

        RotateTowardsMouse();
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ������� ������� � ������� ����������� �����
        mousePosition.z = 0;

        Vector3 direction = (mousePosition - transform.position).normalized; // ���������� ����������� �� ��������� � �������
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // ���������� ���� �������� � ��� ����������
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}