using UnityEngine;
using System;
using UnityEngine.UI;

#if UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    BlazePoseRunner blazePoseRunner;
    private static bool isFlipped = false;
    private static int exercisenumber = 0;
    public static int tutorialNumber = 0;

    NotificationList newnotification;

    public Button cameraButton;
    public Button noCameraButton;
    

    public static bool cameraAllow = false;
    public static bool camPermission = false;

    private static int req_fps = 0;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        UserData data = SaveUserData.LoadUser();
        if (data == null)
        {
            SceneManager.LoadScene("LoginMenu");

        } 

        Debug.Log("Saved user info : "+data.fname+data.lname+data.contactno+data.LoginTime);

        #if UNITY_ANDROID
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (notificationIntentData != null)
        {
            Debug.Log("senior fit notification");
            var notification = notificationIntentData.Notification;
            string link = notification.IntentData;

            Application.OpenURL(link);
            Debug.Log("senior fit url link" + link);
            Debug.Log("senior fit link" + link);

        }
        string s = SaveData.IsNotificationScheduled();
        Debug.Log("noti status found " + s);
        Debug.Log("login version saved: " + data.version);
        if(s != data.version)
        {
            ScheduleAndroidNotifications();
            SaveUserData.UpdateUserVersion(s);
        }
        #endif

        #if UNITY_IOS
        var notification = iOSNotificationCenter.GetLastRespondedNotification();
        if (notification != null)
        {
            Debug.Log("senior fit notification");

            string link = notification.Data;

            if (link.Length != 0)
            {
                Application.OpenURL(link);
                Debug.Log("senior fit url link" + link);
                Debug.Log("senior fit link" + link);
            }

        }
        string s = SaveData.IsNotificationScheduled();
        Debug.Log("noti status found " + s);
        Debug.Log("login version saved: "+ data.version);
        if(s != data.version)
        {
            ScheduleiOSNotifications();
            SaveUserData.UpdateUserVersion(s);
        }
        #endif
    }
    
    #if UNITY_ANDROID
    public void ScheduleAndroidNotifications()
    {
        Debug.Log("New Version Available");
       
        UserData data = SaveUserData.LoadUser();
        string fname = data.fname;
      
        DateTime oldDate = data.LoginTime;
        DateTime current = System.DateTime.Now;


        Debug.Log("First Login time : "+oldDate.ToString());
      
        int time = 0;
        string body;
        newnotification = SaveData.LoadNotifications();
        #if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        #endif
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            time = noti.interval;
            DateTime scheduled=oldDate.AddDays(time);
            if(DateTime.Compare(scheduled, current)>=0)
            {
                body = "Hi " + fname + " ! " + noti.message;
                Debug.Log("body " + body);
                string link = noti.url;
                EventAlarmTest(time, fname, body,link);
                Debug.Log("notification scheduled for: "+noti.interval);
            }
        }
        Debug.Log(fname);
        Debug.Log("App Updated : notification successful");
    }
#endif

