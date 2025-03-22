using UnityEngine;

public class PlayerLegs : MonoBehaviour
{
    private PlayerMovement parent;
    private Animator animator;
    private float targetLegsRotation; // Целевое направление ног
    private float rotationSpeed = 10f; // Скорость поворота ног
    private Vector2 lastMovementDirection; // Последнее направление движения

    public void SetParent(PlayerMovement parent)
    {
        this.parent = parent;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        targetLegsRotation = transform.eulerAngles.z; // Инициализируем целевое направление ног
    }

    private void Update()
    {
        if (parent != null && animator != null)
        {
            // Обновляем параметр IsMoving в аниматоре
            animator.SetBool("IsMoving", parent.isMoving);

            // Получаем текущий поворот корпуса (направление на курсор)
            float parentRotation = parent.transform.eulerAngles.z;

            // Если игрок движется, обновляем направление движения
            if (parent.isMoving)
            {
                // Получаем направление движения из скорости Rigidbody2D
                Vector2 movementDirection = parent.Rb.linearVelocity.normalized;

                // Вычисляем угол поворота ног на основе направления движения
                float movementAngle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;

                // Вычисляем разницу между направлением корпуса и направлением движения
                float angleDifference = Mathf.DeltaAngle(parentRotation, movementAngle);

                // Если угол между корпусом и движением находится в диапазоне от -70 до 70 градусов
                if (Mathf.Abs(angleDifference) <= 70f)
                {
                    // Ноги поворачиваются в сторону движения
                    targetLegsRotation = movementAngle;
                }
                else
                {
                    // Если игрок движется в противоположную сторону, ноги не поворачиваются
                    targetLegsRotation = parentRotation;
                }
            }
            else
            {
                // Если игрок не движется, сбрасываем последнее направление движения
                lastMovementDirection = Vector2.zero;

                // Проверяем разницу между корпусом и ногами
                float angleDifference = Mathf.DeltaAngle(targetLegsRotation, parentRotation);
                if (Mathf.Abs(angleDifference) > 70f)
                {
                    // Если разница больше 70 градусов, поворачиваем ноги в сторону корпуса
                    targetLegsRotation = parentRotation;
                }
            }

            // Плавно поворачиваем ноги к целевому направлению
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0, 0, targetLegsRotation),
                Time.deltaTime * rotationSpeed
            );
        }
    }
}