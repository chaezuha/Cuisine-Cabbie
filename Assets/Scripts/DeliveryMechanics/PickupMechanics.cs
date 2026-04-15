using TMPro;
using UnityEngine;

namespace DeliveryMechanics
{
    public class PickupMechanics : MonoBehaviour
    {
        [SerializeField] private WaypointBrain waypointBrain;
        private bool _waypointIsActive;
        private float _distanceFromPlayer;
        private TMP_Text _text;
        private Vector3 _pickUpPos;
        private Vector3 _pickUpRot;
        private GameObject _visualEffects;
        private ScreenWaypointIndicator _indicator;
        
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

            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            if (_indicator == null) return;

            if (_waypointIsActive)
            {
                _distanceFromPlayer = waypointBrain.CalculateDistance(transform.position);
                _indicator.SetWorldTarget(_pickUpPos);
                _text.color = Color.white;
                _text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.35f);
                _text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                var roundedDistance = Mathf.RoundToInt(_distanceFromPlayer * 3.281f);
                _text.text = "PICK UP PACKAGES HERE" + '\n' + (roundedDistance) + " feet away";
            }
        }
    }
}