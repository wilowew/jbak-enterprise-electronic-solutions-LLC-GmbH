using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Переменные
    private Rigidbody2D rb;
    public bool isMoving = false;
    private bool canMove = false;

    public Rigidbody2D Rb 
    {
        get { return rb; }
    }

    private PauseManager pauseManager;

    void Start()
    {
        canMove = true;
        rb = GetComponent<Rigidbody2D>(); // Находим компонент Rigidbody2D
        pauseManager = Object.FindFirstObjectByType<PauseManager>(); // Новый метод Unity по поиску объекта
        // Передаем ссылку на себя дочернему объекту
        PlayerLegs child = GetComponentInChildren<PlayerLegs>();
        if (child != null)
        {
            child.SetParent(this);
        }
    }

    void Update()
    {
        if (!canMove) return;

        float moveX = Input.GetAxisRaw("Horizontal"); // Перемещение персонажа с резкой остановкой
        float moveY = Input.GetAxisRaw("Vertical");

        if (pauseManager != null && pauseManager.IsPaused)
        {
            rb.linearVelocity = Vector2.zero; // Останавливаем движение, если пауза
            isMoving = false;
            return;
        }

        HandleMovement(moveX, moveY);
        RotateTowardsMouse();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleMovement(float moveX, float moveY)
    {
        // Проверяем, есть ли движение
        if (moveX != 0 || moveY != 0)
        {
            Vector2 movement = new Vector2(moveX, moveY).normalized; // Создаём нормализованный вектор, по которому происходит перемещение персонажа
            rb.linearVelocity = movement * moveSpeed;
            isMoving = true; // Персонаж двигается
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Останавливаем движение
            isMoving = false; // Персонаж стоит на месте
        }
    }

    public void SetMovement(bool state)
    {
        canMove = state;
        if (!state) rb.linearVelocity = Vector2.zero;
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Позиция курсора в мировых координатах юнити
        mousePosition.z = 0;

        Vector3 direction = (mousePosition - transform.position).normalized; // Вычисление направления от персонажа к курсору
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Вычисление угла поворота и его применение
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetMovement(true);
    }
}