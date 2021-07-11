using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;



public class SceneChange : MonoBehaviour
{
    BlazePoseRunner blazePoseRunner;
    private static bool isFlipped = false;
    private static int exercisenumber=0;
    public void StartSeatedMarch()
    {
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        exercisenumber=1;
        if (webCamPermission)
        {
            SceneManager.LoadScene("BlazePoseTest");
        }

        else
        {
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);

            #if PLATFORM_ANDROID
                Permission.RequestUserPermission(Permission.Camera);
                webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            #endif

            if (webCamPermission)
            {
                SceneManager.LoadScene("BlazePoseTest");
                //blazePoseRunner.filename="Shoulder_touch";
            }
        }

    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void FlipCamera()
    {
        isFlipped = !isFlipped;
    }

    public static bool GetIsFlipped()
    {
        return isFlipped;
    }

    public static int GetExerciseNumber()
    {
        return exercisenumber;
    }

    public void StartShoulderTouch()
    {
        exercisenumber=2;
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif
        if (webCamPermission)
        {
            SceneManager.LoadScene("BlazePoseTest");
        }

        else
        {
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);

            #if PLATFORM_ANDROID
                Permission.RequestUserPermission(Permission.Camera);
                webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            #endif

            if (webCamPermission)
            {
                SceneManager.LoadScene("BlazePoseTest");
            }
        }

    }

    
}

