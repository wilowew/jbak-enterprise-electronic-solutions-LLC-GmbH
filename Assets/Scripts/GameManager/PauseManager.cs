using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool isPaused = false;

    public void OnPause()
    {
        TogglePause();
    }

    public bool IsPaused
    {
        get { return isPaused; }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        // Останавливаем или возобновляем время
        Time.timeScale = isPaused ? 0 : 1;

        // Включаем или отключаем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }

        // Включаем/отключаем курсор
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    private void OnEnable()
    {
        Time.timeScale = 1; // Убедимся, что время работает при старте
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false); // Меню паузы выключено в начале
        }
    }
}
