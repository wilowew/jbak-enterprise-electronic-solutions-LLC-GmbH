using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [Header("Локализация")]
    public string nameTermKey;
    [TextArea(3, 5)] public string textTermKey;

    [Header("Стилизация")]
    public Color nameColor = Color.yellow; 
    public TMP_FontAsset nameFont;
    public Sprite characterIcon;
}

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Диалоги/Создать диалог",
    order = 120)]

public class Dialogue : ScriptableObject
{
    [Header("Основные параметры")]
    [Tooltip("Уникальный ID для ссылок (опционально)")]
    public string dialogueID;

    [Header("Содержание диалога")]
    [SerializeField] private DialogueLine[] lines;

    public DialogueLine[] Lines => lines;
    public int Length => lines?.Length ?? 0;

    [ContextMenu("Проверить ключи локализации")]
    private void ValidateLocalizationKeys()
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.nameTermKey))
                Debug.LogWarning($"Пустой ключ имени в диалоге {name}");

            if (string.IsNullOrEmpty(line.textTermKey))
                Debug.LogWarning($"Пустой ключ текста в диалоге {name}");
        }
    }
}