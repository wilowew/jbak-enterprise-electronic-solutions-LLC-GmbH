using UnityEngine;

public class NPCDestroyTrigger : MonoBehaviour
{
    [Header("NPC Settings")]
    [Tooltip("Список NPC, которые должны быть уничтожены")]
    public GameObject[] npcList;

    [Header("Target Settings")]
    [Tooltip("Объект, который нужно переместить")]
    public GameObject targetObject;
    [Tooltip("Координаты для перемещения")]
    public Vector3 targetPosition;

    void Update()
    {
        if (npcList == null || npcList.Length == 0)
        {
            Debug.LogWarning("NPC list is not assigned or empty!");
            return;
        }

        bool allDestroyed = true;
        foreach (GameObject npc in npcList)
        {
            if (npc != null)
            {
                allDestroyed = false;
                break;
            }
        }

        if (allDestroyed)
        {
            if (targetObject != null)
            {
                targetObject.transform.position = targetPosition;
            }

            Destroy(gameObject);
        }
    }
}