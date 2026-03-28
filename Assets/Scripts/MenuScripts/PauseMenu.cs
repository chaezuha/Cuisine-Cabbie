using CarStuff;
using DeliveryMechanics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject instructionsPanel;
    public GameObject optionsPanel;
    public GameObject playerUI;
    public static bool IsPaused;

    [Header("Button SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonSelectSound;

    [Header("Game References")]
    [SerializeField] private PlayerAudioController playerAudioController;

    void Start()
    {
        pauseMenu.SetActive(false);
        instructionsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerAudioController == null)
        {
            playerAudioController = FindFirstObjectByType<PlayerAudioController>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            else if (instructionsPanel.activeSelf)
            {
                CloseInstructions();
            }
            else if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && buttonHoverSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonHoverSound);
        }
    }

    private void PlaySelectSound()
    {
        if (sfxAudioSource != null && buttonSelectSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonSelectSound);
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        if (playerUI != null) playerUI.SetActive(false);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerAudioController != null) playerAudioController.PauseAllAudio();
    }

    public void ResumeGame()
    {
        PlaySelectSound();
        pauseMenu.SetActive(false);
        if (playerUI != null) playerUI.SetActive(true);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerAudioController != null) playerAudioController.ResumeAllAudio();
    }

    public void OpenInstructions()
    {
        PlaySelectSound();
        pauseMenu.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        PlaySelectSound();
        instructionsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenOptions()
    {
        PlaySelectSound();
        pauseMenu.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        PlaySelectSound();
        optionsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void GoToMainMenu()
    {
        PlaySelectSound();
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (TelemetrySession.Instance != null)
        {
            var telemetry = FindFirstObjectByType<DeliveryTelemetry>();
            var brain = FindFirstObjectByType<DeliveryBrain>();
            int deliveries = brain != null ? brain.GetDeliveryCount() : 0;
            int rounds = telemetry != null ? telemetry.GetRoundNumber() : 0;
            TelemetrySession.Instance.FinishCurrentRun(deliveries, rounds, "Quit to Menu");
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        PlaySelectSound();
        Application.Quit();
    }
}