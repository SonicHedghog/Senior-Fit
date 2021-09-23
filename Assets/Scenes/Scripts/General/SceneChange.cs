using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;



public class SceneChange : MonoBehaviour
{
    BlazePoseRunner blazePoseRunner;
    private static bool isFlipped = false;
    private static int exercisenumber=0;

    private static int req_fps=0;

    void Start()
    {

        userdata data = SaveUserData.LoadUser();
        if (data == null)
            SceneManager.LoadScene("LoginMenu");


    }

    public void Walk()
    {
        SceneManager.LoadScene("Walk");
    }


    public void StartSeatedMarch()
    {
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        //userdata data = SaveUserData.LoadUser();

        

            exercisenumber = 1;
            req_fps = 25;
            if (webCamPermission)
            {
                SceneManager.LoadScene("WorkoutSpace");
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
                    SceneManager.LoadScene("WorkoutSpace");
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
    public static int GetFPS()
    {
        return req_fps;
    }


    public void StartShoulderTouch()
    {
        exercisenumber=3;
        req_fps=30;
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif
        if (webCamPermission)
        {
            SceneManager.LoadScene("WorkoutSpace");
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
                SceneManager.LoadScene("WorkoutSpace");
            }
        }

    }

    public void StartSitToStand()
    {
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        exercisenumber=2;
        req_fps=30;
        if (webCamPermission)
        {
            SceneManager.LoadScene("WorkoutSpace");
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
                SceneManager.LoadScene("WorkoutSpace");
                //blazePoseRunner.filename="Shoulder_touch";
            }
        }

    }


    
}