#if UNITY_IOS
    public void ScheduleiOSNotifications()
    {
        Debug.Log("New Version Available");
       
        UserData data = SaveUserData.LoadUser();
        string fname = data.fname;
      
        DateTime oldDate = data.LoginTime;
        DateTime current = System.DateTime.Now;
      
        int time = 0;
        string body;
        newnotification = SaveData.LoadNotifications();
        iOSNotificationCenter.GetLastRespondedNotification();
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            time = noti.interval;
            DateTime scheduled = oldDate.AddHours(time);

            if(DateTime.Compare(scheduled, current)>=0)
            {
                body = "Hi " + fname + " ! " + noti.message;
                Debug.Log("body " + body);
                string link = noti.url;
                EventAlarmTest(time, fname, body,link);
                Debug.Log("notification scheduled for: "+noti.interval);
            }
        }
        Debug.Log(fname);
        Debug.Log("App Updated : notification successful");
    }
    #endif

    public void OpenFB()
    {
        Application.OpenURL("https://www.facebook.com/groups/seniorfitstudyphase2r1");
    }

    void PermissionButtonClick()
    {
        cameraAllow = true;
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
        webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        if (!webCamPermission)
        {
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);

            #if PLATFORM_ANDROID
            Permission.RequestUserPermission(Permission.Camera);
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            #endif
        }
    }
    
    void noPermissionButtonClick()
    {
        cameraAllow = false;
    }

    public void Walk()
    {
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

    public void DisableCamera()
    {
        exercisenumber = GetExerciseNumber();
        SaveData.SaveCameraState(0);
        SceneManager.LoadScene("NoCameraWorkout");
    }

    public void StartShoulderTouch()
    {
        exercisenumber = 3;
        req_fps = 30;
        LoadWorkoutScene();
    }

    public void ChairSitToStand()
    {
        exercisenumber = 4;
        req_fps = 25;
        SceneManager.LoadScene("NoCameraWorkout");
    }

    public void MarchinginPlace()
    {
        exercisenumber = 5;
        req_fps = 25;
        LoadWorkoutScene();
    }

    public void SeatedHamstringStretch()
    {
        exercisenumber = 6;
        req_fps = 25;
    }

    public void SingleLegStance()
    {
        exercisenumber = 2;
        req_fps = 25;
        LoadWorkoutScene();
    }

    public void RockTheBoat()
    {
        exercisenumber = 7;
        req_fps = 25;
        SceneManager.LoadScene("NoCameraWorkout");
    }

    public void WallPushUp()
    {
        exercisenumber = 12;
        req_fps = 25;
        SceneManager.LoadScene("NoCameraWorkout");
    }
    public void LegCurl()
    {
        exercisenumber = 10;
        req_fps = 25;
        SceneManager.LoadScene("NoCameraWorkout");
    }
    public void CalfStretch()
    {
        exercisenumber = 9;
        req_fps = 25;
    }
    public void ThighStretch()
    {
        exercisenumber = 11;
        req_fps = 25;
    }
    public void ShoulderStretch()
    {
        exercisenumber = 8;
        req_fps = 25;
        LoadWorkoutScene();
    }
    public void SideStepping()
    {
        exercisenumber = 13;
        req_fps = 25;
        SceneManager.LoadScene("NoCameraWorkout");
    }

    public void HeelToToe()
    {
        exercisenumber = 14;
        req_fps = 25;
        SceneManager.LoadScene("NoCameraWorkout");
    }

    public void LoadWorkoutScene()
    {
       
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        int CameraState = SaveData.LoadCameraData();
        Debug.Log("Camera State:" + CameraState.ToString());
        
        if (webCamPermission && CameraState == 1)
        {
            SceneManager.LoadScene("WorkoutSpace");
        }

        else
        {
            SceneManager.LoadScene("NoCameraWorkout");
        }
    }
    
    public void ShoulderTouchTutorial()
    {
        tutorialNumber = 1;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void ChairSitTutorial()
    {
        tutorialNumber = 2;

        SceneManager.LoadScene("VideoTutorial");
    }

    public void SingleLegStanceTutorial()
    {
        tutorialNumber = 3;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void SeatedHamstringTutorial()
    {
        tutorialNumber = 4;

        SceneManager.LoadScene("VideoTutorial");
    }
     public void MarchingTutorial()
    {
        tutorialNumber = 5;

        SceneManager.LoadScene("VideoTutorial");
    }
     public void HeeltoToeTutorial()
    {
        tutorialNumber = 6;

        SceneManager.LoadScene("VideoTutorial");
    }
     public void RocktheBoatTutorial()
    {
        tutorialNumber = 7;

        SceneManager.LoadScene("VideoTutorial");
    }
     public void ShoulderStretchTutorial()
    {
        tutorialNumber = 8;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void SideSteppingTutorial()
    {
        tutorialNumber = 9;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void StandingCalfStretchTutorial()
    {
        tutorialNumber = 10;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void StandingLegCurlTutorial()
    {
        tutorialNumber = 11;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void StandingThighStretchTutorial()
    {
        tutorialNumber = 12;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void WalkingInstructions()
    {
        tutorialNumber = 13;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void WalkingwithArmSwingsTutorial()
    {
        tutorialNumber = 14;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void WalkingwithHighKneesTutorial()
    {
        tutorialNumber = 15;

        SceneManager.LoadScene("VideoTutorial");
    }
    public void WallPushUpsTutorial()
    {
        tutorialNumber = 16;

        SceneManager.LoadScene("VideoTutorial");
    }

    int count = 0;
  
    public void EventAlarmTest(int minutesOnTheHour, string firstname, string body,string link)
    {
        count++;
        
        UserData data = SaveUserData.LoadUser();
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
        notification.FireTime = data.LoginTime.AddDays(minutesOnTheHour);
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