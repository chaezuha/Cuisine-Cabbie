using UnityEngine;

namespace CarStuff
{
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        [Header("Engine Loops")]
        [SerializeField] private AudioClip idleLoop;
        [SerializeField] private AudioClip accelerateLoop;
        [SerializeField] private AudioClip reverseLoop;

        [Header("Driving One-Shots")]
        [SerializeField] private AudioClip lowCrashSound;
        [SerializeField] private AudioClip mediumCrashSound;
        [SerializeField] private AudioClip highCrashSound;
        [SerializeField] private AudioClip shiftGearSound;
        [SerializeField] private AudioClip reverseFailSound;
        [SerializeField] private AudioClip gearShiftFailSound;

        [Header("Delivery One-Shots")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropoffSound;
        [SerializeField] private AudioClip errorSound;

        [Header("Tuning Specifics")]
        [SerializeField] private float lowCrashImpactThresholdMph = 5f;
        [SerializeField] private float mediumCrashImpactThresholdMph = 12f;
        [SerializeField] private float highCrashImpactThresholdMph = 22f;
        [SerializeField] private float crashSoundCooldown = 0.5f;
        [SerializeField] private Collider hitSoundCollider;

        private const float MetersPerSecondToMph = 2.237f;
        private AudioClip _currentLoop;
        private PlayerController _playerController;
        private Gearbox _gearbox;
        private float _nextCrashSoundTime;

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = FindFirstObjectByType<PlayerController>();
            }

            if (_gearbox == null)
            {
                _gearbox = FindFirstObjectByType<Gearbox>();
            }

            if (engineAudioSource == null)
            {
                engineAudioSource = GetComponent<AudioSource>();
            }

            if (sfxAudioSource == null)
            {
                sfxAudioSource = engineAudioSource;
            }
        }

        private void Start()
        {
            SetEngineLoop(idleLoop);
        }

        private void Update()
        {
            UpdateEngineLoop();
        }

        public void HandleCollision(Collision collision)
        {
            if (Time.time < _nextCrashSoundTime)
            {
                return;
            }

            var impactSpeedMph = collision.relativeVelocity.magnitude * MetersPerSecondToMph;
            if (impactSpeedMph < lowCrashImpactThresholdMph)
            {
                return;
            }

            if (!IsHitFromConfiguredCollider(collision))
            {
                return;
            }

            var crashClip = GetCrashSoundForImpact(impactSpeedMph);
            if (crashClip == null)
            {
                return;
            }

            PlayOneShot(crashClip);
            _nextCrashSoundTime = Time.time + crashSoundCooldown;
        }

        private AudioClip GetCrashSoundForImpact(float impactSpeedMph)
        {
            if (impactSpeedMph >= highCrashImpactThresholdMph)
            {
                return GetFirstValidClip(highCrashSound, mediumCrashSound, lowCrashSound);
            }

            if (impactSpeedMph >= mediumCrashImpactThresholdMph)
            {
                return GetFirstValidClip(mediumCrashSound, lowCrashSound, highCrashSound);
            }

            return GetFirstValidClip(lowCrashSound, mediumCrashSound, highCrashSound);
        }

        private static AudioClip GetFirstValidClip(AudioClip first, AudioClip second, AudioClip third)
        {
            if (first != null)
            {
                return first;
            }

            if (second != null)
            {
                return second;
            }

            if (third != null)
            {
                return third;
            }

            return null;
        }

        private bool IsHitFromConfiguredCollider(Collision collision)
        {
            if (hitSoundCollider == null)
            {
                return true;
            }

            var targetTransform = hitSoundCollider.transform;
            for (var i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (IsMatchingCollider(contact.thisCollider, targetTransform) ||
                    IsMatchingCollider(contact.otherCollider, targetTransform))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMatchingCollider(Collider contactCollider, Transform targetTransform)
        {
            if (contactCollider == null || targetTransform == null)
            {
                return false;
            }

            var contactTransform = contactCollider.transform;
            return contactTransform == targetTransform ||
                   contactTransform.IsChildOf(targetTransform) ||
                   targetTransform.IsChildOf(contactTransform);
        }

        private void UpdateEngineLoop()
        {
            if (_playerController == null || _gearbox == null)
            {
                return;
            }

            var targetLoop = idleLoop;
            var canApplyGas = _playerController.GetIsGasPressed() && !_playerController.GetIsBraking() && _playerController.HasFuel();
            if (canApplyGas)
            {
                var gearType = _gearbox.GetGearType();
                if (gearType == 1)
                {
                    targetLoop = accelerateLoop != null ? accelerateLoop : idleLoop;
                }
                else if (gearType == -1)
                {
                    targetLoop = reverseLoop != null ? reverseLoop : idleLoop;
                }
            }

            SetEngineLoop(targetLoop);
        }

        private void SetEngineLoop(AudioClip clip)
        {
            if (engineAudioSource == null)
            {
                return;
            }

            if (clip == null)
            {
                engineAudioSource.Stop();
                _currentLoop = null;
                return;
            }

            if (_currentLoop == clip && engineAudioSource.isPlaying)
            {
                return;
            }

            engineAudioSource.clip = clip;
            engineAudioSource.loop = true;
            engineAudioSource.Play();
            _currentLoop = clip;
        }

        public void PlayGearShiftSuccess()
        {
            PlayOneShot(shiftGearSound);
        }

        public void PlayReverseFail()
        {
            PlayOneShot(reverseFailSound);
        }

        public void PlayGearShiftFail()
        {
            PlayOneShot(gearShiftFailSound);
        }

        public void PlayPickupSuccess()
        {
            PlayOneShot(pickupSound);
        }

        public void PlayDropoffSuccess()
        {
            PlayOneShot(dropoffSound);
        }

        public void PlayDeliveryError()
        {
            PlayOneShot(errorSound);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (sfxAudioSource == null || clip == null)
            {
                return;
            }

            sfxAudioSource.PlayOneShot(clip);
        }
    }
}
