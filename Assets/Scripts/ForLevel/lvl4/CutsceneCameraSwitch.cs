using UnityEngine;
using System.Collections;

public class DialogueCameraSwitcher : MonoBehaviour
{
    [Header("Настройки переключения")]
    [SerializeField] private string targetDialogueID = "SO";
    [SerializeField] private float switchDelay = 5f;
    [SerializeField] private Transform newCameraTarget;

    [Header("Параметры следования")]
    [SerializeField] private float newSmoothSpeed = 0.1f;
    [SerializeField] private Vector3 newOffset = new Vector3(0, 0, -10f);
    [SerializeField] private bool disableMouseLookAhead = true;

    [Header("Настройки диалога")]
    [SerializeField] private Transform dialogueTarget;

    [Header("Переход сцены")]
    [SerializeField] private SceneTransistor sceneTransistor;
    [SerializeField] private string targetSceneName;
    [SerializeField] private float cameraMoveHeight = 10f;
    [SerializeField] private float cameraMoveHorizontal = 5f;
    [SerializeField] private float transitionDelay = 1f;

    private CameraFollow cameraFollow;
    private Transform originalTarget;
    private Vector3 originalOffset;
    private float originalSmoothSpeed;
    private float originalEdgeThreshold;
    private float originalMaxLookAhead;
    private bool isSwitching;

    private void Start()
    {
        cameraFollow = GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            Debug.LogError("CameraFollow component not found on the camera!");
            enabled = false;
            return;
        }

        originalTarget = cameraFollow.target;
        originalOffset = cameraFollow.offset;
        originalSmoothSpeed = cameraFollow.smoothSpeed;
        originalEdgeThreshold = cameraFollow.edgeThreshold;
        originalMaxLookAhead = cameraFollow.maxLookAhead;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
        else
        {
            Debug.LogWarning("DialogueManager instance not found!");
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart += HandleDialogueStart;
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart -= HandleDialogueStart;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueStart(Dialogue startedDialogue)
    {
        if (startedDialogue.dialogueID == targetDialogueID && dialogueTarget != null)
        {
            cameraFollow.target = dialogueTarget;
            cameraFollow.offset = newOffset;
            cameraFollow.smoothSpeed = newSmoothSpeed;

            if (disableMouseLookAhead)
            {
                cameraFollow.edgeThreshold = 0;
                cameraFollow.maxLookAhead = 0;
            }
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue != null && endedDialogue.dialogueID == targetDialogueID)
        {
            ReturnToOriginalTarget();
            StartCoroutine(SwitchCameraRoutine());
        }
    }

    private IEnumerator SwitchCameraRoutine()
    {
        if (isSwitching || newCameraTarget == null) yield break;

        isSwitching = true;

        while (DialogueManager.Instance != null && DialogueManager.Instance.IsInPostDialogueDelay)
        {
            yield return null;
        }

        yield return new WaitForSeconds(switchDelay);

        cameraFollow.target = newCameraTarget;
        cameraFollow.offset = newOffset;
        cameraFollow.smoothSpeed = newSmoothSpeed;

        if (disableMouseLookAhead)
        {
            cameraFollow.edgeThreshold = 0;
            cameraFollow.maxLookAhead = 0;
        }

        yield return new WaitForSeconds(transitionDelay);

        if (sceneTransistor != null)
        {
            sceneTransistor.SetSceneParameters(
                targetSceneName,
                cameraMoveHeight,
                cameraMoveHorizontal
            );
            sceneTransistor.StartTransition();
        }
        else
        {
            Debug.LogWarning("SceneTransistor reference missing!");
        }

        isSwitching = false;
    }

    public void ReturnToOriginalTarget()
    {
        if (originalTarget != null)
        {
            cameraFollow.target = originalTarget;
            cameraFollow.offset = originalOffset;
            cameraFollow.smoothSpeed = originalSmoothSpeed;
            cameraFollow.edgeThreshold = originalEdgeThreshold;
            cameraFollow.maxLookAhead = originalMaxLookAhead;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (newCameraTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, newCameraTarget.position);
            Gizmos.DrawWireSphere(newCameraTarget.position, 0.5f);
        }
    }
}