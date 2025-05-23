using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Text noSavesText;
    private const string menuSceneName = "Menu";

    private void Start()
    {
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

        continueButton.gameObject.SetActive(hasSaves);
        noSavesText.gameObject.SetActive(!hasSaves);
    }

    public void StartGame()
    {
        PlayerPrefs.DeleteKey("LastScene");
        SceneManager.LoadScene("Level1_Cutscene");
        UpdateUIElements();
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
}