using DeliveryMechanics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void PlayGame()
    {
        if (TelemetrySession.Instance == null)
        {
            var go = new GameObject("TelemetrySession");
            go.AddComponent<TelemetrySession>();
        }

        SceneManager.LoadScene("Driving");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
