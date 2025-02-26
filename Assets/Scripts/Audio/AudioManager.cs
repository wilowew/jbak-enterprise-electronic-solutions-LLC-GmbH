using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Синглтон для удобного доступа к аудиоменеджеру

    [Header("Audio Sources")]
    public AudioSource backgroundMusicSource; // Источник фоновой музыки
    public AudioSource soundEffectsSource; // Источник звуковых эффектов
    public AudioSource ambientSource; // Источник процедурного эмбиента

    private void Awake()
    {
        // Убеждаемся, что AudioManager существует в единственном экземпляре
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        backgroundMusicSource.clip = clip;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        soundEffectsSource.PlayOneShot(clip);
    }

    public void PlayAmbientSound(AudioClip clip)
    {
        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void PauseAllSounds()
    {
        if (backgroundMusicSource.isPlaying) backgroundMusicSource.Pause();
        if (ambientSource.isPlaying) ambientSource.Pause();
    }

    public void ResumeAllSounds()
    {
        if (backgroundMusicSource.clip != null && !backgroundMusicSource.isPlaying) backgroundMusicSource.UnPause();
        if (ambientSource.clip != null && !ambientSource.isPlaying) ambientSource.UnPause();
    }
}
