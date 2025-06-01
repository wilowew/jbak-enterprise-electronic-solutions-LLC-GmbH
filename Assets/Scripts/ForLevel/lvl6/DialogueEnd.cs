using UnityEngine;
using System.Collections;

public class DialogueEndEffect : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("������, ����� �������� ����������� ������")]
    public Dialogue targetDialogue;

    [Header("��������� �������")]
    [Tooltip("�������� ����� �������� (�������)")]
    public float delay = 1f;
    [Tooltip("���� ��� ���������������")]
    public AudioClip soundEffect;
    [Tooltip("������ ������� ������")]
    public GameObject particleEffectPrefab;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; 
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
        else
        {
            Debug.LogWarning("DialogueManager �� ������!");
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == targetDialogue)
        {
            StartCoroutine(PlayEffectAfterDelay());
        }
    }

    private IEnumerator PlayEffectAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        if (soundEffect != null)
        {
            audioSource.PlayOneShot(soundEffect);
        }

        if (particleEffectPrefab != null)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}