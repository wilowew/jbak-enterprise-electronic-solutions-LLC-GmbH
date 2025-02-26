using UnityEngine;

public class BoxSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip boxDraggingSound; // Звук перетаскивания коробки
    private AudioSource audioSource; // Локальный AudioSource

    private Rigidbody2D rb; // Rigidbody коробки
    private bool isPlayerNear = false; // Флаг для проверки, находится ли игрок рядом

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>(); 
        audioSource.clip = boxDraggingSound;
        audioSource.loop = true; // Включаем зацикливание звука
        audioSource.playOnAwake = false; // Не воспроизводим звук сразу
    }

    private void Update()
    {
        // Проверяем, движется ли коробка и находится ли игрок рядом
        if (isPlayerNear && rb.linearVelocity.magnitude > 0.1f) // Если коробка движется
        {
            if (!audioSource.isPlaying) // Если звук ещё не воспроизводится
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying) // Если звук воспроизводится, но коробка не движется
            {
                audioSource.Stop();
            }
        }
    }

    // Когда игрок касается коробки
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Проверяем, что столкновение с игроком
        {
            isPlayerNear = true;
        }
    }

    // Когда игрок отходит от коробки
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Проверяем, что игрок покинул зону взаимодействия
        {
            isPlayerNear = false;

            // Немедленно прекращаем звук
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
