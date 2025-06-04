using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button continueButton;
    private const string menuSceneName = "Menu";

    private void Start()
    {
        PlayerPrefs.SetInt("GameCompleted", 1);
        PlayerPrefs.Save();
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(menuSceneName);
    }

}