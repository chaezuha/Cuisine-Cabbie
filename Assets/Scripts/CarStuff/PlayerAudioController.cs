using UnityEngine;
using UnityEngine.UIElements;

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
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip shiftGearSound;
        [SerializeField] private AudioClip reverseFailSound;
        [SerializeField] private AudioClip gearShiftFailSound;

        [Header("Delivery One-Shots")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropoffSound;
        [SerializeField] private AudioClip errorSound;

        [Header("Tuning Specifics")]
        [SerializeField] private float collisionImpactThreshold = 1f;
        [SerializeField] private Collider hitSoundCollider;

        private AudioClip _currentLoop;
        private PlayerController _playerController;
        private Gearbox _gearbox;

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

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude < collisionImpactThreshold)
            {
                return;
            }

            if (!IsHitFromConfiguredCollider(collision))
            {
                return;
            }

            PlayOneShot(hitSound);
        }

        private bool IsHitFromConfiguredCollider(Collision collision)
        {
            if (hitSoundCollider == null)
            {
                return true;
            }

            for (var i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (contact.thisCollider == hitSoundCollider || contact.otherCollider == hitSoundCollider)
                {
                    return true;
                }
            }

            return false;
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
