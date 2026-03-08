using TMPro;
using UnityEngine;

namespace DeliveryMechanics
{
    public class DropoffMechanics : MonoBehaviour
    {
        [SerializeField] private WaypointBrain waypointBrain;
        [SerializeField] private string dropOffId;

        private bool _waypointIsActive;
        private float _distanceFromPlayer;
        private TMP_Text _text;
        private Vector3 _dropOffPos;

        public Vector3 GetPositon()
        {
            return _dropOffPos;
        }

        public void SetWaypointActive(bool active)
        {
            _waypointIsActive = active;
        }
        
        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(dropOffId))
            {
                dropOffId = gameObject.name;
            }
        }

        private void Start()
        {
            _text = GetComponentInChildren<TMP_Text>();
            _dropOffPos = transform.position;
            _waypointIsActive = false;
            waypointBrain = FindObjectOfType<WaypointBrain>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DeliveryBrain>(out var brain))
            {
                brain.TryDropoff(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<DeliveryBrain>(out var brain))
            {
                brain.ClearMessage();
            }
        }

        public string GetDropOffId()
        {
            return dropOffId;
        }
        
        private void Update()
        {
            if (_waypointIsActive)
            {
                _distanceFromPlayer = waypointBrain.CalculateDistance(transform.position);
                _text.gameObject.SetActive(true);
                _text.color = Color.cyan;
                var roundedDistance = Mathf.RoundToInt(_distanceFromPlayer * 3.281f);
                _text.text = dropOffId + '\n' + (roundedDistance) + " feet away";
                _text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
                _text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
            }
            else
            {
                _text.gameObject.SetActive(false);
            }
        }
    }
}

