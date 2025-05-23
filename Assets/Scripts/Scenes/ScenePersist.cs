using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersist : MonoBehaviour
{
    private static ScenePersist instance;
    private const string menuSceneName = "Menu";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentScene();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentScene();
        }
    }

    private void SaveCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (!string.Equals(currentScene, menuSceneName, System.StringComparison.Ordinal))
        {
            PlayerPrefs.SetString("LastScene", currentScene);
            PlayerPrefs.Save();
        }
    }
}