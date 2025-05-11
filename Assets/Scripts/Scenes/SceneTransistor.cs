using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private float cameraMoveHeight = 5f;
    [SerializeField] private float cameraMoveHorizontal = 0f;

    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private Transform player;
    [SerializeField] private CutsceneCamera cutsceneCamera;

    [SerializeField] private string requiredDialogueID;

    private bool isTransitioning = false;
    private Camera mainCamera;
    private bool isRequiredDialogueCompleted = false;

    private void Start()
    {
        mainCamera = Camera.main;
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue != null && endedDialogue.dialogueID == requiredDialogueID)
        {
            isRequiredDialogueCompleted = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning && isRequiredDialogueCompleted)
        {
            StartCoroutine(TransitionSequence());
        }
    }

    private void UpdateMainCamera()
    {
        mainCamera = Camera.main;
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        if (cutsceneCamera != null)
        {
            cutsceneCamera.Deactivate();
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            yield break;
        }

        yield return null;

        Vector3 startCameraPos = mainCamera.transform.position;
        Vector3 targetCameraPos = startCameraPos + new Vector3(cameraMoveHorizontal, cameraMoveHeight, 0f);

        fadeAnimator.SetTrigger("FadeOut");
        float timer = 0;

        while (timer < transitionDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(
                startCameraPos,
                targetCameraPos,
                timer / transitionDuration
            );

            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}