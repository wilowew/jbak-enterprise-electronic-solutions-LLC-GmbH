using UnityEngine;

public class ActivateAfterDialogue : MonoBehaviour
{
    [SerializeField] private Dialogue targetDialogue; 
    [SerializeField] private GameObject objectToActivate; 
    [SerializeField] private bool oneTimeUse = true; 

    private bool activated;

    private void Start()
    {
        DialogueManager.Instance.OnDialogueEnd += OnDialogueEnded;
    }

    private void OnDialogueEnded(Dialogue endedDialogue)
    {
        if (endedDialogue == targetDialogue && !activated)
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }

            if (oneTimeUse)
            {
                activated = true;
                DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnded;
            }
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnded;
        }
    }
}