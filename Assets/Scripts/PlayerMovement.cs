using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // ����������
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ������� ��������� Rigidbody2D
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // ����������� ��������� � ������ ����������
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(moveX, moveY).normalized; // ������ ��������������� ������, �� �������� ���������� ����������� ���������
        rb.linearVelocity = movement * moveSpeed;

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
   