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

public class LoginSetUp : MonoBehaviour
{
    public Button loginButton;
    public Button okButton;
    public InputField FirstNameField;
    public InputField LastNameField;
    public InputField ContactNumberField;
    public Text DisclaimerText;
    public long contactno;
    public string fname;
    public string lname;
    public string version;
    public DateTime LoginTime;
    public string[] lines;
    public string[] messages;
    NotificationList newnotification;
    private static List<string> new_url = new List<string>();
   
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        loginButton.onClick.AddListener(LoginButtonClick);
        okButton.onClick.AddListener(LoadStartMenu);
        newnotification = SaveData.LoadNotifications();
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            new_url.Add(noti.url);
        }
    }

    void Update()
    {
        loginButton.interactable = FirstNameField.text.Length > 0 && LastNameField.text.Length > 0 && ContactNumberField.text.Length >= 10;
    }

    int count = 0;
    public void EventAlarmTest(int minutesOnTheHour, string firstname, string body,string link)
    {
        count++;
        Debug.Log("Notification called " + count);

        #if UNITY_ANDROID
        SetUpAndroidNotifications(minutesOnTheHour, firstname, body, link);
        #elif UNITY_IOS
        SetUpIOSNotifications(minutesOnTheHour, firstname, body, link);
        #endif
    }
    
    void LoginButtonClick()
    {
        contactno = long.Parse(ContactNumberField.text);
        fname = FirstNameField.text.Trim();
        lname = LastNameField.text.Trim();
        LoginTime = DateTime.Now;
        version = SaveData.IsNotificationScheduled();
        SaveUserData.SaveUser(this);
        
        int time = 0;
        string body;
        
        Debug.Log("First Time Login & Notification: ");
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            time = noti.interval;
            body = "Hi " + fname + " ! " + noti.message;
            Debug.Log("body " + body);
            string link = noti.url;
            EventAlarmTest(time, fname, body,link);
            Debug.Log("Notification Successful!");
        }
    }

    public void LoadStartMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Concent Button Clicked");
    }

    public static List<string> GetLink()
    {
        return new_url;
    }

    public void loadUser()
    {
        UserData data = SaveUserData.LoadUser();
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
        notification.FireTime = LoginTime.AddDays(minutesOnTheHour);
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
    void SetUpIOSNotifications(int days, string firstname, string body,string link)
    {
        RequestAuthorization();
        DateTime date = DateTime.Now.AddDays(days);

        var timeTrigger = new iOSNotificationCalendarTrigger()
        {
            Year = date.Year,
            Month = date.Month,
            Day = date.Day,
            Repeats = false
        };

        var notification = new iOSNotification(){
            Title = "Senior Fit",
            Data = link,
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