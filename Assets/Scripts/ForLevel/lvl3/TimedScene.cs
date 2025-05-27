using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TimedSceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float delayTime = 5f;
    [SerializeField] private List<string> requiredDialogueIDs;
    [SerializeField] private string targetSceneName;
    [SerializeField] private float cameraMoveHeight;
    [SerializeField] private float cameraMoveHorizontal;

    [Header("Dependencies")]
    [SerializeField] private SceneTransistor sceneTransistor;

    private bool _shouldCheckDialogues = true;

    private IEnumerator Start()
    {
        if (sceneTransistor == null)
            sceneTransistor = GetComponent<SceneTransistor>();

        float timer = 0;

        while (timer < delayTime)
        {
            if (_shouldCheckDialogues && CheckAllDialoguesPlayed())
            {
                ExecuteTransition();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        ExecuteTransition();
    }

    private bool CheckAllDialoguesPlayed()
    {
        return DialogueTracker.Instance.HaveAllDialoguePlayed(requiredDialogueIDs);
    }

    private void ExecuteTransition()
    {
        _shouldCheckDialogues = false;

        if (sceneTransistor != null)
        {
            sceneTransistor.SetSceneParameters(
                targetSceneName,
                cameraMoveHeight,
                cameraMoveHorizontal
            );
            sceneTransistor.StartTransition();
        }
    }
}