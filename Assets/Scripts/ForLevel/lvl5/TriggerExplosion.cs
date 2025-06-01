using UnityEngine;
using System.Collections;

public class TriggerExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public float explosionDuration = 1f;

    [Header("Death Object")]
    public GameObject deathPrefab;

    [Header("Scene Transition")]
    public string nextSceneName;
    public SceneTransistor5 sceneTransistor; 
    public float cameraMoveHeight = 3f;    
    public float cameraMoveHorizontal = 2f; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ExplodeAndTransition(other.gameObject));
        }
    }

    private IEnumerator ExplodeAndTransition(GameObject player)
    {
        Vector2 playerPosition = player.transform.position;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, playerPosition, Quaternion.identity);
        }

        player.SetActive(false);

        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, playerPosition, Quaternion.identity);
        }

        if (sceneTransistor != null)
        {
            sceneTransistor.SetSceneParameters(
                nextSceneName,
                cameraMoveHeight,
                cameraMoveHorizontal
            );


            yield return new WaitForSeconds(0.5f);
            sceneTransistor.StartTransition();
        }
        else
        {
            Debug.LogError("SceneTransistor not assigned! Loading directly...");
            yield return new WaitForSeconds(explosionDuration);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}