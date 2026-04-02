using DeliveryMechanics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip menuMusicIntro;
    [SerializeField] private AudioClip menuMusicLoop;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    private AudioSource _loopAudioSource;

    [Header("Button SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonSelectSound;

    private void Start()
    {
        if (musicAudioSource == null)
        {
            return;
        }

        musicAudioSource.volume = musicVolume;

        if (menuMusicIntro != null && menuMusicLoop != null)
        {
            _loopAudioSource = gameObject.AddComponent<AudioSource>();
            _loopAudioSource.clip = menuMusicLoop;
            _loopAudioSource.loop = true;
            _loopAudioSource.volume = musicVolume;
            _loopAudioSource.playOnAwake = false;

            musicAudioSource.clip = menuMusicIntro;
            musicAudioSource.loop = false;
            musicAudioSource.Play();

            double introDuration = (double)menuMusicIntro.samples / menuMusicIntro.frequency;
            _loopAudioSource.PlayScheduled(AudioSettings.dspTime + introDuration);
        }
        else if (menuMusicIntro != null)
        {
            musicAudioSource.clip = menuMusicIntro;
            musicAudioSource.loop = false;
            musicAudioSource.Play();
        }
        else if (menuMusicLoop != null)
        {
            musicAudioSource.clip = menuMusicLoop;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }

    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && buttonHoverSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void PlaySelectSound()
    {
        if (sfxAudioSource != null && buttonSelectSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonSelectSound);
        }
    }

    public void PlayGame()
    {
        PlaySelectSound();
        if (TelemetrySession.Instance == null)
        {
            var go = new GameObject("TelemetrySession");
            go.AddComponent<TelemetrySession>();
        }

        SceneManager.LoadScene("Driving");
    }

    public void QuitGame()
    {
        PlaySelectSound();
        Application.Quit();
    }
    
}
