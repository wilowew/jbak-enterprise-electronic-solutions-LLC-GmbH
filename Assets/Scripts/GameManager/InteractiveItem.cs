using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private string hintMessage;
    [SerializeField] private Sprite hintIcon;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private bool requireDialogueCompletion;
    [SerializeField] private Dialogue requiredDialogue;

    private CircleCollider2D interactionCollider;
    private bool playerInRange;
    private bool dialogueCompleted;

    private void Start()
    {
        interactionCollider = gameObject.AddComponent<CircleCollider2D>();
        interactionCollider.radius = interactionRadius;
        interactionCollider.isTrigger = true;

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
        if (requireDialogueCompletion && endedDialogue == requiredDialogue)
        {
            dialogueCompleted = true;
            UpdateHintVisibility();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateHintVisibility();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UpdateHintVisibility();
        }
    }

    public void UpdateHintVisibility()
    {
        bool shouldShow = playerInRange && (!requireDialogueCompletion || dialogueCompleted);

        if (shouldShow && !DialogueManager.Instance.IsDialogueActive)
        {
            HintSystem.Instance.ShowHint(hintMessage, hintIcon);
        }
        else
        {
            HintSystem.Instance.HideHint();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}