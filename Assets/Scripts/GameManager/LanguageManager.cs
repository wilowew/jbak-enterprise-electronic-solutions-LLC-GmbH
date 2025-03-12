using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using System.Linq;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [SerializeField] private string defaultLanguage = "ru";
    private Dictionary<string, string> localizedTerms = new Dictionary<string, string>();

    public UnityEvent OnLanguageChanged = new UnityEvent();

    [Serializable]
    private class TermData
    {
        public string key;
        public string value;
    }

    [Serializable]
    private class TermList
    {
        public List<TermData> terms;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(defaultLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLanguage(string languageCode)
    {
        string path = Path.Combine("Languages", languageCode, "terms");
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            Debug.LogError($"Language file not found: {path}");
            return;
        }

        try
        {
            TermList termList = JsonUtility.FromJson<TermList>(jsonFile.text);
            localizedTerms = termList.terms.ToDictionary(t => t.key, t => t.value);
            Debug.Log($"Loaded {localizedTerms.Count} terms for {languageCode}");
            OnLanguageChanged.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON parse error: {e.Message}");
        }
    }

    public string GetTerm(string key)
    {
        if (localizedTerms.TryGetValue(key, out string value))
        {
            return value;
        }
        Debug.LogWarning($"Term not found: {key}");
        return $"#{key}#";
    }
}