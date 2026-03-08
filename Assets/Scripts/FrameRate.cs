using UnityEngine;

public class FrameRate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        
        /*
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        */
    }
}
