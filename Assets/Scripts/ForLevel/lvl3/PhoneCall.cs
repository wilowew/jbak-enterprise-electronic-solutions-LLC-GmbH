using UnityEngine;
using System.Collections;

public class PhoneCall : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private float interactionRadius = 2f;

    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue startDialogue;
    [SerializeField] private Dialogue approachDialogue;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip ringSound;
    [SerializeField][Range(0, 1)] private float volume = 1f;

    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private bool showGizmo = true;

    private CircleCollider2D phoneCollider;
    private AudioSource audioSource;
    private bool isActive;
    private bool callCompleted;

    private void Awake()
    {
        phoneCollider = gameObject.AddComponent<CircleCollider2D>();
        phoneCollider.radius = interactionRadius;
        phoneCollider.isTrigger = true;
        phoneCollider.enabled = false;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = ringSound;
        audioSource.volume = volume;
        audioSource.loop = true;
    }

    private void Start()
    {
        StartCoroutine(ActivatePhone());
    }

    private IEnumerator ActivatePhone()
    {
        yield return new WaitForSeconds(startDelay);

        if (startDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(startDialogue);
        }

        phoneCollider.enabled = true;
        isActive = true;

        if (ringSound != null)
        {
            audioSource.Play();
            Debug.Log("Phone ringing started");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || callCompleted) return;

        if (other.CompareTag("Player"))
        {
            CompleteCall();
        }
    }

    private void CompleteCall()
    {
        callCompleted = true;
        phoneCollider.enabled = false;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Phone ringing stopped");
        }

        if (approachDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(approachDialogue);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}