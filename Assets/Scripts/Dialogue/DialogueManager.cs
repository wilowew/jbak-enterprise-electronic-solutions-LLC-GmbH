using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject continueIndicator;

    [Header("Text Animation")]
    [SerializeField] private float typingSpeed = 0.05f;

    private Dialogue currentDialogue; 
    private int currentLineIndex;   
    private bool isDialogueActive;   
    private bool isTyping = false;    
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            dialoguePanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (isDialogueActive) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        Time.timeScale = 0f;

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogue.lines[currentLineIndex];
        currentLineIndex++;

        characterIcon.sprite = line.characterIcon;
        continueIndicator.SetActive(false);

        typingCoroutine = StartCoroutine(TypeText(line));
    }


    private IEnumerator TypeText(DialogueLine line)
    {
        isTyping = true;

        nameText.text = line.characterName;
        nameText.color = line.nameColor;
        nameText.font = line.nameFont ?? nameText.font; 
        nameText.gameObject.SetActive(!string.IsNullOrEmpty(line.characterName)); // стандартный шрифт

        dialogueText.text = "";

        foreach (char letter in line.text.ToCharArray()) // постепенная печать текста
        {
            dialogueText.text += letter;

            // Пауза после знаков препинания
            float delay = typingSpeed;
            if (char.IsPunctuation(letter))
                delay *= 3;

            yield return new WaitForSecondsRealtime(delay);

            if (!isTyping) break; // Проверка прерывания
        }

        dialogueText.text = line.text;
        isTyping = false;
        continueIndicator.SetActive(true);
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogue.lines[currentLineIndex - 1].text;
            isTyping = false;
            continueIndicator.SetActive(true);
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
        isDialogueActive = false;
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
                FinishTyping();
            else
                DisplayNextLine();
        }
    }
}