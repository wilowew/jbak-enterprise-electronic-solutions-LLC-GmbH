using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class DialogueSound
    {
        [Tooltip("ID диалога из ScriptableObject")]
        public string dialogueID;

        [Tooltip("Звук для воспроизведения")]
        public AudioClip completionSound;

        [Tooltip("Задержка перед воспроизведением (секунды)")]
        public float delaySeconds = 0f;
    }

    [Header("Настройки звуков")]
    [SerializeField] private List<DialogueSound> dialogueSounds = new List<DialogueSound>();
    [SerializeField] private AudioSource audioSource;

    private Dictionary<string, DialogueSound> soundMap = new Dictionary<string, DialogueSound>();
    private Coroutine managerWaitCoroutine;

    private void Awake()
    {
        foreach (var ds in dialogueSounds)
        {
            if (!string.IsNullOrEmpty(ds.dialogueID) && ds.completionSound != null)
            {
                soundMap[ds.dialogueID] = ds;
            }
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
        else
        {
            managerWaitCoroutine = StartCoroutine(WaitForDialogueManager());
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }

        if (managerWaitCoroutine != null)
        {
            StopCoroutine(managerWaitCoroutine);
            managerWaitCoroutine = null;
        }
    }

    private IEnumerator WaitForDialogueManager()
    {
        while (DialogueManager.Instance == null)
        {
            yield return null;
        }
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == null) return;

        string id = endedDialogue.dialogueID;
        if (!string.IsNullOrEmpty(id) && soundMap.TryGetValue(id, out DialogueSound sound))
        {
            StartCoroutine(PlaySoundWithDelay(sound));
        }
    }

    private IEnumerator PlaySoundWithDelay(DialogueSound sound)
    {
        if (sound.delaySeconds > 0)
        {
            yield return new WaitForSecondsRealtime(sound.delaySeconds);
        }

        audioSource.PlayOneShot(sound.completionSound);
    }

    private void AddNewSoundEntry()
    {
        dialogueSounds.Add(new DialogueSound());
    }
}