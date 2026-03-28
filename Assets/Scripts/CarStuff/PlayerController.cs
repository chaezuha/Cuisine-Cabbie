using DeliveryMechanics;
using UnityEngine;

namespace CarStuff
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private WaypointBrain waypointBrain;
        [SerializeField] private PlayerAudioController playerAudioController;
        [SerializeField] private CollisionCameraShake collisionCameraShake;

        [Header("Fuel Settings")] [SerializeField]
        private Fuel fuelBar;

        [SerializeField] private float maxFuel = 1000.0f;
        [SerializeField] private float dropoffRefuelAmount = 300.0f;
        [SerializeField] private float dropoffDistanceMultiplier = 1.0f;
        [SerializeField] private float fuelUseRate = 1.0f;
        [SerializeField] private float coastingFuelUseRate = 0.1f;
        [SerializeField] private float crawlFuelUseRate = 0.25f;
        [SerializeField] private float neutralFuelUseRate = 0.01f;
        [SerializeField, Range(0.9f, 1f)] private float neutralSpeedRetentionPerSecond = 0.98f;
        
        [Header("Fuel Economy")]
        [SerializeField] private bool fuelEconomyEnabled = true;
        [SerializeField] private float lowSpeedFuelMultiplier = 1.5f;
        [SerializeField] private float highSpeedFuelMultiplier = 0.7f;
        [SerializeField] private float fuelEconomyMaxSpeed = -1f;
        
        private float _currentFuel;

        [Header("Shift Settings")]
        [SerializeField] private float shiftCooldown = 0.2f;
        [SerializeField] private float scrollShiftActivationThreshold = 0.35f;
        [SerializeField] private float scrollShiftReleaseThreshold = 0.05f;

        private KeyCode _forwardKey;
        private KeyCode _brakeKey;
        private KeyCode _driftKey;
        private KeyCode _shiftUpKey;
        private KeyCode _shiftDownKey;
        private KeyCode _leftKey;
        private KeyCode _rightKey;
        private bool _useMouseScrollToShift = false;
        private bool _invertScrollShift = false;
        private int _controlNum = 1;
        private float _shiftTimer = 0f;
        private bool _scrollShiftReady = true;

        private CarMovement _physics;
        private Gearbox _gearbox;
        private DeliveryTelemetry _telemetry;

        private int _gas;
        private bool _isBraking;
        private bool _isDrifting;
        private int _turnRate;
        private Vector3 _currentPosition;

        protected Animator m_Anim;

        public bool GetIsBraking()
        {
            return _isBraking;
        }

        public bool GetIsGasPressed()
        {
            return _gas == 1;
        }

        public bool HasFuel()
        {
            return _currentFuel > 0f;
        }

        private void ActivateControlV1()
        {
            _forwardKey = KeyCode.UpArrow;
            _brakeKey = KeyCode.DownArrow;
            _shiftUpKey = KeyCode.LeftShift;
            _shiftDownKey = KeyCode.LeftControl;
            _leftKey = KeyCode.LeftArrow;
            _rightKey = KeyCode.RightArrow;
            _driftKey = KeyCode.Space;

            _useMouseScrollToShift = false;
        }

        private void ActivateControlV2()
        {
            _forwardKey = KeyCode.W;
            _brakeKey = KeyCode.S;
            _leftKey = KeyCode.A;
            _rightKey = KeyCode.D;
            _driftKey = KeyCode.Space;
            
            _useMouseScrollToShift = true;
        }
        
        private void HandleScrollShifting()
        {
            _shiftTimer -= Time.deltaTime;
            float scroll = Input.mouseScrollDelta.y;
            float absScroll = Mathf.Abs(scroll);

            if (absScroll <= scrollShiftReleaseThreshold)
            {
                _scrollShiftReady = true;
                return;
            }

            if (!_scrollShiftReady || _shiftTimer > 0f || absScroll < scrollShiftActivationThreshold)
            {
                return;
            }

            _scrollShiftReady = false;
            _shiftTimer = shiftCooldown;
            int direction = scroll > 0 ? 1 : -1;
            if (_invertScrollShift) direction *= -1;
            TryShiftGear(direction);
        }

        private void OnValidate()
        {
            if (scrollShiftReleaseThreshold > scrollShiftActivationThreshold)
            {
                scrollShiftReleaseThreshold = scrollShiftActivationThreshold;
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            ActivateControlV2();
            _currentPosition = transform.position;
            _physics = GetComponent<CarMovement>();
            _gearbox = GetComponent<Gearbox>();
            m_Anim = GetComponent<Animator>();
            if (playerAudioController == null)
            {
                playerAudioController = FindFirstObjectByType<PlayerAudioController>();
            }

            _telemetry = FindFirstObjectByType<DeliveryTelemetry>();
            _currentFuel = maxFuel;
            fuelBar.SetMaxFuel(maxFuel);
        }

        // Update is called once per frame
        private void Update()
        {
            if (!PauseMenu.IsPaused)
            {
                waypointBrain.SetPlayerPos(_currentPosition);
                _currentPosition = transform.position;
                _gas = Input.GetKey(_forwardKey) ? 1 : 0;
                _isBraking = Input.GetKey(_brakeKey);
                _isDrifting = Input.GetKey(_driftKey);
                
                if (_useMouseScrollToShift)
                {
                    HandleScrollShifting();
                }
                else
                {
                    if (Input.GetKeyDown(_shiftDownKey))
                    {
                        TryShiftGear(-1);
                    }

                    if (Input.GetKeyDown(_shiftUpKey))
                    {
                        TryShiftGear(1);
                    }
                }

                if (Input.GetKeyDown(KeyCode.RightShift))
                {
                    SwitchControls();
                }

                // Animation Acceleration Checks
                if (Input.GetKeyDown(_forwardKey) && _gearbox.GetGearType() == 1)
                {
                    m_Anim.SetTrigger("Forward");
                }

                if (Input.GetKeyDown(_forwardKey) && _gearbox.GetGearType() == -1)
                {
                    m_Anim.SetTrigger("Reverse");
                }

                if (Input.GetKey(_forwardKey) && _gearbox.GetGearType() == 1)
                {
                    m_Anim.SetBool("Forward_Bool", true);
                }
                else
                {
                    m_Anim.SetBool("Forward_Bool", false);
                }

                if (Input.GetKey(_forwardKey) && _gearbox.GetGearType() == -1)
                {
                    m_Anim.SetBool("Reverse_Bool", true);
                }
                else
                {
                    m_Anim.SetBool("Reverse_Bool", false);
                }


                // Turn Animation Checks
                if (Input.GetKeyDown(_leftKey))
                {
                    m_Anim.SetTrigger("Left");
                }

                if (Input.GetKeyDown(_rightKey))
                {
                    m_Anim.SetTrigger("Right");
                }

                _turnRate = 0;
                if (Input.GetKey(_leftKey))
                {
                    _turnRate = -1;
                    m_Anim.SetBool("Left_Bool", true);
                }
                else
                {
                    m_Anim.SetBool("Left_Bool", false);
                }

                if (Input.GetKey(_rightKey))
                {
                    _turnRate = 1;
                    m_Anim.SetBool("Right_Bool", true);
                }
                else
                {
                    m_Anim.SetBool("Right_Bool", false);
                }
            }
        }

        public void SetInvertScrollShift(bool invert)
        {
            _invertScrollShift = invert;
        }

        public bool GetInvertScrollShift()
        {
            return _invertScrollShift;
        }

        public void SwitchControls()
        {
            if (_controlNum == 1)
            {
                _controlNum = 0;
                ActivateControlV1();
            }
            else
            {
                _controlNum = 1;
                ActivateControlV2();
            }
        }

        
        public float GetFuel()
        {
            return _currentFuel;
        }

        public float GetMaxFuel()
        {
            return maxFuel;
        }

        public void RefillFuelToMax()
        {
            _currentFuel = maxFuel;
            fuelBar.SetFuel(_currentFuel);
        }

        public void RefuelFromDropoff(float distanceMeters)
        {
            const float metersToFeet = 3.28084f;
            var distanceFeet = Mathf.Max(0f, distanceMeters * metersToFeet);
            var distanceBonus = distanceFeet * dropoffDistanceMultiplier;
            var refuelAmount = dropoffRefuelAmount + distanceBonus;
            AddFuel(refuelAmount);
        }


        private bool TryShiftGear(int shiftDirection)
        {
            if (!_isBraking)
            {
                playerAudioController?.PlayGearShiftFail();
                return false;
            }

            var previousGearType = _gearbox.GetGearType();
            var isTryingToEnterReverse = shiftDirection < 0 && previousGearType == 0;
            var absForwardSpeed = Mathf.Abs(_physics.GetForwardSpeed());
            if (isTryingToEnterReverse && absForwardSpeed > _physics.GetMinBrakeSpeed())
            {
                playerAudioController?.PlayReverseFail();
                return false;
            }

            if (shiftDirection > 0)
            {
                _gearbox.ShiftGearUp();
            }
            else if (shiftDirection < 0)
            {
                _gearbox.ShiftGearDown();
            }

            if (previousGearType == _gearbox.GetGearType())
            {
                playerAudioController?.PlayGearShiftFail();
                return false;
            }

            playerAudioController?.PlayGearShiftSuccess();
            return true;
        }

        private void FixedUpdate()
        {
            var gearType = _gearbox.GetGearType();
            var isGrounded = _physics.IsGrounded();
            var adjustedTurnRate = gearType == -1 ? -_turnRate : _turnRate;
            var absForwardSpeed = Mathf.Abs(_physics.GetForwardSpeed());
            
           _physics.ApplyDrift(_isDrifting);
            
            if (_isBraking)
            {
                _physics.ApplyBrake(_isBraking);
            }
            else if (_gas == 1 && _currentFuel > 0f && gearType != 0)
            {
                if (isGrounded)
                {
                    _physics.ApplyGas(_gas, gearType);
                    ConsumeFuel(fuelUseRate * GetFuelEconomyMultiplier());
                }
            }
            else
            {
                if (gearType != 0)
                {
                    if (isGrounded)
                    {
                        _physics.ApplyEngineBraking(gearType);

                        if (_currentFuel > 0f && absForwardSpeed <= _physics.GetMinBrakeSpeed())
                        {
                            _physics.Crawl(gearType);
                            ConsumeFuel(crawlFuelUseRate);
                        }
                        else if (_currentFuel > 0f)
                        {
                            ConsumeFuel(coastingFuelUseRate);
                        }
                    }
                }
                else
                {
                    if (_currentFuel > 0f)
                    {
                        ConsumeFuel(neutralFuelUseRate);
                    }

                    _physics.ApplyNeutralCoastDrag(neutralSpeedRetentionPerSecond);
                }
            }

            if (isGrounded)
            {
                _physics.ApplyTurn(adjustedTurnRate);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            float fuelBefore = _currentFuel;
            playerAudioController?.HandleCollision(collision);
            collisionCameraShake?.HandleCollision(collision);
            float fuelLost = fuelBefore - _currentFuel;
            _telemetry?.OnCollision(fuelLost);
        }

        private float GetFuelEconomyMultiplier()
        {
            if (!fuelEconomyEnabled) return 1f;
            var maxSpeed = fuelEconomyMaxSpeed > 0f ? fuelEconomyMaxSpeed : _physics.GetMaxForwardSpeed();
            var speedRatio = Mathf.Clamp01(Mathf.Abs(_physics.GetForwardSpeed()) / maxSpeed);
            return Mathf.Lerp(lowSpeedFuelMultiplier, highSpeedFuelMultiplier, speedRatio);
        }

        public void ConsumeFuel(float amount)
        {
            if (amount <= 0f || _currentFuel <= 0f)
            {
                return;
            }

            _currentFuel = Mathf.Max(0f, _currentFuel - amount);
            fuelBar.SetFuel(_currentFuel);
        }

        public void AddFuel(float amount)
        {
            if (amount <= 0f || _currentFuel >= maxFuel)
            {
                return;
            }

            _currentFuel = Mathf.Min(maxFuel, _currentFuel + amount);
            fuelBar.SetFuel(_currentFuel);
        }
    }
}
