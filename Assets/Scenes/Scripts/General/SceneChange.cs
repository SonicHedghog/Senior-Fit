using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void StartSeatedMarch()
    {
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

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void StartShoulderTouch()
    {
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif
        if (webCamPermission)
        {
            SceneManager.LoadScene("BlazePoseShoulderTouch");
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
                SceneManager.LoadScene("BlazePoseShoulderTouch");
            }
        }

    }

    
}

