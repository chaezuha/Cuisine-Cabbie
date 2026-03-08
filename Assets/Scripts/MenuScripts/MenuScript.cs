using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Driving");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
