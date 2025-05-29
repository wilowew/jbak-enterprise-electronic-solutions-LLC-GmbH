using UnityEngine;
using System.Collections.Generic;

public class DialogueTriggerOnAmmoAndObjects : MonoBehaviour
{
    [Header("Диалог")]
    [SerializeField] private Dialogue dialogueToActivate;

    [Header("Отслеживаемое оружие")]
    [SerializeField] private FirearmShooting firearm; 

    [Header("Объекты для сохранения")]
    [SerializeField] private List<GameObject> objectsToSave; 

    private bool wasAmmoEverLoaded = false; 
    private bool alreadyTriggered = false; 

    private void Start()
    {
        if (firearm == null)
        {
            Debug.LogError("Firearm reference not set in DialogueTriggerOnAmmoAndObjects!");
            return;
        }

        // Подписка на событие пополнения боеприпасов
        firearm.OnAmmoLoaded += HandleAmmoLoaded;
    }

    private void OnDestroy()
    {
        if (firearm != null)
        {
            firearm.OnAmmoLoaded -= HandleAmmoLoaded;
        }
    }

    private void HandleAmmoLoaded()
    {
        wasAmmoEverLoaded = true;
    }

    private void Update()
    {
        if (alreadyTriggered) return;
        if (firearm == null) return;
        if (!wasAmmoEverLoaded) return; 

        if (CheckAmmoCondition() && CheckObjectsCondition())
        {
            TriggerDialogue();
        }
    }

    private bool CheckAmmoCondition()
    {
        return wasAmmoEverLoaded &&
               firearm.currentBulletsInMag == 0 &&
               firearm.reserveMagazineCount == 0;
    }

    private bool CheckObjectsCondition()
    {
        foreach (GameObject obj in objectsToSave)
        {
            if (obj != null && obj.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private void TriggerDialogue()
    {
        if (dialogueToActivate == null)
        {
            Debug.LogError("Dialogue SO not assigned!");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueToActivate);
        alreadyTriggered = true;
    }
}