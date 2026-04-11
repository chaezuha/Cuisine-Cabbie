using DeliveryMechanics;
using UnityEngine;

public class WaypointTextDepot : MonoBehaviour
{
    public Camera _camera;

    public PickupMechanics pickupMechanics;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pickupMechanics = GetComponentInParent<PickupMechanics>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.forward = _camera.transform.forward;
        var newPos = pickupMechanics.GetPositon();
        newPos.y += 5;
        transform.position = newPos;
    }
}
