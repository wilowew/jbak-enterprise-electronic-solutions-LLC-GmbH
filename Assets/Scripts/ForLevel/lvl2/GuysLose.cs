using UnityEngine;

public class GuysLose : MonoBehaviour
{
    [Header("��������� ��������")]
    [Tooltip("�������, ������� ������ ��������� ��� ��������� �������")]
    [SerializeField] private GameObject[] targetObjects;

    [Header("������")]
    [Tooltip("������, ������� ����� �������")]
    [SerializeField] private Dialogue triggerDialogue;

    [Header("��������")]
    [Tooltip("�������� ����� �������� ������� (�������)")]
    [SerializeField] private float delayBeforeDialogue = 0.5f;

    private bool dialogueTriggered = false;

    private void Update()
    {
        if (dialogueTriggered || !AllObjectsDisappeared()) return;

        dialogueTriggered = true;
        Invoke(nameof(StartDialogue), delayBeforeDialogue);
    }

    private bool AllObjectsDisappeared()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
    }

    private void StartDialogue()
    {
        if (triggerDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(triggerDialogue);
        }
        else
        {
            Debug.LogError("�� �������� ������ ��� ����������� DialogueManager!");
        }
    }

    public void AddTargetObject(GameObject newObject)
    {
        System.Collections.Generic.List<GameObject> tempList = new System.Collections.Generic.List<GameObject>(targetObjects);
        tempList.Add(newObject);
        targetObjects = tempList.ToArray();
    }
}
