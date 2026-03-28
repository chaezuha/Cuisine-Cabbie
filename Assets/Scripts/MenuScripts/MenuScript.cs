using DeliveryMechanics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Button SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonSelectSound;

    private void Start()
    {
        if (musicAudioSource != null && menuMusic != null)
        {
            musicAudioSource.clip = menuMusic;
            musicAudioSource.loop = true;
            musicAudioSource.volume = musicVolume;
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
