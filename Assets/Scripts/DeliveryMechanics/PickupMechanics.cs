using TMPro;
using UnityEngine;

namespace DeliveryMechanics
{
    public class PickupMechanics : MonoBehaviour
    {
        [SerializeField] private WaypointBrain waypointBrain;
        [Header("Minimap Icon")]
        [SerializeField] private Sprite minimapIconSprite;
        [SerializeField] private float minimapIconSize = 30f;
        [SerializeField] private Color minimapIconTint = Color.white;
        private bool _waypointIsActive;
        private float _distanceFromPlayer;
        private TMP_Text _text;
        private Vector3 _pickUpPos;
        private Vector3 _pickUpRot;
        private GameObject _visualEffects;
        private ScreenWaypointIndicator _indicator;
        private MinimapIconIndicator _minimapIcon;
        
        public Vector3 GetPositon()
        {
            return _pickUpPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DeliveryBrain>(out var brain))
            {
                brain.TryPickup();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<DeliveryBrain>(out var brain))
            {
                brain.ClearMessage();
            }
        }
        
        public void SetWaypointActive(bool active)
        {
            _waypointIsActive = active;
            gameObject.SetActive(active);
            if (_indicator != null)
                _indicator.gameObject.SetActive(active);
            if (_minimapIcon != null)
                _minimapIcon.gameObject.SetActive(active);
        }

        public Vector3 GetRotation()
        {
            return _pickUpRot;
        }

        private void Start()
        {
            _pickUpPos = transform.position;
            _pickUpRot = transform.rotation.eulerAngles;
            _waypointIsActive = false;
            waypointBrain = FindObjectOfType<WaypointBrain>();

            _indicator = ScreenWaypointIndicator.Create(Camera.main);
            if (_indicator != null)
            {
                _text = _indicator.Text;
                _indicator.gameObject.SetActive(false);
            }

            var minimapCam = GameObject.Find("Mini Map Camera");
            if (minimapCam != null)
            {
                _minimapIcon = MinimapIconIndicator.Create(minimapCam.GetComponent<Camera>(), minimapIconSprite, minimapIconSize, minimapIconTint);
                if (_minimapIcon != null)
                    _minimapIcon.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            if (_indicator == null) return;

            if (PauseMenu.IsPaused)
            {
                _indicator.gameObject.SetActive(false);
                return;
            }
            else if (_waypointIsActive)
            {
                _indicator.gameObject.SetActive(true);
            }

            if (_waypointIsActive)
            {
                _distanceFromPlayer = waypointBrain.CalculateDistance(transform.position);
                _indicator.SetWorldTarget(_pickUpPos);
                if (_minimapIcon != null)
                    _minimapIcon.SetWorldTarget(_pickUpPos);
                _text.color = Color.white;
                _text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.35f);
                _text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                var roundedDistance = Mathf.RoundToInt(_distanceFromPlayer * 3.281f);
                _text.text = "PICK UP PACKAGES HERE" + '\n' + (roundedDistance) + " feet away";
            }
        }
    }
}