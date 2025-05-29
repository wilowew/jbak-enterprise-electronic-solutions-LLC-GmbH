using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayedDialogueTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Список ID диалогов, которые должны быть проиграны перед срабатыванием")]
    public List<string> requiredDialogueIDs = new List<string>();

    [Tooltip("Диалог для запуска через 3 секунды после проигрыша всех требуемых диалогов")]
    public Dialogue targetDialogue;

    [Tooltip("Срабатывает только один раз")]
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

            if (debugLogs) Debug.Log($"[DialogueTrigger] Подписан на события. ID: {GetInstanceID()}");
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

        if (debugLogs) Debug.Log($"[DialogueTrigger] Отписан от событий. ID: {GetInstanceID()}");
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == null)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] Получен пустой диалог");
            return;
        }

        if (debugLogs) Debug.Log($"[DialogueTrigger] Окончание диалога: {endedDialogue.dialogueID}");

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
            if (debugLogs) Debug.Log("[DialogueTrigger] Уже срабатывал ранее");
            return;
        }

        if (isWaiting)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] Уже в процессе ожидания");
            return;
        }

        if (targetDialogue == null)
        {
            if (debugLogs) Debug.Log("[DialogueTrigger] Целевой диалог не назначен");
            return;
        }

        foreach (string id in requiredDialogueIDs)
        {
            if (!playedDialogueIDs.Contains(id))
            {
                if (debugLogs) Debug.Log($"[DialogueTrigger] Не все диалоги проиграны. Отсутствует: {id}");
                return;
            }
        }

        if (debugLogs) Debug.Log($"[DialogueTrigger] Все диалоги проиграны! Запуск отложенного диалога...");
        StartCoroutine(TriggerDelayedDialogue());
    }

    private IEnumerator TriggerDelayedDialogue()
    {
        isWaiting = true;

        if (debugLogs) Debug.Log($"[DialogueTrigger] Ожидание 3 секунды...");

        yield return new WaitForSeconds(3f);

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[DialogueTrigger] DialogueManager недоступен!");
            isWaiting = false;
            yield break;
        }

        if (debugLogs) Debug.Log($"[DialogueTrigger] Запуск диалога: {targetDialogue.name}");

        DialogueManager.Instance.StartDialogue(targetDialogue);
        hasTriggered = true;
        isWaiting = false;
    }

    [ContextMenu("Тест: Запустить отложенный диалог")]
    public void ManualTriggerTest()
    {
        if (Application.isPlaying && targetDialogue != null)
        {
            StartCoroutine(TriggerDelayedDialogue());
        }
        else
        {
            Debug.LogWarning("Тест доступен только в режиме игры при назначенном диалоге!");
        }
    }

    public void MarkDialogueAsPlayed(string dialogueID)
    {
        if (!playedDialogueIDs.Contains(dialogueID))
        {
            playedDialogueIDs.Add(dialogueID);
            if (debugLogs) Debug.Log($"[DialogueTrigger] Диалог помечен как проигранный: {dialogueID}");
            CheckAllDialoguesPlayed();
        }
    }
}