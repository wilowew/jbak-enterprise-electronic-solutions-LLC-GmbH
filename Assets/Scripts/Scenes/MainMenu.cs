using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Text noSavesText;
    private const string menuSceneName = "Menu";

    [Header("Completion Button")]
    [SerializeField] private Button completionButton; 
    [SerializeField] private Text completionHintText;

    private void Start()
    {
        completionButton.onClick.AddListener(OnCompletionButtonClicked);
        completionHintText.gameObject.SetActive(false);
        UpdateUIElements(); 

        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            InitializeFirstLaunch();
        }
    }

    private void InitializeFirstLaunch()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("FirstLaunch", 1);
        PlayerPrefs.Save();
        UpdateUIElements();
    }

    private void UpdateUIElements()
    {
        bool hasSaves = PlayerPrefs.HasKey("LastScene");
        bool gameCompleted = PlayerPrefs.HasKey("GameCompleted") &&
                             PlayerPrefs.GetInt("GameCompleted") == 1;

        continueButton.gameObject.SetActive(hasSaves);
        noSavesText.gameObject.SetActive(!hasSaves);

        completionButton.interactable = gameCompleted;
    }

    public void StartGame()
    {
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.DeleteKey("GameCompleted");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1_Cutscene");
        UpdateUIElements();
    }

    public void OnCompletionButtonClicked()
    {
        if (PlayerPrefs.HasKey("GameCompleted") && PlayerPrefs.GetInt("GameCompleted") == 1)
        {
            SceneManager.LoadScene("LevelSelection");
        }
        else
        {
            StartCoroutine(ShowCompletionHint());
        }
    }

    private IEnumerator ShowCompletionHint()
    {
        completionHintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        completionHintText.gameObject.SetActive(false);
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("LastScene"))
        {
            string sceneToLoad = PlayerPrefs.GetString("LastScene");

            if (sceneToLoad != menuSceneName)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                HandleInvalidSave();
            }
        }
    }

    private void HandleInvalidSave()
    {
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.Save();
        UpdateUIElements(); 
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}