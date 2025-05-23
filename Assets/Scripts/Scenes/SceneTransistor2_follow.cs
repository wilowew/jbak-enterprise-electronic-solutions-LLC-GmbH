using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransistor : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("Transition Effects")]
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private CameraFollow cameraFollow; 

    private string targetSceneName;
    private float cameraMoveHeight;
    private float cameraMoveHorizontal;
    private Camera mainCamera;
    private bool isTransitioning = false;

    public void SetSceneParameters(string sceneName, float height, float horizontal)
    {
        targetSceneName = sceneName;
        cameraMoveHeight = height;
        cameraMoveHorizontal = horizontal;
    }

    public void StartTransition()
    {
        if (!isTransitioning && !string.IsNullOrEmpty(targetSceneName))
        {
            StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;
        UpdateMainCamera();

        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }

        Vector3 startCameraPos = mainCamera.transform.position;
        Vector3 targetCameraPos = startCameraPos +
            new Vector3(cameraMoveHorizontal, cameraMoveHeight, 0f);

        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("FadeOut");
        }

        float timer = 0;

        while (timer < transitionDuration)
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(
                    startCameraPos,
                    targetCameraPos,
                    timer / transitionDuration
                );
            }

            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void UpdateMainCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }
}