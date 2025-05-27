using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class DialogueTriggerF : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool oneTimeUse = true;

    private bool isPlayerInTrigger = false;
    public bool alreadyUsed = false;
    private PolygonCollider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<PolygonCollider2D>();
        triggerCollider.isTrigger = true;
    }

    private void Update()
    {
        if (CanStartDialogue() && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }
    }

    private bool CanStartDialogue()
    {
        return isPlayerInTrigger
            && !alreadyUsed
            && !DialogueManager.Instance.IsDialogueActive
            && !PauseManager.Instance.IsPaused;
    }

    private void StartDialogue()
    {
        if (dialogue == null)
        {
            Debug.LogWarning("No dialogue assigned to trigger: " + gameObject.name);
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
        HintSystem.Instance.HideHint();
        if (oneTimeUse) alreadyUsed = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !alreadyUsed)
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    public void ResetTrigger()
    {
        alreadyUsed = false;
    }
}