using UnityEngine;

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
    }
}
