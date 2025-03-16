using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [Header("Основные настройки")]
    public string characterName;    
    [TextArea(3, 5)] public string text; 
    public Sprite characterIcon;      

    [Header("Стилизация")]
    public Color nameColor = Color.yellow; 
    public TMP_FontAsset nameFont;  
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;    
}