using UnityEngine;
using System.Collections;

public class DialogueEventTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool usePersistentEvents = true;
    [SerializeField] private float initializationRetryDelay = 0.5f;

    [System.Serializable]
    public class DialogueEvent
    {
        public Dialogue targetDialogue;
        public GameObject[] objectsToActivate;
    }

    [SerializeField] private DialogueEvent[] dialogueEvents;

    private void Start()
    {
        StartCoroutine(InitializeWithRetry());
    }

    private IEnumerator InitializeWithRetry()
    {
        while (DialogueManager.Instance == null)
        {
            Debug.LogWarning("Waiting for DialogueManager initialization...");
            yield return new WaitForSecondsRealtime(initializationRetryDelay);
        }

        if (usePersistentEvents)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
            Debug.Log("Successfully subscribed to DialogueManager events");
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        foreach (var eventItem in dialogueEvents)
        {
            if (eventItem.targetDialogue == endedDialogue)
            {
                foreach (var obj in eventItem.objectsToActivate)
                {
                    if (obj != null)
                    {
                        StartCoroutine(ActivateObjectWithDelay(obj));
                    }
                }
            }
        }
    }

    private IEnumerator ActivateObjectWithDelay(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        obj.SetActive(true);
        Debug.Log($"Activated object: {obj.name}", obj);
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }
}