using UnityEngine;

namespace CarStuff
{
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioSource driftAudioSource;
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource lowFuelMusicAudioSource;

        [Header("Music")]
        [SerializeField] private AudioClip gameMusicIntro;
        [SerializeField] private AudioClip gameMusicLoop;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

        [Header("Low Fuel Music")]
        [SerializeField] private AudioClip lowFuelMusicLoop;
        [SerializeField] [Range(0f, 1f)] private float lowFuelMusicVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float lowFuelThreshold = 0.25f;
        [SerializeField] private float musicFadeSpeed = 1f;

        [Header("Engine Loops")]
        [SerializeField] private AudioClip idleLoop;
        [SerializeField] private AudioClip accelerateLoop;
        [SerializeField] private AudioClip reverseLoop;

        [Header("Driving One-Shots")]
        [SerializeField] private AudioClip lowCrashSound;
        [SerializeField] private AudioClip mediumCrashSound;
        [SerializeField] private AudioClip highCrashSound;
        [SerializeField] private AudioClip shiftGearUpSound;
        [SerializeField] private AudioClip shiftGearDownSound;
        [SerializeField] private AudioClip reverseFailSound;
        [SerializeField] private AudioClip gearShiftFailSound;

        [Header("Drift")]
        [SerializeField] private AudioClip driftLoop;

        [Header("Delivery One-Shots")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropoffSound;
        [SerializeField] private AudioClip errorSound;

        [Header("Volume")]
        [SerializeField] [Range(0f, 1f)] private float engineLoopVolume = 1f;

        [Header("Tuning Specifics")]
        [SerializeField] private float crashSoundCooldown = 0.5f;

        [SerializeField] private Collider hitSoundCollider;

        private const float MetersPerSecondToMph = 2.237f;
        private AudioClip _currentLoop;
        private PlayerController _playerController;
        private Gearbox _gearbox;
        private float _nextCrashSoundTime;
        private bool _isPaused;
        private bool _musicStarted;
        private AudioSource _loopAudioSource;

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

            if (!PauseMenu.IsPaused)
            {
                StartMusic();
            }
        }

        private void Update()
        {
            UpdateEngineLoop();
            ApplyEngineLoopVolume();
            UpdateLowFuelMusic();
        }

        private void ApplyEngineLoopVolume()
        {
            if (engineAudioSource != null) engineAudioSource.volume = engineLoopVolume;
        }

        public void HandleCollision(Collision collision)
        {
            if (Time.time < _nextCrashSoundTime)
            {
                return;
            }

            var impactSpeedMph = collision.relativeVelocity.magnitude * MetersPerSecondToMph;
            if (_playerController == null || impactSpeedMph < _playerController.GetLowCrashThreshold())
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
            if (impactSpeedMph >= _playerController.GetHighCrashThreshold())
            {
                return GetFirstValidClip(highCrashSound, mediumCrashSound, lowCrashSound);
            }

            if (impactSpeedMph >= _playerController.GetMediumCrashThreshold())
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

        public void StartMusic()
        {
            if (_musicStarted || musicAudioSource == null)
            {
                return;
            }

            _musicStarted = true;
            musicAudioSource.volume = musicVolume;

            if (gameMusicIntro != null && gameMusicLoop != null)
            {
                _loopAudioSource = gameObject.AddComponent<AudioSource>();
                _loopAudioSource.clip = gameMusicLoop;
                _loopAudioSource.loop = true;
                _loopAudioSource.volume = musicVolume;
                _loopAudioSource.playOnAwake = false;

                musicAudioSource.clip = gameMusicIntro;
                musicAudioSource.loop = false;
                musicAudioSource.Play();

                double introDuration = (double)gameMusicIntro.samples / gameMusicIntro.frequency;
                _loopAudioSource.PlayScheduled(AudioSettings.dspTime + introDuration);
            }
            else if (gameMusicIntro != null)
            {
                musicAudioSource.clip = gameMusicIntro;
                musicAudioSource.loop = false;
                musicAudioSource.Play();
            }
            else if (gameMusicLoop != null)
            {
                musicAudioSource.clip = gameMusicLoop;
                musicAudioSource.loop = true;
                musicAudioSource.Play();
            }

            if (lowFuelMusicAudioSource != null && lowFuelMusicLoop != null)
            {
                lowFuelMusicAudioSource.clip = lowFuelMusicLoop;
                lowFuelMusicAudioSource.loop = true;
                lowFuelMusicAudioSource.volume = 0f;
                lowFuelMusicAudioSource.Play();
            }
        }

        private void UpdateLowFuelMusic()
        {
            if (_playerController == null || lowFuelMusicAudioSource == null || lowFuelMusicLoop == null)
            {
                return;
            }

            float fuelPercent = _playerController.GetFuel() / _playerController.GetMaxFuel();
            bool isLowFuel = fuelPercent <= lowFuelThreshold && fuelPercent > 0f;
            float fadeStep = musicFadeSpeed * Time.deltaTime;

            if (isLowFuel)
            {
                lowFuelMusicAudioSource.volume = Mathf.MoveTowards(lowFuelMusicAudioSource.volume, lowFuelMusicVolume, fadeStep);
                musicAudioSource.volume = Mathf.MoveTowards(musicAudioSource.volume, 0f, fadeStep);
                if (_loopAudioSource != null)
                {
                    _loopAudioSource.volume = Mathf.MoveTowards(_loopAudioSource.volume, 0f, fadeStep);
                }
            }
            else
            {
                lowFuelMusicAudioSource.volume = Mathf.MoveTowards(lowFuelMusicAudioSource.volume, 0f, fadeStep);
                musicAudioSource.volume = Mathf.MoveTowards(musicAudioSource.volume, musicVolume, fadeStep);
                if (_loopAudioSource != null)
                {
                    _loopAudioSource.volume = Mathf.MoveTowards(_loopAudioSource.volume, musicVolume, fadeStep);
                }
            }
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

        public void PlayGearShiftSuccess(int shiftDirection)
        {
            PlayOneShot(shiftDirection > 0 ? shiftGearUpSound : shiftGearDownSound);
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

        public void SetDrifting(bool isDrifting)
        {
            if (driftAudioSource == null || driftLoop == null)
            {
                return;
            }

            if (isDrifting && !driftAudioSource.isPlaying)
            {
                driftAudioSource.clip = driftLoop;
                driftAudioSource.loop = true;
                driftAudioSource.Play();
            }
            else if (!isDrifting && driftAudioSource.isPlaying)
            {
                driftAudioSource.Stop();
            }
        }

        public void PauseAllAudio()
        {
            _isPaused = true;
            if (musicAudioSource != null) musicAudioSource.Pause();
            if (_loopAudioSource != null) _loopAudioSource.Pause();
            if (lowFuelMusicAudioSource != null) lowFuelMusicAudioSource.Pause();
            if (engineAudioSource != null) engineAudioSource.Pause();
            if (sfxAudioSource != null) sfxAudioSource.Pause();
            if (driftAudioSource != null) driftAudioSource.Pause();
        }

        public void ResumeAllAudio()
        {
            _isPaused = false;
            if (musicAudioSource != null) musicAudioSource.UnPause();
            if (_loopAudioSource != null) _loopAudioSource.UnPause();
            if (lowFuelMusicAudioSource != null) lowFuelMusicAudioSource.UnPause();
            if (engineAudioSource != null) engineAudioSource.UnPause();
            if (sfxAudioSource != null) sfxAudioSource.UnPause();
            if (driftAudioSource != null) driftAudioSource.UnPause();
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
