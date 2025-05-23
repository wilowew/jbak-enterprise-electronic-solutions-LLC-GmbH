using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    [Header("�����������")]
    public string nameTermKey;
    [TextArea(3, 5)] public string textTermKey;

    [Header("����������")]
    public Color nameColor = Color.yellow;
    public TMP_FontAsset nameFont;
    public Sprite characterIcon;
    public bool isChoicePoint;
    public List<Choice> choices;

    [Header("���������� �������")]
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
    menuName = "�������/������� ������",
    order = 120)]

public class Dialogue : ScriptableObject
{
    [Header("�������� ���������")]
    [Tooltip("���������� ID ��� ������ (�����������)")]
    public string dialogueID;

    [Header("���������� �������")]
    [SerializeField] private DialogueLine[] lines;

    public DialogueLine[] Lines => lines;
    public int Length => lines?.Length ?? 0;

    [Header("�������� ����� �������")]
    [Tooltip("��������, ����� �������� �������� ����� ���������� ����� �������")]
    public bool usePostDialogueDelay = false;
    [Tooltip("����������� �������� ��������� �� ����� ��������")]
    public bool blockMovementDuringDelay = true;
    [Tooltip("����� �������� � �������� ����� ������� ���������� �������")]
    public float postDialogueDelayTime = 1f;

    [Header("��������� ������")]
    [Tooltip("������, ������� �������� ����� �����")]
    public Dialogue nextDialogue;

    [Header("���������� �������")]
    [Tooltip("������� ������ �������� �������")]
    public bool requireManualClose;

    [Header("������� �� �����")]
    [Tooltip("��� ����� ��� �������� ����� ����� �������")]
    public string nextSceneName;

    [Header("������ ��������")]
    public float cameraMoveHeight = 5f;
    public float cameraMoveHorizontal = 0f;

    [ContextMenu("��������� ����� �����������")]
    private void ValidateLocalizationKeys()
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.nameTermKey))
                Debug.LogWarning($"������ ���� ����� � ������� {name}");

            if (string.IsNullOrEmpty(line.textTermKey))
                Debug.LogWarning($"������ ���� ������ � ������� {name}");
        }
    }
}