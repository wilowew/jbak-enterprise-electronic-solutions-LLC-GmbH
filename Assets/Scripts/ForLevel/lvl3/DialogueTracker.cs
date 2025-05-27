using UnityEngine;
using System.Collections.Generic;

public class DialogueTracker : MonoBehaviour
{
    public static DialogueTracker Instance { get; private set; }

    private HashSet<string> _playedDialogueIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkDialogueAsPlayed(string dialogueID)
    {
        if (!string.IsNullOrEmpty(dialogueID))
            _playedDialogueIDs.Add(dialogueID);
    }

    public bool HaveAllDialoguePlayed(List<string> requiredIDs)
    {
        if (requiredIDs == null || requiredIDs.Count == 0) return true;

        foreach (string id in requiredIDs)
        {
            if (!_playedDialogueIDs.Contains(id)) return false;
        }
        return true;
    }
}
