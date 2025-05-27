using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HintTrigger : MonoBehaviour
{
    [SerializeField] public string hintMessage;
    [SerializeField] public Sprite hintIcon; 
    [SerializeField] private DialogueTriggerF linkedDialogueTrigger;

    private void Awake()
    {
        linkedDialogueTrigger = GetComponent<DialogueTriggerF>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isDialogueUsed = linkedDialogueTrigger != null && linkedDialogueTrigger.alreadyUsed;

        if (!isDialogueUsed)
        {
            HintSystem.Instance.ShowHintConditional(hintMessage, hintIcon);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HintSystem.Instance.HideHint();
        }
    }
}