using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI = null;
    private static bool isPaused = false;

    public static bool GetIsPaused()
    {
        return isPaused;
    }

    public void ActivateMenu()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void DeactivateMenu()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }
    
}
