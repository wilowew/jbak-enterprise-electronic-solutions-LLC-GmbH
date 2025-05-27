using UnityEngine;

public class MoneyDialogue : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Dialogue dialogueToStart;
    [SerializeField] private bool triggerOnlyOnce = true;

    [SerializeField] private bool checkForDestroyed = true;
    [SerializeField] private bool checkForDisabled = true;

    private bool _alreadyTriggered;
    private bool _wasActive; 

    private void Update()
    {
        if (_alreadyTriggered || dialogueToStart == null)
            return;

        bool isCurrentlyActive = targetObject != null && targetObject.activeInHierarchy;

        if (isCurrentlyActive)
        {
            _wasActive = true;
        }
        else
        {
            if (_wasActive)
            {
                bool isDestroyed = targetObject == null;
                bool shouldTrigger = (isDestroyed && checkForDestroyed) || (!isDestroyed && checkForDisabled);

                if (shouldTrigger)
                {
                    TriggerDialogue();
                    if (triggerOnlyOnce)
                        _alreadyTriggered = true;
                }

                _wasActive = false; 
            }
        }
    }

    private void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueToStart);
    }

    public void SetTargetObject(GameObject newTarget)
    {
        targetObject = newTarget;
        _alreadyTriggered = false;
        _wasActive = newTarget != null && newTarget.activeInHierarchy;
    }
}