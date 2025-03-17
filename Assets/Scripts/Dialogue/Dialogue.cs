using UnityEngine;
using TMPro;

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