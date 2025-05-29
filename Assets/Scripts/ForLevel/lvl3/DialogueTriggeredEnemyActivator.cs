using UnityEngine;
using System.Collections;

public class DialogueTriggeredEnemyActivator : MonoBehaviour
{
    [Header("��������� ���������")]
    [Tooltip("ID �������, ����� �������� ������������ ����")]
    [SerializeField] private string targetDialogueID;

    [Tooltip("�������� ����� ���������� (�������)")]
    [SerializeField] private float activationDelay = 5f;

    [Header("������")]
    [Tooltip("������ FriendlyEnemy ��� ���������")]
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
            Debug.LogError("DialogueManager �� ������ � �����!");
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
            Debug.Log($"[���������] FriendlyEnemy ����������� ����� ������� '{targetDialogueID}'");
        }
        else
        {
            Debug.LogWarning($"[���������] ������ �� FriendlyEnemy �� �����������!");
        }
    }

    private void OnValidate()
    {
        if (friendlyEnemyScript != null && friendlyEnemyScript.GetType().Name != "FriendlyNPC")
        {
            Debug.LogError($"[���������] �������� ��� �������! ��������� FriendlyEnemy, ������� {friendlyEnemyScript.GetType().Name}");
            friendlyEnemyScript = null;
        }
    }
}