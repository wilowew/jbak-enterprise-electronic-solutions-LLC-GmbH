using UnityEngine;
using System.Collections;

public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private float cameraDropHeight = 10f;
    [SerializeField] private float cameraDropHorizontal = 0f;
    [SerializeField] private float cameraDropDuration = 2f; 
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator fadeAnimator;

    private Camera mainCamera;
    private Vector3 targetCameraPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        InitializeCameraPosition();
        StartCoroutine(EntranceSequence());
    }

    private void InitializeCameraPosition()
    {
        if (player != null && mainCamera != null)
        {
            targetCameraPosition = player.position + new Vector3(0, 0, -10);
            Vector3 startPosition = targetCameraPosition
                + new Vector3(cameraDropHorizontal, cameraDropHeight, 0);
            mainCamera.transform.position = startPosition;
        }
    }

    private IEnumerator EntranceSequence()
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovement(false);
            Debug.Log("Движение заблокировано");
        }

        fadeAnimator.SetTrigger("FadeOut");

        float timer = 0;
        Vector3 startPosition = mainCamera.transform.position;

        while (timer < cameraDropDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(
                startPosition,
                targetCameraPosition,
                timer / cameraDropDuration
            );

            timer += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetCameraPosition;

        if (playerMovement != null)
        {
            playerMovement.SetMovement(true);
            Debug.Log("Движение разблокировано");
        }

        yield return new WaitForSeconds(0.1f);
        fadeAnimator.ResetTrigger("FadeOut");
        enabled = false;
    }
}