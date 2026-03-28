using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject instructionsPanel;
    public static bool IsPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
        instructionsPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (instructionsPanel.activeSelf)
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
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
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

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}