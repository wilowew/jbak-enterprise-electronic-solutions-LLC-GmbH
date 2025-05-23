using UnityEngine;
using System.Collections;

public class TimedImage : MonoBehaviour
{
    public GameObject targetImage;
    public PlayerMovement playerMovement;

    void Start()
    {
        StartCoroutine(ShowAndHideImage());
    }

    IEnumerator ShowAndHideImage()
    {
        targetImage.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovement(false);

        yield return new WaitForSeconds(1.3f);

        targetImage.SetActive(true);

        yield return new WaitForSeconds(3.7f);

        targetImage.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovement(true);
    }
}