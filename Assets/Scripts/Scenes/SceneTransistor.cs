using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private float cameraMoveHeight = 5f;

    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private Transform player;

    [SerializeField] private string requiredDialogueID;

    private bool isTransitioning = false;
    private Vector3 cameraStartPosition;
    private Camera mainCamera;
    private bool isRequiredDialogueCompleted = false;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraStartPosition = mainCamera.transform.position;
        }
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

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        Vector3 startCameraPos = mainCamera.transform.position;
        Vector3 targetCameraPos = startCameraPos + Vector3.up * cameraMoveHeight;

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

        if (mainCamera != null && player != null)
        {
            mainCamera.transform.position = player.position + new Vector3(0, 0, -10);
        }

        fadeAnimator.SetTrigger("FadeOut");

        isTransitioning = false;
    }
}