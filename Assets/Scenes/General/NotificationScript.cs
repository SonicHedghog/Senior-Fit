using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;


public class NotificationScript : MonoBehaviour,IPointerClickHandler 
{
    public TextMeshProUGUI  notificationText;
    public string notification_messages = "";
    public string delivery_time,link_text;
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {       
        UserData data = SaveUserData.LoadUser();

        //*****time check*************
        DateTime currentDate = System.DateTime.Now;
        DateTime oldDate = data.LoginTime;
        TimeSpan fullDifference = currentDate.Subtract(oldDate);

       // DateTime scheduled;
        Debug.Log("Log in time "+data.LoginTime.ToString()+" time diff : "+fullDifference);
        
        int interval=0;
        NotificationList newnotification = SaveData.LoadNotifications();
        for(int i=59;i>=0;i--)
        {
            interval=newnotification.allnotifications[i].interval;

            if((int)fullDifference.TotalDays>=(interval) && count<12)
            {
                Debug.Log("time dif"+(int)fullDifference.TotalDays+"interval "+interval);
               
                string url_=newnotification.allnotifications[i].url;
                count++;
                delivery_time="<color=\"red\">"+oldDate.AddDays(interval).ToString()+"</color>";
                if(url_!="")
                {
                    link_text="<link=\""+url_+"\">"+"<color=\"blue\"><b>"+url_+"</b></color></link>";
                    notification_messages+=delivery_time+"\n"+"<color=\"black\">"+newnotification.allnotifications[i].message+"</color>\n"+link_text+"\n\n";
                }

                else
                notification_messages+=delivery_time+"\n"+"<color=\"black\">"+newnotification.allnotifications[i].message+"</color>\n\n";
                Debug.Log("time :"+ oldDate.AddDays(interval).ToString("HH:mm:ss"));
            }
            
        }
        notificationText.SetText(notification_messages);
    }

    public void OnPointerClick(PointerEventData eventData) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(notificationText, eventData.position, null);  // If you are not in a Canvas using Screen Overlay, put your camera instead of null
        if (linkIndex != -1) { // was a link clicked?
            TMP_LinkInfo linkInfo = notificationText.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
