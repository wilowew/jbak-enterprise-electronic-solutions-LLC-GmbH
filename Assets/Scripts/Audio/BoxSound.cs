using UnityEngine;

public class BoxSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip boxDraggingSound; // ���� �������������� �������
    private AudioSource audioSource; // ��������� AudioSource

    private Rigidbody2D rb; // Rigidbody �������
    private bool isPlayerNear = false; // ���� ��� ��������, ��������� �� ����� �����

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>(); 
        audioSource.clip = boxDraggingSound;
        audioSource.loop = true; // �������� ������������ �����
        audioSource.playOnAwake = false; // �� ������������� ���� �����
    }

    private void Update()
    {
        // ���������, �������� �� ������� � ��������� �� ����� �����
        if (isPlayerNear && rb.linearVelocity.magnitude > 0.1f) // ���� ������� ��������
        {
            if (!audioSource.isPlaying) // ���� ���� ��� �� ���������������
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying) // ���� ���� ���������������, �� ������� �� ��������
            {
                audioSource.Stop();
            }
        }
    }

    // ����� ����� �������� �������
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // ���������, ��� ������������ � �������
        {
            isPlayerNear = true;
        }
    }

    // ����� ����� ������� �� �������
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // ���������, ��� ����� ������� ���� ��������������
        {
            isPlayerNear = false;

            // ���������� ���������� ����
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
