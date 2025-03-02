using UnityEngine;
using UnityEngine.Rendering;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.125f; 
    public Vector3 offset;
    public float edgeThreshold = 0.2f; // Определеяет, насколько близко курсор находится к краю экрана, чтобы камера начала смещаться
    public float maxLookAhead = 2f; // Задаёт максимальную дистанцию перемещения камеры за курсором

    private Vector3 velocity = Vector3.zero; 

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target is not assigned!  Please assign the player object to the 'Target' field.");
            return; 
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Определеяем положение курсора
        mousePosition.z = 0;

        Vector3 directionToMouse = (mousePosition - target.position).normalized; // Определяем направление от игрока к курсору
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(mousePosition);
        float edgeX = Mathf.Clamp((screenPoint.x - 0.5f) * 2, -1, 1); // Преобразует позицию курсора в нормализованные значения, где (0, 0) - левый нижний угол экрана, а (1, 1) - правый верхний
        float edgeY = Mathf.Clamp((screenPoint.y - 0.5f) * 2, -1, 1);

        Vector3 lookAhead = Vector3.zero;
        if (Mathf.Abs(edgeX) > edgeThreshold || Mathf.Abs(edgeY) > edgeThreshold) // Если курсор близок к краю экрана, то добавляем смещение
        {
            lookAhead = directionToMouse * maxLookAhead;
        }

        Vector3 desiredPosition = target.position + offset + lookAhead; // Желаемая позиция камеры с учётом всех изменений
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed); // Плавное перемещение камеры
        transform.position = smoothedPosition;
    }
  
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: No GameObject with the 'Player' tag found.  Please assign the player object to the 'Target' field manually, or add the 'Player' tag to your player GameObject.");
            }
        }
    }
}
