using UnityEngine;

public class ScarecrowDialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue destructionDialogue;
    [SerializeField] private bool oneTimeUse = true;

    private Scarecrow scarecrow;
    private bool hasTriggered;

    private void Awake()
    {
        scarecrow = GetComponent<Scarecrow>();
    }

    private void OnEnable()
    {
        scarecrow.OnDestroyed.AddListener(HandleDestruction);
    }

    private void OnDisable()
    {
        scarecrow.OnDestroyed.RemoveListener(HandleDestruction);
    }

    private void HandleDestruction()
    {
        if (hasTriggered && oneTimeUse) return;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(destructionDialogue);
            hasTriggered = true;
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
