using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CursorChanger cursorChanger;
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

        Time.timeScale = isPaused ? 0 : 1;
        pauseMenu.SetActive(isPaused);

        // ћен€ем курсор в зависимости от паузы
        if (cursorChanger != null)
        {
            if (isPaused)
            {
                cursorChanger.SetPauseCursor();
            }
            else
            {
                cursorChanger.SetGameCursor();
            }
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        // ѕри перезапуске возвращаем игровой курсор
        if (cursorChanger != null)
        {
            cursorChanger.SetGameCursor();
        }
    }
}
