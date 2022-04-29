using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_ANDROID

using Unity.Notifications.Android;
#endif

#if UNITY_IOS
// using Unity.Notifications.iOS;
#endif
using System.IO;

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

    }
    


    void LoginButtonClick()
    {
        contactno = long.Parse(ContactNumber.text);
        fname = firstname.text;
        lname = lastname.text;
        LoginTime=DateTime.Now;
        SaveUserData.SaveUser(this);
        SceneManager.LoadScene("MainMenu");
        int time=0;
        string body;
        
        Debug.Log("first time log in & notification : ");
        foreach (UserNotification noti in newnotification.allnotifications)
        {
            time=noti.interval*10;
            //time=time+10;
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



    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {



        }

    }


    // Update is called once per frame
    void Update()
    {

    }
}