using UnityEngine;
using UnityEngine.UI;

public class Fuel : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private Slider slider;

    private void Awake()
    {
        if (slider != null)
        {
            slider.interactable = false;
        }
    }

    public void SetMaxFuel(float fuel)
    {
        slider.maxValue = fuel;
        slider.value = fuel;
    }

    public void SetFuel(float fuel)
    {
        
        slider.value = fuel;
    }
}
