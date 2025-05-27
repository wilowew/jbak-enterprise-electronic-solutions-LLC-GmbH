using UnityEngine;
using System.Collections;

public class TimedSceneCut : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float delayTime = 5f;
    [SerializeField] private string targetSceneName;
    [SerializeField] private float cameraMoveHeight;
    [SerializeField] private float cameraMoveHorizontal;

    [Header("Dependencies")]
    [SerializeField] private SceneTransistor2_cutscene sceneTransistor;

    private void Start()
    {
        if (sceneTransistor == null)
        {
            sceneTransistor = GetComponent<SceneTransistor2_cutscene>();
        }

        StartCoroutine(StartTimedTransition());
    }

    private IEnumerator StartTimedTransition()
    {
        yield return new WaitForSeconds(delayTime);

        if (sceneTransistor != null)
        {
            sceneTransistor.SetSceneParameters(
                targetSceneName,
                cameraMoveHeight,
                cameraMoveHorizontal
            );

            sceneTransistor.StartTransition();
        }
        else
        {
            Debug.LogError("SceneTransistor component not found!");
        }
    }
}