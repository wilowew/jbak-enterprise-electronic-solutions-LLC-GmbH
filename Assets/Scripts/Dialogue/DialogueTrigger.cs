using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool startAutomatically = false;
    [SerializeField][Min(0)] private float delaySeconds = 3f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (startAutomatically)
        {
            StartCoroutine(StartDialogueAfterDelay());
        }
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
            GetComponent<Collider2D>().enabled = false;
        }
    }
}