using UnityEngine;
using System.Collections.Generic;

public class SpecificScarecrowsDialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue targetDialogue;
    [SerializeField] private Scarecrow[] targetScarecrows;
    [SerializeField] private bool oneTimeTrigger = true;

    private List<Scarecrow> aliveScarecrows = new List<Scarecrow>();
    private bool hasTriggered;

    private void Start()
    {
        InitializeScarecrowsList();
        SubscribeToDestructionEvents();
    }

    private void InitializeScarecrowsList()
    {
        aliveScarecrows.Clear();
        foreach (var scarecrow in targetScarecrows)
        {
            if (scarecrow != null && !scarecrow.IsDestroyed)
            {
                aliveScarecrows.Add(scarecrow);
            }
        }
    }

    private void SubscribeToDestructionEvents()
    {
        foreach (var scarecrow in targetScarecrows)
        {
            if (scarecrow != null)
            {
                scarecrow.OnDestroyed.AddListener(HandleScarecrowDestroyed);
            }
        }
    }

    private void UnsubscribeFromDestructionEvents()
    {
        foreach (var scarecrow in targetScarecrows)
        {
            if (scarecrow != null)
            {
                scarecrow.OnDestroyed.AddListener(HandleScarecrowDestroyed);
            }
        }
    }

    private void HandleScarecrowDestroyed()
    {
        if (hasTriggered && oneTimeTrigger) return;

        aliveScarecrows.RemoveAll(s => s.IsDestroyed);

        if (aliveScarecrows.Count == 0)
        {
            TriggerDialogue();
        }
    }

    private void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(targetDialogue);
            hasTriggered = true;
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        InitializeScarecrowsList();
    }

    private void OnDestroy()
    {
        UnsubscribeFromDestructionEvents();
    }

}