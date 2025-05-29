using UnityEngine;

public class DisableObjectAfterDialogue : MonoBehaviour
{
    [SerializeField] private string targetDialogueID;
    [SerializeField] private bool destroyInstead = false;

    private void Start()
    {
        CheckDialoguePlayed();
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void CheckDialoguePlayed()
    {
        if (DialogueTracker.Instance != null &&
            DialogueTracker.Instance.HasPlayed(targetDialogueID))
        {
            DisableTargetObject();
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue != null && endedDialogue.dialogueID == targetDialogueID)
        {
            DisableTargetObject();
        }
    }

    private void DisableTargetObject()
    {
        if (destroyInstead)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }

        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
    }
}