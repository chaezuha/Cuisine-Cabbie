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

    void Start()
    {
        pauseMenu.SetActive(false);
        instructionsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        if (playerUI != null) playerUI.SetActive(false);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        if (playerUI != null) playerUI.SetActive(true);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenInstructions()
    {
        pauseMenu.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        instructionsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void GoToMainMenu()
    {
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
        Application.Quit();
    }
}