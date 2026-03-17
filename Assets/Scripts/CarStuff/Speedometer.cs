using System;
using CarStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    private const float MAX_SPEED_ANGLE = -120;
    private const float ZERO_SPEED_ANGLE = 120;

    private Transform needleTransform;
    private Transform speedLabelTemplateTransform;

    private CarMovement _carMovement;
    private const float MetersPerSecondToMph = 2.237f;

    private float speedMax;
    private float speed;

    private void Awake()
    {
        needleTransform = transform.Find("Needle");
        speedLabelTemplateTransform = transform.Find("SpeedLabelTemplate");
        speedLabelTemplateTransform.gameObject.SetActive(false);

        speed = 0f;
        speedMax = 140f;
        
        CreateSpeedLabels();
        
        needleTransform.SetAsLastSibling();

        _carMovement = GameObject.Find("Car").GetComponent<CarMovement>();
    }

    private void Update()
    {
        speed = _carMovement.GetSpeedMagnitude() * MetersPerSecondToMph;
        if (speed > speedMax) speed = speedMax;

        needleTransform.eulerAngles = new Vector3(0, 0, GetSpeedRotation());
    }

    private void CreateSpeedLabels()
    {
        int labelAmount = 7;
        float totalAngleSize = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;

        for (int i = 0; i <= labelAmount; i++)
        {
            Transform speedLabelTransform = Instantiate(speedLabelTemplateTransform, transform);
            float labelSpeedNormalized = (float)i / labelAmount;
            float speedLabelAngle = ZERO_SPEED_ANGLE - labelSpeedNormalized * totalAngleSize;
            speedLabelTransform.eulerAngles = new Vector3(0, 0, speedLabelAngle);
            speedLabelTransform.Find("SpeedText").GetComponent<TextMeshProUGUI>().text =
                Mathf.RoundToInt(labelSpeedNormalized * speedMax).ToString();
            speedLabelTransform.Find("SpeedText").eulerAngles = Vector3.zero;
            speedLabelTransform.gameObject.SetActive(true);
        }
    }

    private float GetSpeedRotation()
    {
        float totalAngleSize = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;
        float speedNormalized = speed / speedMax;

        return ZERO_SPEED_ANGLE - speedNormalized * totalAngleSize;
    }
}
