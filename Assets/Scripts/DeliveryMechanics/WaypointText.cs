using UnityEngine;

namespace DeliveryMechanics
{
    public class WaypointText : MonoBehaviour
    {
        public Camera playerCamera;
        public DropoffMechanics dropoffMechanics;
        
        void Start()
        {
            dropoffMechanics = GetComponentInParent<DropoffMechanics>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        
        }
        
        void LateUpdate()
        {
            transform.forward = playerCamera.transform.forward;
            var newPos = dropoffMechanics.GetPositon();
            newPos.y += 5;
            transform.position = newPos;
            
        }
    }
}
