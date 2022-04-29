using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class NotificationScript : MonoBehaviour,IPointerClickHandler 
{
    //public Text notification;
    public TextMeshProUGUI  notificationText;
    public string notification_messages="";
    public string delivery_time,link_text;
    int count=0;
    // Start is called before the first frame update
    void Start()
    {
       // notificationText = notificationText.gameObject.GetComponent<TextMeshPro>();
       
        userdata data = SaveUserData.LoadUser();
        //*****time check*************
        DateTime currentDate = System.DateTime.Now;
        DateTime oldDate = data.LoginTime;
        TimeSpan fullDifference = currentDate.Subtract(oldDate);
        Debug.Log("Log in time "+data.LoginTime.ToString()+" time diff : "+fullDifference);
        //notification.text=fullDifference.Minutes.ToString();

        NotificationList newnotification = SaveData.LoadNotifications();
         for (int i = 35; i >=0; i--)
        {

            if(fullDifference.TotalMinutes>(newnotification.allnotifications[i].interval)*10 & count<12)
            {
                int interval=newnotification.allnotifications[i].interval;
                string url_=newnotification.allnotifications[i].url;
                count++;
                delivery_time="<color=blue>"+oldDate.AddMinutes(interval*10).ToString()+"</color>";
                if(url_!="")
                {
                    link_text="<link=\""+url_+"\">"+url_+"</link>";
                    notification_messages+=delivery_time+"\n"+newnotification.allnotifications[i].message+"\n\n"+link_text+"\n\n";
                }

                else
                notification_messages+=delivery_time+"\n"+newnotification.allnotifications[i].message+"\n\n";
                Debug.Log("time :"+ oldDate.AddHours(interval*10).ToString("HH:mm:ss"));
            }
            
        }
       // notificationText.text="<link=\"https://www.google.com\">my link</link>";
        notificationText.SetText(notification_messages);
    }

    public void OnPointerClick(PointerEventData eventData) {
        
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(notificationText, eventData.position, null);  // If you are not in a Canvas using Screen Overlay, put your camera instead of null
        if (linkIndex != -1) { // was a link clicked?
            TMP_LinkInfo linkInfo = notificationText.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
