using System;
using UnityEngine;

public class WaypointIcon : MonoBehaviour
{
    public Camera miniMapCamera;

    private void Start()
    {
        if (miniMapCamera == null)
        {
            miniMapCamera = GameObject.Find("Mini Map Camera").GetComponent<Camera>();
        }
    }

    void LateUpdate()
    {
        transform.rotation = miniMapCamera.transform.rotation;
    }
}
