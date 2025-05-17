using UnityEngine;

public class GuysLose : MonoBehaviour
{
    [Header("Настройки триггера")]
    [Tooltip("Объекты, которые должны исчезнуть для активации диалога")]
    [SerializeField] private GameObject[] targetObjects;

    [Header("Диалог")]
    [Tooltip("Диалог, который будет запущен")]
    [SerializeField] private Dialogue triggerDialogue;

    [Header("Задержка")]
    [Tooltip("Задержка перед запуском диалога (секунды)")]
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
            Debug.LogError("Не настроен диалог или отсутствует DialogueManager!");
        }
    }

    public void AddTargetObject(GameObject newObject)
    {
        System.Collections.Generic.List<GameObject> tempList = new System.Collections.Generic.List<GameObject>(targetObjects);
        tempList.Add(newObject);
        targetObjects = tempList.ToArray();
    }
}
