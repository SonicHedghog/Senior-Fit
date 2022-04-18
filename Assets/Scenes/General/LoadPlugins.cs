using UnityEngine;
using UnityEngine.UI;

public class LoadPlugins : MonoBehaviour
{
    [SerializeField] public Text latText;
    [SerializeField] public Text distanceText;

    public Button StartWalking;
    public Button StopWalking;
    public Text GPSStatus;
    public Text timestamp;

    #if UNITY_ANDROID

    private ServiceManager locationService; 
    private LocationPlugin locationPlugin;

    #elif UNITY_IOS

    private BackgroundiOS locationService; 

    #endif

    void OnEnable()
    {
        #if UNITY_ANDROID

        locationPlugin = this.gameObject.AddComponent(typeof(LocationPlugin)) as LocationPlugin;

        locationService = this.gameObject.AddComponent(typeof(ServiceManager)) as ServiceManager;
        
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
    }
     public void stopPlugin()
    {
        #if UNITY_ANDROID

        locationService.OnStopLocationServiceBtn();


        #elif UNITY_IOS

        locationService.StopTask();

        #endif

        StartWalking.gameObject.SetActive(true);
    }
}
