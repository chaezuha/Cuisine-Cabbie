using System.Collections.Generic;
using CarStuff;
using DeliveryMechanics;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Car UI")]
    [SerializeField] private TextMeshProUGUI neutralText;
    [SerializeField] private TextMeshProUGUI driveText;
    [SerializeField] private TextMeshProUGUI reverseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI brakeText;
    [SerializeField] private GameObject reverseSprite;
    [SerializeField] private GameObject neutralSprite;
    [SerializeField] private GameObject driveSprite;

    [Header("Delivery UI")]
    [SerializeField] private TextMeshProUGUI deliveryCounterText;
    [SerializeField] private TextMeshProUGUI packageListText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.white;
    
    private Gearbox _gearbox;
    private CarMovement _carMovement;
    private PlayerController _playerController;
    private DeliveryBrain _deliveryBrain;
    private GameOverPanel _gameOverPanel;
    

    private const float MetersPerSecondToMph = 2.237f;

    void Awake()
    {
        if (_gearbox == null)
        {
            _gearbox = FindFirstObjectByType<Gearbox>();
        }

        if (_carMovement == null)
        {
            _carMovement = FindFirstObjectByType<CarMovement>();
        }

        if (_playerController == null)
        {
            _playerController = FindFirstObjectByType<PlayerController>();
        }

        if (_deliveryBrain == null)
        {
            _deliveryBrain = FindFirstObjectByType<DeliveryBrain>();
        }

        if (_gameOverPanel == null)
        {
            _gameOverPanel = FindFirstObjectByType<GameOverPanel>();
        }
    }

    void Update()
    {
        UpdateGearText();
        UpdateSpeedText();
        UpdateBrakeText();
        UpdateDeliveryUI();
        UpdateGameOver();
    }

    private void UpdateGameOver()
    {
        if (_gameOverPanel == null)
        {
            _gameOverPanel = FindFirstObjectByType<GameOverPanel>();
        }
        if (_playerController == null)
        {
            _playerController = FindFirstObjectByType<PlayerController>();
        }
        if (_gameOverPanel == null || _playerController == null) return;

        if (_playerController.GetFuel() <= 0)
        {
            _gameOverPanel.Show();
        }
        else if (_gameOverPanel.IsShowing)
        {
            _gameOverPanel.Hide();
        }
    }
    private void UpdateGearText()
    {
        if (_gearbox == null)
        {
            return;
        }

        var gearType = _gearbox.GetGearType();
        
        /*
        SetTextColor(neutralText, gearType == 0 ? activeColor : inactiveColor);
        SetTextColor(driveText, gearType == 1 ? activeColor : inactiveColor);
        SetTextColor(reverseText, gearType == -1 ? activeColor : inactiveColor);
        */

        if (gearType == 0)
        {
            neutralText.gameObject.SetActive(true);
            driveText.gameObject.SetActive(false);
            reverseText.gameObject.SetActive(false);
            
            reverseSprite.gameObject.SetActive(false);
            neutralSprite.gameObject.SetActive(true);
            driveSprite.gameObject.SetActive(false);
        }
        else if (gearType == 1)
        {
            neutralText.gameObject.SetActive(false);
            driveText.gameObject.SetActive(true);
            reverseText.gameObject.SetActive(false);
            
            reverseSprite.gameObject.SetActive(false);
            neutralSprite.gameObject.SetActive(false);
            driveSprite.gameObject.SetActive(true);
        }
        else if (gearType == -1)
        {
            neutralText.gameObject.SetActive(false);
            driveText.gameObject.SetActive(false);
            reverseText.gameObject.SetActive(true);
            
            reverseSprite.gameObject.SetActive(true);
            neutralSprite.gameObject.SetActive(false);
            driveSprite.gameObject.SetActive(false);
        }
    }

    private void UpdateSpeedText()
    {
        if (speedText == null || _carMovement == null)
        {
            return;
        }

        var mph = Mathf.FloorToInt(_carMovement.GetSpeedMagnitude() * MetersPerSecondToMph);
        speedText.text = "SPEED: " + mph + "mp/h";
    }

    private void UpdateBrakeText()
    {
        if (brakeText == null)
        {
            return;
        }

        var isBraking = _playerController != null && _playerController.GetIsBraking();
        brakeText.color = isBraking ? activeColor : inactiveColor;
    }

    private void UpdateDeliveryUI()
    {
        if (_deliveryBrain == null)
        {
            return;
        }

        if (deliveryCounterText != null)
        {
            deliveryCounterText.text = "Successful Deliveries: " + _deliveryBrain.GetDeliveryCount();
        }

        if (packageListText != null)
        {
            if (!_deliveryBrain.HasPackages())
            {
                packageListText.text = "No packages held\nReturn to depot for pickup";
            }
            else
            {
                var names = new List<string>();
                foreach (var dropoff in _deliveryBrain.HeldPackages)
                {
                    names.Add("Package for " + dropoff.GetDropOffId());
                }

                names.Sort();
                packageListText.text = string.Join("\n", names);
            }
        }

        if (messageText != null)
        {
            var msg = _deliveryBrain.GetCurrentMessage();
            if (!string.IsNullOrEmpty(msg))
            {
                messageText.text = msg;
                messageText.color = Color.red;
                messageText.gameObject.SetActive(true);
            }
            else
            {
                messageText.gameObject.SetActive(false);
            }
        }
    }

    private static void SetTextColor(TextMeshProUGUI target, Color color)
    {
        if (target != null)
        {
            target.color = color;
        }
    }
}
