using UnityEngine;
using System;
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
    public static int tutorialNumber=0;

    NotificationList newnotification;

    

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
        /*string s=SaveData.IsNotificationScheduled();
        Debug.Log("noti status found "+s);
        if(s=="scheduled")
        {
            ScheduleNotifications();

        }*/
        
            
        
        
        #endif

    }

   /* public void ScheduleNotifications()
    {
        Debug.Log("inside schedule notifications");
       
       
        userdata data = SaveUserData.LoadUser();
        string fname = data.fname;
      
        DateTime oldDate = data.LoginTime;
        DateTime current=System.DateTime.Now;
        if(DateTime.Compare(oldDate, current)<0)
        {
            
      
        int time=0;
        string body;
        //DateTime oldDate = data.LoginTime;
        //DateTime current=System.DateTime.Now;
        newnotification = SaveData.LoadNotifications();
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            time=noti.interval+10;
            DateTime scheduled=oldDate.AddMinutes(time);

            if(DateTime.Compare(scheduled, current)>=0)
            {
                
            body = "Hi " + fname + " ! " + noti.message;
            Debug.Log("body " + body);
            string link = noti.url;
            EventAlarmTest(time, fname, body,link);
            

            }


        }
        Debug.Log("App Updated : notification successful");
       
        }
    }*/

    public void OpenFB()
    {
        Application.OpenURL("https://www.facebook.com/groups/seniorfitstudyphase2r1");
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
     public static int GetTutorialNumber()
    {
        return tutorialNumber;
    }

    

    /*public void StartSeatedMarch()
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
    }*/



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

    public void ChairSitToStand()
    {
       
        exercisenumber = 4;
        req_fps = 30;
        LoadWorkoutScene();

    }

    public void MarchinginPlace()
    {
        exercisenumber=5;
        req_fps=30;
        LoadWorkoutScene();
    }

    public void LoadWorkoutScene()
    {
        
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
                //blazePoseRunner.filename="Shoulder_touch";
            }
        }
    }

    public void SingleLegStance()
    {
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
#if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
#endif

        //userdata data = SaveUserData.LoadUser();



        exercisenumber = 2;
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


    
    public void ShoulderTouchTutorial()
    {
        tutorialNumber=1;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void ChairSitTutorial()
    {
        tutorialNumber=2;

        SceneManager.LoadScene("VideoTutorial");
    }

     public void SingleLegStanceTutorial()
    {
        tutorialNumber=3;

        SceneManager.LoadScene("VideoTutorial");
    }
     public void SeatedHamstringTutorial()
    {
        tutorialNumber=4;

        SceneManager.LoadScene("VideoTutorial");
    }
     public void MarchingTutorial()
    {
        tutorialNumber=5;

        SceneManager.LoadScene("VideoTutorial");
    }

    int count=0;
    public void EventAlarmTest(int minutesOnTheHour, string firstname, string body,string link)
    {

        count++;
        Debug.Log("Notification called "+count);
       
        #if UNITY_ANDROID
        var c1 = new AndroidNotificationChannel()
        {
            Id = "notification_id",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Reminder notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c1);

        var notification = new AndroidNotification();
        notification.Title = "Senior Fit";
        //string body = "Hi " + firstname + " ! " + lines[1];
        notification.Text = body;
        notification.FireTime = System.DateTime.Now.AddHours(minutesOnTheHour);
        notification.ShouldAutoCancel = true;
        notification.ShowTimestamp = true;
        notification.IntentData = link;
        notification.Style = NotificationStyle.BigTextStyle;

        var notification_id = AndroidNotificationCenter.SendNotification(notification, "notification_id");

        Debug.Log($"notification status - {AndroidNotificationCenter.CheckScheduledNotificationStatus(notification_id)}");
        var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(notification_id);
        if (notificationStatus == NotificationStatus.Delivered)
        {
            // Remove the previously shown notification from the status bar.
            AndroidNotificationCenter.CancelNotification(notification_id);
        }



    #endif

    }





}

