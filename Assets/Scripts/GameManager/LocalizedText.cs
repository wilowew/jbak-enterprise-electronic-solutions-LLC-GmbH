using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string termKey;
    private Text textComponent;

    private void Start()
    {
        textComponent = GetComponent<Text>();
        UpdateText();

        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged.AddListener(UpdateText);
    }

    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged.RemoveListener(UpdateText);
    }

    public void UpdateText()
    {
        if (textComponent == null) return;
        if (LanguageManager.Instance == null) return;

        textComponent.text = LanguageManager.Instance.GetTerm(termKey);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (LanguageManager.Instance == null) return;

        UpdateText();
    }
#endif
}