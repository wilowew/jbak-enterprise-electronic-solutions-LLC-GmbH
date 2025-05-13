using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HintSystem : MonoBehaviour
{
    public static HintSystem Instance { get; private set; }

    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Image hintIcon;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        HideHint();
    }

    public void ShowHintConditional(string text, Sprite icon)
    {
        if (!DialogueManager.Instance.IsDialogueActive &&
            !DialogueManager.Instance.IsInPostDialogueDelay)
        {
            ShowHint(text, icon);
        }
    }

    public void ShowHint(string text, Sprite icon)
    {
        hintText.text = text;
        hintIcon.sprite = icon;
        hintPanel.SetActive(true);
    }

    public void HideHint()
    {
        hintPanel.SetActive(false);
        hintText.text = string.Empty;
        hintIcon.sprite = null;
    }
}