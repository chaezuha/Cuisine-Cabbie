using UnityEngine;

public class StartInstructionsPanel : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject startInstructionsPanel;

    [Header("Game References")]
    [SerializeField] private GameObject playerUI;

    [Header("Button SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonSelectSound;

    private static bool _hasShownThisSession;

    void Start()
    {
        if (!_hasShownThisSession)
        {
            _hasShownThisSession = true;
            ShowPanel();
        }
        else
        {
            startInstructionsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (startInstructionsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    private void ShowPanel()
    {
        startInstructionsPanel.SetActive(true);
        if (playerUI != null) playerUI.SetActive(false);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseMenu.IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && buttonHoverSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void ClosePanel()
    {
        if (sfxAudioSource != null && buttonSelectSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonSelectSound);
        }

        startInstructionsPanel.SetActive(false);
        if (playerUI != null) playerUI.SetActive(true);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PauseMenu.IsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
