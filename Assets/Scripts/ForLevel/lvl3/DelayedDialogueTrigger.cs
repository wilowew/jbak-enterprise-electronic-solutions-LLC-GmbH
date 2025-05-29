using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayedDialogueTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("������ ID ��������, ������� ������ ���� ��������� ����� �������������")]
    public List<string> requiredDialogueIDs = new List<string>();

    [Tooltip("������ ��� ������� ����� 3 ������� ����� ��������� ���� ��������� ��������")]
    public Dialogue targetDialogue;

    [Tooltip("����������� ������ ���� ���")]
    public bool triggerOnlyOnce = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool hasTriggered = false;
    private bool isWaiting = false;
    private bool isSubscribed = false;
    private HashSet<string> playedDialogueIDs = new HashSet<string>();

    private void Start()
    {
        SubscribeToEvents();
        CheckAllDialoguesPlayed(); 
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (isSubscribed) return;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
            isSubscribed = true;

            if (debugLogs) Debug.Log($"[DialogueTrigger] �������� �� �������. ID: {GetInstanceID()}");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (!isSubscribed) return;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
        isSubscribed = false;

        if (debugLogs) Debug.Log($"[DialogueTrigger] ������� �� �������. ID: {GetInstanceID()}");
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == null)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] ������� ������ ������");
            return;
        }

        if (debugLogs) Debug.Log($"[DialogueTrigger] ��������� �������: {endedDialogue.dialogueID}");

        if (!string.IsNullOrEmpty(endedDialogue.dialogueID))
        {
            playedDialogueIDs.Add(endedDialogue.dialogueID);
        }

        CheckAllDialoguesPlayed();
    }

    private void CheckAllDialoguesPlayed()
    {
        if (hasTriggered && triggerOnlyOnce)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] ��� ���������� �����");
            return;
        }

        if (isWaiting)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] ��� � �������� ��������");
            return;
        }

        if (targetDialogue == null)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] ������� ������ �� ��������");
            return;
        }

        foreach (string id in requiredDialogueIDs)
        {
            if (!playedDialogueIDs.Contains(id))
            {
                if (debugLogs) Debug.Log($"[DialogueTrigger] �� ��� ������� ���������. �����������: {id}");
                return;
            }
        }

        if (debugLogs) Debug.Log($"[DialogueTrigger] ��� ������� ���������! ������ ����������� �������...");
        StartCoroutine(TriggerDelayedDialogue());
    }

    private IEnumerator TriggerDelayedDialogue()
    {
        isWaiting = true;

        if (debugLogs) Debug.Log($"[DialogueTrigger] �������� 3 �������...");

        yield return new WaitForSeconds(3f);

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[DialogueTrigger] DialogueManager ����������!");
            isWaiting = false;
            yield break;
        }

        if (debugLogs) Debug.Log($"[DialogueTrigger] ������ �������: {targetDialogue.name}");

        DialogueManager.Instance.StartDialogue(targetDialogue);
        hasTriggered = true;
        isWaiting = false;
    }

    [ContextMenu("����: ��������� ���������� ������")]
    public void ManualTriggerTest()
    {
        if (Application.isPlaying && targetDialogue != null)
        {
            StartCoroutine(TriggerDelayedDialogue());
        }
        else
        {
            Debug.LogWarning("���� �������� ������ � ������ ���� ��� ����������� �������!");
        }
    }

    public void MarkDialogueAsPlayed(string dialogueID)
    {
        if (!playedDialogueIDs.Contains(dialogueID))
        {
            playedDialogueIDs.Add(dialogueID);
            if (debugLogs) Debug.Log($"[DialogueTrigger] ������ ������� ��� �����������: {dialogueID}");
            CheckAllDialoguesPlayed();
        }
    }
}