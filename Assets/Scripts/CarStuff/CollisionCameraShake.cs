using Unity.Cinemachine;
using UnityEngine;

namespace CarStuff
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CollisionCameraShake : MonoBehaviour
    {
        [Header("Impact Thresholds (mph)")]
        [SerializeField] private float lowImpactThresholdMph = 5f;
        [SerializeField] private float mediumImpactThresholdMph = 12f;
        [SerializeField] private float highImpactThresholdMph = 22f;

        [Header("Shake Intensity")]
        [SerializeField] private float lowShakeForce = 0.3f;
        [SerializeField] private float mediumShakeForce = 0.7f;
        [SerializeField] private float highShakeForce = 1.5f;

        private const float MetersPerSecondToMph = 2.237f;
        private CinemachineImpulseSource _impulseSource;

        private void Awake()
        {
            _impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void HandleCollision(Collision collision)
        {
            var impactSpeedMph = collision.relativeVelocity.magnitude * MetersPerSecondToMph;
            if (impactSpeedMph < lowImpactThresholdMph)
                return;

            float force;
            if (impactSpeedMph >= highImpactThresholdMph)
                force = highShakeForce;
            else if (impactSpeedMph >= mediumImpactThresholdMph)
                force = mediumShakeForce;
            else
                force = lowShakeForce;

            _impulseSource.GenerateImpulse(force);
        }
    }
}
