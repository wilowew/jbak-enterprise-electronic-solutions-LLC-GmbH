using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DialogueTriggerSklad : MonoBehaviour
{
    [Header("Enemy Tracking")]
    [SerializeField] private List<GameObject> trackedEnemies;

    [Header("Dialogue Trigger")]
    [SerializeField] private Collider2D dialogueTriggerCollider; 
    [SerializeField] private float checkInterval = 0.3f;

    private bool isTriggerActivated = false;

    private void Start()
    {
        dialogueTriggerCollider.enabled = false;
        StartCoroutine(CheckEnemiesPresence());
    }

    private IEnumerator CheckEnemiesPresence()
    {
        while (!isTriggerActivated)
        {
            yield return new WaitForSeconds(checkInterval);

            foreach (var enemy in trackedEnemies)
            {
                if (enemy == null || !enemy.activeInHierarchy)
                {
                    ActivateTrigger();
                    yield break;
                }
            }
        }
    }

    private void ActivateTrigger()
    {
        isTriggerActivated = true;
        dialogueTriggerCollider.enabled = true;
    }

}
