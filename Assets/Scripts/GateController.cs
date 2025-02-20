using UnityEngine;

public class GateController : MonoBehaviour
{
    private HingeJoint2D hinge; // компонент в Unity, который позволяет двум объектам вращаться вокруг общей точки.
    private JointMotor2D motor; // компонент, который отвечает за параметры дополнительной силы двигателя

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
            hinge.useMotor = true; // Включаем мотор
            motor.motorSpeed = -hinge.jointAngle * 10f; // Возвращаем калитку в нулевую позицию
            hinge.motor = motor;
        }
        else
        {
            hinge.useMotor = false; // Останавливаем мотор, если калитка почти закрыта
        }
    }
}


