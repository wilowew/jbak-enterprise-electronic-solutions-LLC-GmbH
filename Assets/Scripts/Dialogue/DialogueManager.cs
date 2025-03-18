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
    public bool isDialogueActive;   
    private bool isTyping = false;    
    private Coroutine typingCoroutine;

    private bool isDialoguePaused = false;
    private int pausedLineIndex;
    private int pausedCharIndex;

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

    public bool IsDialogueActive => isDialogueActive;
    public bool IsDialoguePaused => isDialoguePaused;   

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
        if (isDialogueActive || PauseManager.Instance.IsPaused) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        PauseManager.Instance.UpdateTimeScale();

        DisplayNextLine();
    }

    public void PauseDialogue()
    {
        if (!isDialogueActive) return;

        isDialoguePaused = true;
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        if (isTyping)
        {
            pausedLineIndex = currentLineIndex;
            pausedCharIndex = dialogueText.text.Length;
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }
        else
        {
            pausedLineIndex = currentLineIndex;
            pausedCharIndex = 0;
        }

        PauseManager.Instance.UpdateTimeScale();
    }

    public void ResumeDialogue()
    {
        if (!isDialoguePaused) return;

        if (pausedLineIndex >= currentDialogue.Lines.Length)
        {
            EndDialogue();
            return;
        }

        isDialoguePaused = false;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        currentLineIndex = pausedLineIndex;

        if (pausedCharIndex > 0)
        {
            DialogueLine line = currentDialogue.Lines[currentLineIndex];
            typingCoroutine = StartCoroutine(TypeText(line, pausedCharIndex));
        }
        else
        {
            if (currentLineIndex < currentDialogue.Lines.Length)
            {
                DialogueLine line = currentDialogue.Lines[currentLineIndex];
                characterIcon.sprite = line.characterIcon;
                continueIndicator.SetActive(false);
                typingCoroutine = StartCoroutine(TypeText(line));
            }
        }

        PauseManager.Instance.UpdateTimeScale();
    }

    public void ForceEndDialogue()
    {
        EndDialogue();
    }

    public void DisplayNextLine(int startIndex = 0)
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
        characterIcon.sprite = line.characterIcon;
        continueIndicator.SetActive(false);

        typingCoroutine = StartCoroutine(TypeText(line, startIndex));
    }

    private IEnumerator TypeText(DialogueLine line, int startIndex = 0)
    {
        isTyping = true;

        string localizedName = LanguageManager.Instance.GetTerm(line.nameTermKey);
        string localizedText = LanguageManager.Instance.GetTerm(line.textTermKey);

        nameText.text = localizedName;
        nameText.color = line.nameColor;
        nameText.font = line.nameFont ?? nameText.font;
        nameText.gameObject.SetActive(!string.IsNullOrEmpty(localizedName));

        dialogueText.text = localizedText.Substring(0, startIndex);

        for (int i = startIndex; i < localizedText.Length; i++)
        {
            dialogueText.text += localizedText[i];

            float delay = typingSpeed;
            if (char.IsPunctuation(localizedText[i]))
                delay *= 3;

            yield return new WaitForSecondsRealtime(typingSpeed);

            if (!isTyping) break;
        }

        dialogueText.text = localizedText;
        isTyping = false;
        continueIndicator.SetActive(true);

        currentLineIndex++;
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
        isDialogueActive = false;
        isDialoguePaused = false;
        PauseManager.Instance.UpdateTimeScale();
    }

    private void ReloadCurrentDialogue()
    {
        if (!isDialogueActive) return;

        currentLineIndex = Mathf.Max(0, currentLineIndex - 1);
        DisplayNextLine();
    }

    private void Update()
    {
        if (!isDialogueActive || PauseManager.Instance.IsPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
                FinishTyping();
            else
                DisplayNextLine();
        }
    }
}