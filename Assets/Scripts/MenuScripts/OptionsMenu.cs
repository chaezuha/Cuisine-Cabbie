using CarStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown fpsTMP_Dropdown;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle invertScrollToggle;

    [Header("Game References")]
    [SerializeField] private PlayerController playerController;

    private readonly int[] _fpsOptions = { 30, 60, 120, 144, -1 };

    private void OnEnable()
    {
        InitFpsTMP_Dropdown();
        volumeSlider.value = AudioListener.volume;

        if (playerController != null)
        {
            invertScrollToggle.isOn = playerController.GetInvertScrollShift();
        }

        fpsTMP_Dropdown.onValueChanged.AddListener(OnFpsChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        invertScrollToggle.onValueChanged.AddListener(OnInvertScrollChanged);
    }

    private void OnDisable()
    {
        fpsTMP_Dropdown.onValueChanged.RemoveListener(OnFpsChanged);
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        invertScrollToggle.onValueChanged.RemoveListener(OnInvertScrollChanged);
    }

    private void InitFpsTMP_Dropdown()
    {
        fpsTMP_Dropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < _fpsOptions.Length; i++)
        {
            options.Add(_fpsOptions[i] == -1 ? "Unlimited" : _fpsOptions[i].ToString());
            if (Application.targetFrameRate == _fpsOptions[i])
            {
                currentIndex = i;
            }
        }

        fpsTMP_Dropdown.AddOptions(options);
        fpsTMP_Dropdown.value = currentIndex;
    }

    private void OnFpsChanged(int index)
    {
        Application.targetFrameRate = _fpsOptions[index];
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    private void OnInvertScrollChanged(bool inverted)
    {
        if (playerController != null)
        {
            playerController.SetInvertScrollShift(inverted);
        }
    }
}
