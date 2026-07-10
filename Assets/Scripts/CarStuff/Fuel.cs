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
        // Scoped lookups: these are children of Fuel_Gauge, and Transform.Find
        // also finds inactive children (the label template gets deactivated
        // after the labels are built).
        var needle = transform.Find("Gauge_Needle");
        var labelTemplate = transform.Find("FuelLabelTemplate");
        var gasIcon = transform.Find("Gas_Icon_Blink");
        var carGo = GameObject.Find("Car");

        if (needle == null || labelTemplate == null || carGo == null || gasIcon == null)
        {
            Debug.LogError("Fuel: missing scene object(s) — " +
                           (needle == null ? "'Gauge_Needle' " : "") +
                           (labelTemplate == null ? "'FuelLabelTemplate' " : "") +
                           (carGo == null ? "'Car' " : "") +
                           (gasIcon == null ? "'Gas_Icon_Blink' " : "") +
                           "not found.");
            enabled = false;
            return;
        }

        needleTransform = needle;
        fuelLabelTemplateTransform = labelTemplate;
        _playerController = carGo.GetComponent<PlayerController>();
        _gasIcon = gasIcon.GetComponent<Image>();
        fuelMax = _playerController.GetMaxFuel();
        fuelCurr = _playerController.GetFuel();

        CreateFuelLabels();

        needleTransform.SetAsLastSibling();
    }

    public void SetMaxFuel(float fuel)
    {
        fuelMax = fuel;
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

        fuelLabelTemplateTransform.gameObject.SetActive(false);
    }

    private float GetFuelRotation()
    {
        float totalAngleSize = ZERO_FUEL_ANGLE - MAX_FUEL_ANGLE;
        float fuelNormalized = fuelCurr / fuelMax;

        return ZERO_FUEL_ANGLE - fuelNormalized * totalAngleSize;
    }
}
