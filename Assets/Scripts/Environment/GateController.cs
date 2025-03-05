using UnityEngine;

public class GateController : MonoBehaviour
{
    private HingeJoint2D hinge; // Компонент калитки
    private JointMotor2D motor; // Двигатель калитки

    [Header("Audio Clips")]
    public AudioClip gateCreakSound; // Звук скрипа калитки

    private bool isPlayerNear = false; // Флаг для проверки, находится ли игрок рядом

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
        // Проверяем, движется ли калитка
        if ((hinge.jointAngle > 0.1f || hinge.jointAngle < -0.1f) && isPlayerNear)
        {
            hinge.useMotor = true; // Включаем мотор
            motor.motorSpeed = -hinge.jointAngle * 10f; // Возвращаем калитку в исходное положение
            hinge.motor = motor;

            // Воспроизводим звук, если игрок рядом и звук ещё не проигрывается
            if (!AudioManager.Instance.soundEffectsSource.isPlaying)
            {
                AudioManager.Instance.PlaySoundEffect(gateCreakSound);
            }
        }
        else
        {
            hinge.useMotor = false; // Отключаем мотор, если калитка почти закрыта
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Проверяем, что столкновение с игроком
        {
            isPlayerNear = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Проверяем, что игрок покинул зону
        {
            isPlayerNear = false;
        }
    }
}
