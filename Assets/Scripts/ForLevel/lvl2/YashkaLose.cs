using UnityEngine;
using System.Collections;

public class YashkaLose : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("������, ������������ �������� �����������")]
    public GameObject targetObject;

    [Header("Object Lists")]
    [Tooltip("������� ��� ����������� ��� ������������ ����")]
    public GameObject[] objectsToDeactivate;

    [Tooltip("������� ��� ��������� ��� ������������ ����")]
    public GameObject[] objectsToActivate;

    [Header("Dialogue Settings")]
    [Tooltip("������ ��� ������� ����� 3 �������")]
    public Dialogue selectedDialogue;

    private bool hasProcessed = false;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target Object �� �������� � ����������!");
            enabled = false;
        }
    }

    void Update()
    {
        if (!hasProcessed && CheckTargetDisappeared())
        {
            ProcessObjects();
            hasProcessed = true;
        }
    }

    private bool CheckTargetDisappeared()
    {
        return targetObject == null || !targetObject.activeInHierarchy;
    }

    private void ProcessObjects()
    {
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (selectedDialogue != null)
        {
            StartCoroutine(StartDialogueAfterDelay());
        }
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive &&
            !PauseManager.Instance.IsPaused)
        {
            DialogueManager.Instance.StartDialogue(selectedDialogue);
        }
    }
}
