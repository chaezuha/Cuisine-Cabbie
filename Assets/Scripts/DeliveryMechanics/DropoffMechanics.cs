using TMPro;
using UnityEngine;

namespace DeliveryMechanics
{
    public class DropoffMechanics : MonoBehaviour
    {
        [SerializeField] private WaypointBrain waypointBrain;
        [SerializeField] private string dropOffId;
        [Header("Minimap Icon")]
        [SerializeField] private Sprite minimapIconSprite;
        [SerializeField] private float minimapIconSize = 30f;
        [SerializeField] private Color minimapIconTint = Color.white;

        [Header("Overrides")]
        [SerializeField] private int circleOverride = -1;
        [SerializeField] private float fuelOverride = -1f;

        private bool _waypointIsActive;
        private float _distanceFromPlayer;
        private float _distanceFromDepot;
        private TMP_Text _text;
        private Vector3 _dropOffPos;
        private Vector3 _dropOffRot;
        private ScreenWaypointIndicator _indicator;
        private MinimapIconIndicator _minimapIcon;

        public Vector3 GetPositon()
        {
            return _dropOffPos;
        }

        public Vector3 GetRotation()
        {
            return _dropOffRot;
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
        
        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(dropOffId))
            {
                dropOffId = gameObject.name;
            }
        }

        private void Start()
        {
            _dropOffRot = transform.eulerAngles;
            _dropOffPos = transform.position;
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

        public int GetCircleOverride()
        {
            return circleOverride;
        }

        public float GetFuelOverride()
        {
            return fuelOverride;
        }
        
        private void Update()
        {
            if (_indicator == null) return;

            if (_waypointIsActive)
            {
                _distanceFromPlayer = waypointBrain.CalculateDistance(transform.position);
                _indicator.SetWorldTarget(_dropOffPos);
                if (_minimapIcon != null)
                    _minimapIcon.SetWorldTarget(_dropOffPos);
                _text.color = Color.white;
                var roundedDistance = Mathf.RoundToInt(_distanceFromPlayer * 3.281f);
                _text.text = dropOffId + '\n' + (roundedDistance) + " feet away";
                _text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.35f);
                _text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
            }
        }
    }
}

