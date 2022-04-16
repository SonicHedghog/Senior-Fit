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
using Unity.Notifications.iOS;
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


    public void EventAlarmTest(int minutesOnTheHour, string firstname, string body,string link)
    {

        /*var notification_id = 10000;
        var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(notification_id);

        if (notificationStatus == NotificationStatus.Delivered)
        {
            // Remove the previously shown notification from the status bar.
            AndroidNotificationCenter.CancelNotification(notification_id);
        }*/
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


        /* var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(id);


         if (notificationStatus == NotificationStatus.Delivered)
         {
             // Remove the previously shown notification from the status bar.
             AndroidNotificationCenter.CancelNotification(id);
             id=AndroidNotificationCenter.SendNotification(notification, "notification_id");
         }
         else if (notificationStatus == NotificationStatus.Unknown)
         {
             id= AndroidNotificationCenter.SendNotification(notification, "notification_id");
         }
 */

    #endif

    }
    


    void LoginButtonClick()
    {
        contactno = long.Parse(ContactNumber.text);
        fname = firstname.text;
        lname = lastname.text;
        SaveUserData.SaveUser(this);
        SceneManager.LoadScene("MainMenu");
        int time=0;
        string body;
        for (int i = 0; i < 5; i++)
        {
            
            time=time+6;
            body = "Hi " + fname + " ! " + newnotification.allnotifications[i].message;
            Debug.Log("body " + body);
            string link = newnotification.allnotifications[i].url;
            //SceneChange.OpenNewLink(newnotification.allnotifications[0].url);
            EventAlarmTest(time, fname, body,link);
            Debug.Log("notification successful");
        }
        //LoadNotifications();
        /*string body;
       
        body = "Hi " + fname + " ! " + newnotification.allnotifications[0].message;
        Debug.Log("body "+ body);
        new_url=newnotification.allnotifications[0].url;
        //SceneChange.OpenNewLink(newnotification.allnotifications[0].url);
        EventAlarmTest(30, fname, body);
        Debug.Log("notification successful");
        //SceneChange.set_url(newnotification.allnotifications[0].url);
        body = "Hi " + fname + " ! " + newnotification.allnotifications[1].message;
        
       new_url=newnotification.allnotifications[1].url;
        EventAlarmTest(60, fname, body);
        //SceneChange.set_url(newnotification.allnotifications[1].url);
     
       
        EventAlarmTest(120, fname, body);
        body = "Hi " + fname + " ! " + newnotification.allnotifications[2].message;
        new_url=newnotification.allnotifications[2].url;
       EventAlarmTest(560, fname, body);
        //SceneChange.set_url(newnotification.allnotifications[2].url);*/



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