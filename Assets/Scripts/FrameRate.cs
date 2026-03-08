using UnityEngine;

public class FrameRate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QualitySettings.vSyncCount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
