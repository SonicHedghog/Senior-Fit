using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI = null;
    [SerializeField] private AudioSource music = null;
    private static bool isPaused = false;
    private static bool isFlipped = false;
    private static bool higherAccuracy = false;

    void Start()
    {
        if(SaveData.LoadSoundState()==null)
        {
            SaveData.UpdateSoundData(music.mute);
        }
        else
        {
            SOundState data = SaveData.LoadSoundState();
            music.mute=data.soundState;
        }
    }

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

    public void ToggleMute()
    {
        music.mute = !music.mute;
        SaveData.UpdateSoundData(music.mute);
        
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
