using UnityEngine;
using System.Collections;

public class DialogueTriggeredEnemyActivator : MonoBehaviour
{
    [Header("Настройки активации")]
    [Tooltip("ID диалога, после которого активируется враг")]
    [SerializeField] private string targetDialogueID;

    [Tooltip("Задержка перед активацией (секунды)")]
    [SerializeField] private float activationDelay = 5f;

    [Header("Ссылки")]
    [Tooltip("Скрипт FriendlyEnemy для активации")]
    [SerializeField] private MonoBehaviour friendlyEnemyScript;

    private DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd += HandleDialogueEnd;
        }
        else
        {
            Debug.LogError("DialogueManager не найден в сцене!");
        }
    }

    private void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue != null && endedDialogue.dialogueID == targetDialogueID)
        {
            StartCoroutine(ActivateEnemyAfterDelay());
        }
    }

    private IEnumerator ActivateEnemyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(activationDelay);

        if (friendlyEnemyScript != null)
        {
            friendlyEnemyScript.enabled = true;
            Debug.Log($"[Активатор] FriendlyEnemy активирован после диалога '{targetDialogueID}'");
        }
        else
        {
            Debug.LogWarning($"[Активатор] Ссылка на FriendlyEnemy не установлена!");
        }
    }

    private void OnValidate()
    {
        if (friendlyEnemyScript != null && friendlyEnemyScript.GetType().Name != "FriendlyNPC")
        {
            Debug.LogError($"[Активатор] Неверный тип скрипта! Ожидается FriendlyEnemy, получен {friendlyEnemyScript.GetType().Name}");
            friendlyEnemyScript = null;
        }
    }
}