using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_ANDROID

using Unity.Notifications.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

/*using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;*/

public class loginscript : MonoBehaviour
{
    // Start is called before the first frame update
    public Button loginButton;
    public InputField firstname;
    public InputField lastname;

    public InputField ContactNumber;
    public long contactno;
    public string fname;

    public string lname;

    public string version;

    public DateTime LoginTime;


    public string[] lines;

    public string[] messages;
    //public string[] url;
    NotificationList newnotification;

   // private static string new_url = "";
   private static List<string> new_url=new List<string>();
   

    //**********************************************************************

    async void Start()
    {
        //UnityInitializer.AttachToGameObject(this.gameObject);
        loginButton.onClick.AddListener(LoginButtonClick);
        newnotification = SaveData.LoadNotifications();
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            new_url.Add(noti.url);
        }
        //Debug.Log(newnotification.allnotifications[1].url);
        //DontDestroyOnLoad(this.gameObject);


    }


    /*public void LoadNotifications()
    {
        string[] paths = { Application.streamingAssetsPath, "Routines", "PushNotifications.csv" };

        
        if (Application.platform == RuntimePlatform.Android)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
            www.SendWebRequest();
            while (!www.isDone)
            {
            }
            lines = www.downloadHandler.text.Split('\n');
            Debug.Log(lines);


        }
        else
        {
            lines = File.ReadAllLines(Application.streamingAssetsPath + "/Routines/" +  "PushNotifications.csv");
            Debug.Log(lines[0]);
        }


       
    }*/

    int count=0;
    public void EventAlarmTest(int minutesOnTheHour, string firstname, string body,string link)
    {
        count++;
        Debug.Log("Notification called "+count);
       /*
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
        notification.FireTime = System.DateTime.Now.AddMinutes(minutesOnTheHour);
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
    */

        #if UNITY_ANDROID
        SetUpAndroidNotifications(minutesOnTheHour, firstname, body, link);
        #elif UNITY_IOS
        SetUpIOSNotifications(minutesOnTheHour, firstname, body, link);
        #endif
    }
    


    void LoginButtonClick()
    {
        contactno = long.Parse(ContactNumber.text);
        fname = firstname.text;
        lname = lastname.text;
        LoginTime=DateTime.Now;
        version=SaveData.IsNotificationScheduled();
        SaveUserData.SaveUser(this);
        SceneManager.LoadScene("MainMenu");
        int time=0;
        string body;
        
        Debug.Log("first time log in & notification : ");
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            time=6;
            //time=noti.interval/2;
            body = "Hi " + fname + " ! " + noti.message;
            Debug.Log("body " + body);
            string link = noti.url;
            //SceneChange.OpenNewLink(newnotification.allnotifications[0].url);
            EventAlarmTest(time, fname, body,link);
            Debug.Log("notification successful");
        }
        



    }

    public static List<string> GetLink()
    {
        return new_url;
    }

   
    public void loadUser()
    {
        userdata data = SaveUserData.LoadUser();

        Debug.Log("Saved user name : " + data.fname + " and number : " + data.contactno);
    }


    #if UNITY_ANDROID
    void SetUpAndroidNotifications(int minutesOnTheHour, string firstname, string body,string link)
    {
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
    }
    #elif UNITY_IOS
    void SetUpIOSNotifications(int minutesOnTheHour, string firstname, string body,string link)
    {
        RequestAuthorization();

        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(minutesOnTheHour, 0, 0),
            Repeats = false
        };

        var notification = new iOSNotification(){
            Title = "Senior Fit",

            Body = body,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);

    }

    IEnumerator RequestAuthorization()
    {
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log(res);
        }
    }
    #endif
    
}