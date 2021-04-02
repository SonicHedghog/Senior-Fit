using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI = null;
    private static bool isPaused = false;
    private static bool isFlipped = false;

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
    
    public void SwitchMLModel()
    {
        isFlipped = !isFlipped;
    }
    public static bool GetIsFlipped()
    {
        return isFlipped;
    }
    
}
