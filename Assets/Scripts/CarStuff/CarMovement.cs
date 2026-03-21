using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CarStuff
{
    public class CarMovement : MonoBehaviour
    {
        [Header("Forward/Backward physics")] [SerializeField]
        private float forceMultiplier;

        [SerializeField] private float maxSpeedForward;
        [SerializeField] private float maxSpeedBackward;
        [SerializeField] private float crawlSpeed;
        [SerializeField] private float maxCrawlSpeed;

        [Header("Braking physics")] [SerializeField]
        private float brakeMultiplier;

        [SerializeField] private float engineBrakeFactor = 0.15f;
        [SerializeField] private float engineBrakeMinSpeed = 0.5f;

        [Header("Turning Physics")] [SerializeField]
        private float turnMultiplier;
        [SerializeField] private float gripFactor;
        [SerializeField] private float turnFriction;

        [Header("Raycast Suspension")] [SerializeField]
        private float springStrength = 500f;
        [SerializeField] private float damperStrength = 50f;
        [SerializeField] private float restLength = 1.5f;
        [SerializeField] private Transform[] wheelGroundChecks;
        
        [Header("Camera")]
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        
        
        private float _distanceToGround = 1.5f;
        private const float MetersPerSecondToMph = 2.237f;

        private Rigidbody _rb;

        public void ApplyBrake(bool brakeCondition)
        {
            if (!brakeCondition)
            {
                return;
            }
            
            var speed = _rb.linearVelocity.magnitude;
            
            if (speed > 0.15f)
            {
                var direction = -_rb.linearVelocity.normalized;
                _rb.AddForce(direction * brakeMultiplier, ForceMode.Acceleration);
            }
            else
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity *= 0.9f;
            }
        }

        public void ApplyDrift(bool isDrifiting)
        {
            if (isDrifiting == false)
            {
                float maxGrip = 50.0f;
                float seconds = 10.0f;
                if (gripFactor < maxGrip)
                {
                    float newGrip = maxGrip/seconds * Time.deltaTime;
                    if (gripFactor + newGrip > maxGrip)
                    {
                        gripFactor = maxGrip;
                    }
                    else
                    {
                        gripFactor += newGrip;
                    }
                }
            }
            else
            {
                gripFactor = 1.0f;
                Debug.Log("APPLY GRIP: " + gripFactor);
            }
        }

        public void ApplyGas(int gasCondition, int gearType)
        {
            if (gasCondition != 1)
            {
                return;
            }

            var forwardSpeed = GetForwardSpeed();
            var flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            
            switch (gearType)
            {
                // Forward
                case 1 when forwardSpeed < maxSpeedForward:
                    _rb.AddForce(flatForward * forceMultiplier, ForceMode.Acceleration);
                    break;
                // Reverse
                case -1 when forwardSpeed > -maxSpeedBackward:
                    _rb.AddForce(-flatForward * (forceMultiplier / 1.2f), ForceMode.Acceleration);
                    break;
            }
        }

        public void ApplyTurn(int turnCondition)
        {
            if (turnCondition == 0)
            {
                return;
            }

            var absForwardSpeed = Mathf.Abs(GetForwardSpeed());
            if (absForwardSpeed <= 0.1f)
            {
                return;
            }

            var turnRate = CalculateTurnSpeed(absForwardSpeed);
            var turnAmount = turnCondition * turnRate * Time.fixedDeltaTime;
            var turnDelta = Quaternion.Euler(0f, turnAmount, 0f);
            _rb.MoveRotation(_rb.rotation * turnDelta);

            var turnDrag = Mathf.Pow(turnFriction, Time.fixedDeltaTime);
            _rb.linearVelocity *= turnDrag;
        }

        public void ApplyEngineBraking(int gearType)
        {
            if (gearType == 0)
            {
                return;
            }

            var forwardSpeed = GetForwardSpeed();
            if (Mathf.Abs(forwardSpeed) < engineBrakeMinSpeed)
            {
                return;
            }

            var movingWithGear = (gearType == 1 && forwardSpeed > 0f) || (gearType == -1 && forwardSpeed < 0f);
            if (!movingWithGear)
            {
                return;
            }

            var opposingDirection = forwardSpeed > 0f ? -transform.forward : transform.forward;
            var engineBrakeForce = brakeMultiplier * engineBrakeFactor;
            _rb.AddForce(opposingDirection * engineBrakeForce, ForceMode.Acceleration);
        }

        public void ApplyNeutralCoastDrag(float speedRetentionPerSecond)
        {
            var clampedRetention = Mathf.Clamp(speedRetentionPerSecond, 0f, 1f);
            if (Mathf.Approximately(clampedRetention, 1f))
            {
                return;
            }

            var drag = Mathf.Pow(clampedRetention, Time.fixedDeltaTime);
            _rb.linearVelocity *= drag;
        }

        private void AccountForGrip()
        {
            var slidingSpeed = GetSidewaysSpeed();
            var counterForce = -transform.right * slidingSpeed * gripFactor;
            _rb.AddForce(counterForce, ForceMode.Acceleration);
        }

        private float CalculateTurnSpeed(float currentSpeed)
        {
            var safeMaxForwardSpeed = Mathf.Max(0.01f, maxSpeedForward);
            var speedRatio = Mathf.Clamp01(currentSpeed / safeMaxForwardSpeed);
            var rampUp = Mathf.Clamp01(currentSpeed / 5f);
            var speedDamping = Mathf.Lerp(1f, 0.3f, speedRatio);
            return rampUp * speedDamping * turnMultiplier;
        }

        public float GetForwardSpeed()
        {
            return Vector3.Dot(_rb.linearVelocity, transform.forward);
        }

        public float GetSpeedMiles()
        { 
            return Mathf.FloorToInt(GetSpeedMagnitude() * MetersPerSecondToMph);
        }
        
        public float GetMinBrakeSpeed()
        {
            return engineBrakeMinSpeed;
        }

        public float GetSpeedMagnitude()
        {
            return _rb.linearVelocity.magnitude;
        }

        private float GetSidewaysSpeed()
        {
            return Vector3.Dot(_rb.linearVelocity, transform.right);
        }

        public bool IsGrounded()
        {
            var hasValidWheelRef = false;
            if (wheelGroundChecks != null && wheelGroundChecks.Length > 0)
            {
                foreach (var wheel in wheelGroundChecks)
                {
                    if (wheel == null)
                    {
                        continue;
                    }

                    hasValidWheelRef = true;
                    if (Physics.Raycast(wheel.position, -wheel.up, _distanceToGround))
                    {
                        return true;
                    }
                }

                if (hasValidWheelRef)
                {
                    return false;
                }
            }

            return Physics.Raycast(transform.position, -transform.up, _distanceToGround);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        }

        void FixedUpdate()
        {
            _distanceToGround = restLength + 0.4f;
            foreach (var wheel in wheelGroundChecks)
            {
                if (Physics.Raycast(wheel.position, -transform.up, out var hit, restLength))
                {
                    float compression = 1f - (hit.distance / restLength);
                    float springForce = compression * springStrength;
                    float damperForce = -Vector3.Dot(
                        _rb.GetPointVelocity(wheel.position), hit.normal
                    ) * damperStrength;

                    _rb.AddForceAtPosition(
                        hit.normal * (springForce + damperForce), wheel.position
                    );
                }
            }

            if (IsGrounded())
            {
                AccountForGrip();
            }
            
            var speed = _rb.linearVelocity.magnitude;
            orbitalFollow.HorizontalAxis.Recentering.Enabled = speed > 1.0f;
            orbitalFollow.VerticalAxis.Recentering.Enabled = speed > 1.0f;
        }

        //idk the term but its like car crawl forward/reverse slowly when on drive/reverse and not pressing gas
        public void Crawl(int gearType)
        {
            var forwardSpeed = GetForwardSpeed();

            var flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            if (gearType == 1 && forwardSpeed < maxCrawlSpeed)
            {
                _rb.AddForce(flatForward * crawlSpeed, ForceMode.Acceleration);
            }

            // Reverse
            if (gearType == -1 && forwardSpeed > -maxCrawlSpeed)
            {
                _rb.AddForce(-flatForward * (crawlSpeed / 1.2f), ForceMode.Acceleration);
            }
        }
    }
}