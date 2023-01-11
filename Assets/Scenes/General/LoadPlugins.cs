using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadPlugins : MonoBehaviour
{
    [SerializeField] public Text latText;
    [SerializeField] public Text distanceText;

    public Button StartWalking;
    public Button StopWalking;
    public GameObject DialoguePanel;
    public GameObject WalkPanel;
    public Button OK;

    public Button PauseTimer;
    public Button Back;
    public Text GPSStatus;
    public Text timestamp;
    public bool pause=false;

    #if UNITY_ANDROID

    private ServiceManager locationService; 
    private LocationPlugin locationPlugin;

    #elif UNITY_IOS

    private BackgroundiOS locationService; 

    #endif

    void Start()
    {

    }

    public void DisclaimerAccepted()
    {
        DialoguePanel.SetActive(false);
        WalkPanel.SetActive(true);
    }

    void OnEnable()
    {
        #if UNITY_ANDROID

        locationPlugin = this.gameObject.AddComponent(typeof(LocationPlugin)) as LocationPlugin;

        locationService = this.gameObject.AddComponent(typeof(ServiceManager)) as ServiceManager;

        if(latText is null) latText = GameObject.Find("Latitude").GetComponent<Text>();
        if(GPSStatus is null) GPSStatus = GameObject.Find("GPSMsg").GetComponent<Text>();
        if(distanceText is null) distanceText = GameObject.Find("distance").GetComponent<Text>();
        if(timestamp is null) timestamp = GameObject.Find("timestamp").GetComponent<Text>();
        
        locationService.latText = latText;
        locationService.distanceText = distanceText;
        locationService.GPSStatus = GPSStatus;
        locationService.timestamp=timestamp;


        #elif UNITY_IOS

        locationService = this.gameObject.AddComponent(typeof(BackgroundiOS)) as BackgroundiOS;
        
        if(latText is null) latText = GameObject.Find("Latitude").GetComponent<Text>();
        if(GPSStatus is null) GPSStatus = GameObject.Find("GPSMsg").GetComponent<Text>();
        if(distanceText is null) distanceText = GameObject.Find("distance").GetComponent<Text>();
        if(timestamp is null) timestamp = GameObject.Find("timestamp").GetComponent<Text>();

        locationService.latText = latText;
        locationService.distanceText = distanceText;
        locationService.GPSStatus = GPSStatus;
        locationService.timestamp = timestamp;

        #endif
    }

    public void startPlugin()
    {
        #if UNITY_ANDROID

        locationService.OnStartLocationServiceBtn();


        #elif UNITY_IOS

        locationService.StartTask();

        #endif

        StartWalking.gameObject.SetActive(false);
        PauseTimer.gameObject.SetActive(true);
        Back.gameObject.SetActive(false);
    }
     public void stopPlugin()
    {
        #if UNITY_ANDROID

        locationService.OnStopLocationServiceBtn();


        #elif UNITY_IOS

        locationService.StopTask();

        #endif

        StartWalking.gameObject.SetActive(true);
        Back.gameObject.SetActive(true);
    }

     public void OnClickPauseTimer()
     {
         if(pause==false)
         {
             pause=true;
              GameObject.Find("PauseTimer").GetComponentInChildren<Text>().text = "Resume Walking";
         }
         else
         {
             pause=false;
             GameObject.Find("PauseTimer").GetComponentInChildren<Text>().text = "Pause Walking";
         }
        
        /* if(PauseTimer.Text=="Pause Timer")
         {
             PauseTimer.Text="Resume Timer";

         }
         else
         {
             PauseTimer.Text=="Pause Timer";
         }*/
          locationService.OnClickPauseTimer();
     }

     public void LoadMainMenu()
     {
          SceneManager.LoadScene("MainMenu");
     }
}
