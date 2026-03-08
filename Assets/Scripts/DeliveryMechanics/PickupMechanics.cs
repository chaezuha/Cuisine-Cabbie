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
        }

        private void Start()
        {
            _text = GetComponentInChildren<TMP_Text>();
            _pickUpPos = transform.position;
            _waypointIsActive = false;
            waypointBrain = FindObjectOfType<WaypointBrain>();
        }
        
        private void Update()
        {
            if (_waypointIsActive)
            {
                Debug.Log("working");
                _distanceFromPlayer = waypointBrain.CalculateDistance(transform.position);
                _text.gameObject.SetActive(true);
                _text.color = Color.cyan;
                _text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
                _text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                var roundedDistance = Mathf.RoundToInt(_distanceFromPlayer * 3.281f);
                _text.text = "RETURN TO DEPOT" + '\n' + (roundedDistance) + " feet away";
            }
            else
            {
                Debug.Log("nada");
                _text.gameObject.SetActive(false);
            }
        }
    }
}