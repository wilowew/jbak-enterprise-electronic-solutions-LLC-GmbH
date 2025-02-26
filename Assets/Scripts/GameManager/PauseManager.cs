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

        // ������������� ��� ������������ �����
        Time.timeScale = isPaused ? 0 : 1;

        // �������� ��� ��������� ���� �����
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }

        // ��������/��������� ������
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    private void OnEnable()
    {
        Time.timeScale = 1; // ��������, ��� ����� �������� ��� ������
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false); // ���� ����� ��������� � ������
        }
    }
}
