using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class NotificationScript : MonoBehaviour
{
    public Text notification;
    public string notification_messages="";
    public string delivery_time;
    int count=0;
    // Start is called before the first frame update
    void Start()
    {
        userdata data = SaveUserData.LoadUser();
        //*****time check*************
        DateTime currentDate = System.DateTime.Now;
        DateTime oldDate = data.LoginTime;
        TimeSpan fullDifference = currentDate.Subtract(oldDate);
        Debug.Log("Log in time "+data.LoginTime.ToString()+" time diff : "+fullDifference);
        notification.text=fullDifference.Minutes.ToString();

        NotificationList newnotification = SaveData.LoadNotifications();
         for (int i = 35; i >=0; i--)
        {

            if(fullDifference.TotalHours>newnotification.allnotifications[i].interval & count<12)
            {
                count++;
                delivery_time="<color=blue>"+oldDate.AddMinutes(newnotification.allnotifications[i].interval).ToString()+"</color>";
                notification_messages+=delivery_time+"\n"+newnotification.allnotifications[i].message+"\n\n";
                Debug.Log(notification_messages);
            }
            
        }
        notification.text=notification_messages;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
