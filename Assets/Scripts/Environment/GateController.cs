using UnityEngine;

public class GateController : MonoBehaviour
{
    private HingeJoint2D hinge; // ��������� �������
    private JointMotor2D motor; // ��������� �������

    [Header("Audio Clips")]
    public AudioClip gateCreakSound; // ���� ������ �������

    private bool isPlayerNear = false; // ���� ��� ��������, ��������� �� ����� �����

    private void Start()
    {
        hinge = GetComponent<HingeJoint2D>();

        if (hinge.useMotor)
        {
            motor = hinge.motor;
        }
    }

    private void Update()
    {
        // ���������, �������� �� �������
        if ((hinge.jointAngle > 0.1f || hinge.jointAngle < -0.1f) && isPlayerNear)
        {
            hinge.useMotor = true; // �������� �����
            motor.motorSpeed = -hinge.jointAngle * 10f; // ���������� ������� � �������� ���������
            hinge.motor = motor;

            // ������������� ����, ���� ����� ����� � ���� ��� �� �������������
            if (!AudioManager.Instance.soundEffectsSource.isPlaying)
            {
                AudioManager.Instance.PlaySoundEffect(gateCreakSound);
            }
        }
        else
        {
            hinge.useMotor = false; // ��������� �����, ���� ������� ����� �������
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // ���������, ��� ������������ � �������
        {
            isPlayerNear = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // ���������, ��� ����� ������� ����
        {
            isPlayerNear = false;
        }
    }
}
