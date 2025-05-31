using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CursorChanger cursorChanger;
    private bool isPaused = false;

    public static PauseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public bool IsPaused
    {
        get { return isPaused; }
    }

    private void TogglePause()
    {
        bool wasDialogueInterrupted = DialogueManager.Instance.IsDialogueActive && DialogueManager.Instance.IsDialoguePaused;

        if (wasDialogueInterrupted)
        {
            if (isPaused)
            {
                DialogueManager.Instance.ResumeDialogue();
            }
            else
            {
                DialogueManager.Instance.PauseDialogue();
            }
        }

        isPaused = !isPaused;

        UpdateTimeScale();
        pauseMenu.SetActive(isPaused);

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

    public void UpdateTimeScale()
    {
        Time.timeScale = (isPaused || DialogueManager.Instance.IsDialogueActive) ? 0 : 1;
    }

    private void OnEnable()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        if (cursorChanger != null)
        {
            cursorChanger.SetGameCursor();
        }
    }
}
