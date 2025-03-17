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

    private void OnEnable()
    {
        if (LanguageManager.Instance)
            LanguageManager.Instance.OnLanguageChanged.AddListener(ReloadCurrentDialogue);
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance)
            LanguageManager.Instance.OnLanguageChanged.RemoveListener(ReloadCurrentDialogue);
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

        if (currentLineIndex >= currentDialogue.Lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogue.Lines[currentLineIndex];
        currentLineIndex++;

        characterIcon.sprite = line.characterIcon;
        continueIndicator.SetActive(false);

        typingCoroutine = StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(DialogueLine line)
    {
        isTyping = true;

        string localizedName = LanguageManager.Instance.GetTerm(line.nameTermKey);
        string localizedText = LanguageManager.Instance.GetTerm(line.textTermKey);

        nameText.text = localizedName;
        nameText.color = line.nameColor;
        nameText.font = line.nameFont ?? nameText.font;
        nameText.gameObject.SetActive(!string.IsNullOrEmpty(localizedName));

        dialogueText.text = "";

        foreach (char letter in localizedText.ToCharArray())
        {
            dialogueText.text += letter;

            float delay = typingSpeed;
            if (char.IsPunctuation(letter))
                delay *= 3;

            yield return new WaitForSecondsRealtime(delay);

            if (!isTyping) break;
        }

        dialogueText.text = localizedText;
        isTyping = false;
        continueIndicator.SetActive(true);
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            DialogueLine line = currentDialogue.Lines[currentLineIndex - 1];
            dialogueText.text = LanguageManager.Instance.GetTerm(line.textTermKey);
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

    private void ReloadCurrentDialogue()
    {
        if (!isDialogueActive) return;

        currentLineIndex = Mathf.Max(0, currentLineIndex - 1);
        DisplayNextLine();
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