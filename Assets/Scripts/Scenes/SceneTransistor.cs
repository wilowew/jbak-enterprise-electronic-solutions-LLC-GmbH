﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class DoorTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private float cameraMoveHeight = 5f;
    [SerializeField] private float cameraMoveHorizontal = 0f;

    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private Transform player;
    [SerializeField] private CutsceneCamera cutsceneCamera;

    [SerializeField] private string requiredDialogueID;
    [SerializeField] private List<GameObject> npcs;

    private bool isTransitioning = false;
    private Camera mainCamera;
    private bool isRequiredDialogueCompleted = false;

    private void Start()
    {
        mainCamera = Camera.main;
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue != null && endedDialogue.dialogueID == requiredDialogueID)
        {
            Debug.Log("Диалог завершен, условие выполнено.");
            isRequiredDialogueCompleted = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning && (isRequiredDialogueCompleted || AreAllNPCsDead()))
        {
            Debug.Log("Условие выполнено: либо диалог завершен, либо все NPC мертвы.");
            StartCoroutine(TransitionSequence());
        }
        else
        {
            Debug.Log($"Условия не выполнены: isTransitioning={isTransitioning}, isRequiredDialogueCompleted={isRequiredDialogueCompleted}, AreAllNPCsDead={AreAllNPCsDead()}");
        }
    }

    private bool AreAllNPCsDead()
    {
        foreach (GameObject npc in npcs)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                Debug.Log($"NPC {npc.name} еще жив!");
                return false;
            }
        }
        Debug.Log("Все NPC мертвы.");
        return true;
    }

    private void UpdateMainCamera()
    {
        mainCamera = Camera.main;
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        if (cutsceneCamera != null)
        {
            cutsceneCamera.Deactivate();
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            yield break;
        }

        yield return null;

        Vector3 startCameraPos = mainCamera.transform.position;
        Vector3 targetCameraPos = startCameraPos + new Vector3(cameraMoveHorizontal, cameraMoveHeight, 0f);

        fadeAnimator.SetTrigger("FadeOut");
        float timer = 0;

        while (timer < transitionDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(
                startCameraPos,
                targetCameraPos,
                timer / transitionDuration
            );

            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
