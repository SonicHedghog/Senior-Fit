using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android;
#endif
using UnityEngine.SceneManagement;




public class SceneChange : MonoBehaviour
{
    BlazePoseRunner blazePoseRunner;
    private static bool isFlipped = false;
    private static int exercisenumber = 0;

    private static int req_fps = 0;
   // private static string new_url = "";

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        userdata data = SaveUserData.LoadUser();
        if (data == null)
            SceneManager.LoadScene("LoginMenu");
        
        #if UNITY_ANDROID
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (notificationIntentData != null)
        {
            Debug.Log("senior fit notification");
            //var id = notificationIntentData.Id;
           // var channel = notificationIntentData.IntentData;
            var notification = notificationIntentData.Notification;
            string link=notification.IntentData;

            //string link = "https://www.google.com/";
            //if (link.Length != 0)
            {
                Application.OpenURL(link);
                Debug.Log("senior fit url link" + link);
                 Debug.Log("senior fit link" + link);
            }

        }
        #endif

    }

    public static void OpenNewLink()
    {
       // string link = loginscript.GetLink()[0];
        //Debug.Log("senior fit link unity" + link);
        
        //Application.OpenURL(plink);

       /* var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (notificationIntentData != null)
        {
            Debug.Log("senior fit notification");
            //var id = notificationIntentData.Id;
           // var channel = notificationIntentData.IntentData;
            var notification = notificationIntentData.Notification;
            string data=notification.IntentData;

            //string link = "https://www.google.com/";
            //if (link.Length != 0)
            {
                Application.OpenURL(data);
                Debug.Log("senior fit url link" + data);
                 Debug.Log("senior fit link" + link);
            }

        }*/
        
        


    }

    public void Walk()
    {
        //SceneManager.LoadScene("Walk");
        SceneManager.LoadScene("Walk");
    }

    public void QuitTheGame()
    {
        Application.Quit();
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
        exercisenumber = 3;
        req_fps = 30;
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

        exercisenumber = 2;
        req_fps = 30;
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

    public void StartSeatedHamstring()
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



}

