using CarStuff;
using DeliveryMechanics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject playerUI;

    [Header("Button SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonSelectSound;

    private bool _isShowing;
    private bool _inCredits;

    public bool IsShowing => _isShowing;

    void Start()
    {
        gameOverPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void Show()
    {
        if (_isShowing) return;
        _isShowing = true;

        gameOverPanel.SetActive(true);
        creditsPanel.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Hide()
    {
        if (!_isShowing || _inCredits) return;
        _isShowing = false;

        gameOverPanel.SetActive(false);
        if (playerUI != null) playerUI.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    public void RestartGame()
    {
        PlaySelectSound();
        Time.timeScale = 1f;
        PauseMenu.IsPaused = false;
        AudioListener.pause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (TelemetrySession.Instance != null)
        {
            var telemetry = FindFirstObjectByType<DeliveryTelemetry>();
            var brain = FindFirstObjectByType<DeliveryBrain>();
            int deliveries = brain != null ? brain.GetDeliveryCount() : 0;
            int rounds = telemetry != null ? telemetry.GetRoundNumber() : 0;
            TelemetrySession.Instance.FinishCurrentRun(deliveries, rounds, "Ran Out of Fuel");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowCredits()
    {
        PlaySelectSound();
        _inCredits = true;
        gameOverPanel.SetActive(false);
        creditsPanel.SetActive(true);
        if (playerUI != null) playerUI.SetActive(false);

        Time.timeScale = 0f;
        PauseMenu.IsPaused = true;
    }

    public void GoToMainMenu()
    {
        PlaySelectSound();
        StartInstructionsPanel.ResetForNewSession();
        Time.timeScale = 1f;
        PauseMenu.IsPaused = false;
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