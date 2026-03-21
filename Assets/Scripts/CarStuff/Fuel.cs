using CarStuff;
using UnityEngine;
using UnityEngine.UI;

public class Fuel : MonoBehaviour
{
    private const float MAX_FUEL_ANGLE = -75;
    private const float ZERO_FUEL_ANGLE = 75;

    private Transform needleTransform;
    private Transform fuelLabelTemplateTransform;

    private PlayerController _playerController;
    private Image _gasIcon;

    private float fuelMax;
    private float fuelCurr;
    
    [Header("Colors")]
    [SerializeField] private Color lowFuelColor = Color.red;
    [SerializeField] private Color normalFuelColor = Color.white;

    private void Awake()
    {
        /*
        if (slider != null)
        {
            slider.interactable = false;
        }
        */
        needleTransform = GameObject.Find("Gauge_Needle").transform;
        fuelLabelTemplateTransform = GameObject.Find("FuelLabelTemplate").transform;
        
        //builds and runs but freezes editor
        //fuelLabelTemplateTransform.gameObject.SetActive(false)
        
        _playerController = GameObject.Find("Car").GetComponent<PlayerController>();
        _gasIcon = GameObject.Find("Gas_Icon_Blink").GetComponent<Image>();
        fuelMax = _playerController.GetMaxFuel();
        fuelCurr = _playerController.GetFuel();
        
        CreateFuelLabels();
        
        needleTransform.SetAsLastSibling();
    }

    public void SetMaxFuel(float fuel)
    {
        fuelCurr = fuel;
    }

    public void SetFuel(float fuel)
    {
        fuelCurr = fuel;
    }
    
    private void Update()
    {
        needleTransform.eulerAngles = new Vector3(0, 0, GetFuelRotation());

        if (fuelCurr < fuelMax / 4)
        {
            _gasIcon.color = lowFuelColor;
        }
        else
        {
            _gasIcon.color = normalFuelColor;
        }
    }
    
    private void CreateFuelLabels()
    {
        int labelAmount = 4;
        float totalAngleSize = ZERO_FUEL_ANGLE - MAX_FUEL_ANGLE;

        for (int i = 0; i <= labelAmount; i++)
        {
            Transform fuelLabelTransform = Instantiate(fuelLabelTemplateTransform, fuelLabelTemplateTransform.parent);
            float labelFuelNormalized = (float)i / labelAmount;
            float fuelLabelAngle = ZERO_FUEL_ANGLE - labelFuelNormalized * totalAngleSize;
            fuelLabelTransform.eulerAngles = new Vector3(0, 0, fuelLabelAngle);
            fuelLabelTransform.gameObject.SetActive(true);
        }
    }

    private float GetFuelRotation()
    {
        float totalAngleSize = ZERO_FUEL_ANGLE - MAX_FUEL_ANGLE;
        float fuelNormalized = fuelCurr / fuelMax;

        return ZERO_FUEL_ANGLE - fuelNormalized * totalAngleSize;
    }
}
