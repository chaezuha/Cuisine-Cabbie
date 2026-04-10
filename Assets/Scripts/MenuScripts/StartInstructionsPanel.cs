using CarStuff;
using UnityEngine;

public class StartInstructionsPanel : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject instructionsManual;

    [Header("Panels")]
    [SerializeField] private GameObject[] panels;

    [Header("Controls")]
    [SerializeField] private KeyCode openKey = KeyCode.H;

    [Header("Game References")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private PlayerAudioController playerAudioController;
    

    [Header("SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonSelectSound;
    [SerializeField] private AudioClip carStartSound;

    private int _currentPanelIndex;
    private static bool _hasShownThisSession;

    public static void ResetForNewSession()
    {
        _hasShownThisSession = false;
    }

    void Start()
    {
        for (int i = 0; i < panels.Length; i++)                                                                                                                                                                                                                                                                        
            panels[i].SetActive(i == 0);                                                                                                                                                                                                                                                                                 
        _currentPanelIndex = 0;                                                                                                                                                                                                                                                                                        

        if (!_hasShownThisSession)
        {
            _hasShownThisSession = true;
            InitialShowPanel();
        }
        else
        {
            instructionsManual.SetActive(false);
        }
    }

    void Update()
    {
        if (instructionsManual.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Closing here");
            ClosePanel();
        }
        else if (!instructionsManual.activeSelf && Input.GetKeyDown(openKey))
        {
            OpenPanel();
        }
    }

    public void JumpToPanel(int index)
    {
        Debug.Log("Jumping Panel Panel");

        if (index < 0 || index >= panels.Length) return;

        panels[_currentPanelIndex].SetActive(false);
        _currentPanelIndex = index;
        panels[_currentPanelIndex].SetActive(true);
    }

    public void ShowNextPanel()
    {
        Debug.Log("Next Panel");
        if (_currentPanelIndex < panels.Length - 1)
            JumpToPanel(_currentPanelIndex + 1);
    }

    public void ShowPreviousPanel()
    {
        if (_currentPanelIndex > 0)
            JumpToPanel(_currentPanelIndex - 1);
    }

    public void OpenPanel()
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetActive(i == 0);
        _currentPanelIndex = 0;

        instructionsManual.SetActive(true);
        if (playerUI != null) playerUI.SetActive(false);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseMenu.IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void InitialShowPanel()
    {
        instructionsManual.SetActive(true);
        if (playerUI != null) playerUI.SetActive(false);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseMenu.IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (sfxAudioSource != null && carStartSound != null)
        {
            sfxAudioSource.ignoreListenerPause = true;
            sfxAudioSource.PlayOneShot(carStartSound);
        }
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
        Debug.Log("Close Panel");

        if (sfxAudioSource != null && buttonSelectSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonSelectSound);
        }

        instructionsManual.SetActive(false);
        if (playerUI != null) playerUI.SetActive(true);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PauseMenu.IsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerAudioController == null)
        {
            playerAudioController = FindFirstObjectByType<PlayerAudioController>();
        }
        if (playerAudioController != null)
        {
            playerAudioController.StartMusic();
        }
    }
}
