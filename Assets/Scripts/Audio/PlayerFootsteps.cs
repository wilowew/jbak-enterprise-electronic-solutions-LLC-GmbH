using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioSource footstepSource; // �������� ��� ��������������� ������ �����
    public float stepInterval = 0.4f; // �������� ����� ������

    // ������� ������ ��� ������ ������������
    public AudioClip[] grassFootstepSounds;
    public AudioClip[] pathwayFootstepSounds;
    public AudioClip[] defaultFootstepSounds;

    private float stepTimer = 0f;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement �� ������ �� �������! �������, ��� ������ PlayerMovement ��������� � ���������.");
        }

        if (footstepSource == null)
        {
            Debug.LogError("FootstepSource (AudioSource) �� ��������! ������� ��������� � ����������.");
        }
    }

    void Update()
    {
        if (playerMovement == null || playerMovement.Rb == null)
        {
            return; // �������, ���� playerMovement ��� Rb ����� null
        }

        // ���������� ��������� �������� Rb ������ ������� ��������� � rb
        if (playerMovement.Rb.linearVelocity.magnitude > 0.1f) // ���� �������� ���������
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // ����� �������, ���� �������� �����������
        }
    }

    void PlayFootstepSound()
    {
        // ���������� ������, �� ������� ��������� ��������
        Vector3 currentPosition = transform.position;
        Collider2D hit = Physics2D.OverlapPoint(currentPosition);

        if (hit != null)
        {
            // �������� ��� �������, �� ������� ����� ��������
            string areaTag = hit.gameObject.tag;

            AudioClip[] footstepSounds = null;

            // ���������� ������ ������ � ����������� �� ����
            switch (areaTag)
            {
                case "pathway":
                    footstepSounds = pathwayFootstepSounds;
                    break;
                case "grass":
                    footstepSounds = grassFootstepSounds;
                    break;
                default:
                    footstepSounds = defaultFootstepSounds;
                    break;
            }

            // ������������� ��������� ���� �� �������
            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                footstepSource.PlayOneShot(clip);
            }
        }
    }
}
