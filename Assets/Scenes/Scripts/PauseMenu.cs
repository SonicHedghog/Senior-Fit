using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI = null;
    private static bool isPaused = false;
    private static bool isFlipped = false;
    private static bool higherAccuracy = false;

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
        higherAccuracy = !higherAccuracy;
    }

     public void FlipCamera()
    {
        isFlipped = !isFlipped;
    }

    public void Reset()
    {
        SceneManager.LoadScene("WorkoutSpace");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public static bool GetIsFlipped()
    {
        return isFlipped;
    }

    public static bool GetAccuracyLevel()
    {
        return higherAccuracy;
    }
}
