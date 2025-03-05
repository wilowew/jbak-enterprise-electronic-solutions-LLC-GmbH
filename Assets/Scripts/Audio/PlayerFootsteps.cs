using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioSource footstepSource; // Источник для воспроизведения звуков шагов
    public float stepInterval = 0.4f; // Интервал между шагами

    // Массивы звуков для разных поверхностей
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
            Debug.LogError("PlayerMovement не найден на объекте! Убедись, что скрипт PlayerMovement прикреплён к персонажу.");
        }

        if (footstepSource == null)
        {
            Debug.LogError("FootstepSource (AudioSource) не привязан! Проверь настройки в инспекторе.");
        }
    }

    void Update()
    {
        if (playerMovement == null || playerMovement.Rb == null)
        {
            return; // Выходим, если playerMovement или Rb равны null
        }

        // Используем публичное свойство Rb вместо прямого обращения к rb
        if (playerMovement.Rb.linearVelocity.magnitude > 0.1f) // Если персонаж двигается
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
            stepTimer = 0f; // Сброс таймера, если персонаж остановился
        }
    }

    void PlayFootstepSound()
    {
        // Определяем объект, на котором находится персонаж
        Vector3 currentPosition = transform.position;
        Collider2D hit = Physics2D.OverlapPoint(currentPosition);

        if (hit != null)
        {
            // Получаем тег объекта, на котором стоит персонаж
            string areaTag = hit.gameObject.tag;

            AudioClip[] footstepSounds = null;

            // Определяем массив звуков в зависимости от тега
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

            // Воспроизводим случайный звук из массива
            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                footstepSource.PlayOneShot(clip);
            }
        }
    }
}
