using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
    public bool isChoicePoint;
    public List<Choice> choices;

    [Header("Завершение диалога")]
    public bool isExitLine;
}

[System.Serializable]
public class Choice
{
    public string choiceTermKey;
    public int targetLineIndex;
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

    [Header("Задержка после диалога")]
    [Tooltip("Включите, чтобы добавить задержку после завершения этого диалога")]
    public bool usePostDialogueDelay = false;
    [Tooltip("Блокировать движение персонажа во время задержки")]
    public bool blockMovementDuringDelay = true;
    [Tooltip("Время задержки в секундах перед началом следующего диалога")]
    public float postDialogueDelayTime = 1f;

    [Header("Следующий диалог")]
    [Tooltip("Диалог, который начнется после этого")]
    public Dialogue nextDialogue;

    [Header("Завершение диалога")]
    [Tooltip("Требует явного закрытия игроком")]
    public bool requireManualClose;

    [Header("Переход на сцену")]
    [Tooltip("Имя сцены для загрузки после этого диалога")]
    public string nextSceneName;

    [Header("Камера перехода")]
    public float cameraMoveHeight = 5f;
    public float cameraMoveHorizontal = 0f;

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