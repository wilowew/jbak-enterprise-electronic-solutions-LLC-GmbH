using UnityEngine;

public class GateController : MonoBehaviour
{
    private HingeJoint2D hinge; // ��������� � Unity, ������� ��������� ���� �������� ��������� ������ ����� �����.
    private JointMotor2D motor; // ���������, ������� �������� �� ��������� �������������� ���� ���������

    void Start()
    {
        hinge = GetComponent<HingeJoint2D>();

        if (hinge.useMotor)
        {
            motor = hinge.motor;
        }
    }

    void Update()
    {
        if (hinge.jointAngle > 0.1f || hinge.jointAngle < -0.1f)
        {
            hinge.useMotor = true; // �������� �����
            motor.motorSpeed = -hinge.jointAngle * 10f; // ���������� ������� � ������� �������
            hinge.motor = motor;
        }
        else
        {
            hinge.useMotor = false; // ������������� �����, ���� ������� ����� �������
        }
    }
}


