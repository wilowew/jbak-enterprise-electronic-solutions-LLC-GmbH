using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // �������� ��� �������� ������� � ��������������

    [Header("Audio Sources")]
    public AudioSource backgroundMusicSource; // �������� ������� ������
    public AudioSource soundEffectsSource; // �������� �������� ��������
    public AudioSource ambientSource; // �������� ������������ ��������

    private void Awake()
    {
        // ����������, ��� AudioManager ���������� � ������������ ����������
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
