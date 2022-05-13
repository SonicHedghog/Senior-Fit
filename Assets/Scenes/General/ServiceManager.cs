using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
using System;
using System.IO;

public class ServiceManager : MonoBehaviour
{
#if UNITY_ANDROID
    public void UsedOnlyForAOTCodeGeneration() {
        //Bug reported on github https://github.com/aws/aws-sdk-net/issues/477
        //IL2CPP restrictions: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
        //Inspired workaround: https://docs.unity3d.com/ScriptReference/AndroidJavaObject.Get.html
 
        AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
        int valueString = jo.Get<int>("what");
    }

     void OnApplicationFocus ( bool focus )
     {
         if ( focus )  
         {
            
            
            InvokeRepeating("UpdateAWSinfo", 0.1f, 30f);
            onfocus=true;
         }  
         else            
         {
             onfocus=false;
         }
     }


#endif
    private LocationPlugin plugin;
    [SerializeField]
    public Text latText;
    [SerializeField]
    private Text lngText;
    [SerializeField]
    private Text altText;
    [SerializeField]
    private Text accuracyText;
    [SerializeField]
    private Text bearingText;
    [SerializeField]
    private Text speedText;
    [SerializeField]
    private Text providerText;
    [SerializeField]
    public Text distanceText;
    [SerializeField]
    private Text locationAvailabilityText;
    public Text GPSStatus;
    public string fileContents;
    string[] lines;
    public string fname;
    public string lname;
    public long contactno;
    public string current_date, start_time;
    public float time1 = 0, time2 = 0,Time_duration=0,new_duration=0,pauseTime1=0,pauseTime2=0;
    public float hours,minutes, seconds;
    public Text timestamp;
    public bool walkStart=false;
    public bool onfocus=true;
    public bool pause=false;

    public double NewDistance;

    DateTime CheckTime;

    private bool appQuit = false;
    // ***************AWS set up*******************************************


    public string IdentityPoolId = "us-east-2:1e21968c-63d4-4870-b719-89219bdc8f34";
    public string CognitoPoolRegion = RegionEndpoint.USEast2.SystemName;
    public string DynamoRegion = RegionEndpoint.USEast2.SystemName;

    private RegionEndpoint _CognitoPoolRegion
    {
        get { return RegionEndpoint.GetBySystemName(CognitoPoolRegion); }
    }

    private RegionEndpoint _DynamoRegion
    {
        get { return RegionEndpoint.GetBySystemName(DynamoRegion); }
    }

    private static IAmazonDynamoDB _ddbClient;
    private DynamoDBContext _context;

    private AWSCredentials _credentials;

    private AWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
                _credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoPoolRegion);
            return _credentials;
        }
    }

    protected IAmazonDynamoDB Client
    {
        get
        {
            if (_ddbClient == null)
            {
                _ddbClient = new AmazonDynamoDBClient(Credentials, _DynamoRegion);
            }

            return _ddbClient;
        }
    }

    private DynamoDBContext Context
    {
        get
        {
            if (_context == null)
                _context = new DynamoDBContext(Client);

            return _context;
        }
    }

    [DynamoDBTable("FinalGPSData")]
    public class FinalGPSData
    {
        [DynamoDBProperty]
        public string UserKey { get; set; }
        [DynamoDBProperty]
        public string FirstName { get; set; }
        [DynamoDBProperty]
        public string LastName { get; set; }
        [DynamoDBProperty]
        public long ContactNumber { get; set; }
        [DynamoDBProperty]
        public double latitudeData { get; set; }
        [DynamoDBProperty]
        public double longitudeData { get; set; }
        [DynamoDBProperty]
        public string Date { get; set; }
        [DynamoDBProperty]
        public string StartTime { get; set; }
        [DynamoDBProperty]
        public string CurrentTime { get; set; }



    }


    //**********************************************************************


    private void Awake()
    {
        plugin = GetComponentInChildren<LocationPlugin>();
    }

    private void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        plugin.OnLocation += OnLocationReceived;
        plugin.OnAvailability += OnLocationAvailability;
        plugin.OnDistanceChanged += OnDistanceChanged;
       // plugin.onTimeChanged += onTimeChanged;

        plugin.checkPermission();
        Screen.orientation = ScreenOrientation.Portrait;
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactno = data.contactno;
        //current_date = DateTime.Now.ToString("yyyy/MM/dd");

        CheckTime=DateTime.Now;
        current_date = DateTime.Now.ToString("MM/dd/yyyy");
        start_time = DateTime.Now.ToString("HH:mm:ss");



        UpdateAWSinfo();

        LoadMessages();
        NewDistance=0.0;

        /* Destination destination = new Destination();
         destination.destinationName = "Post";
         destination.latitude = 47.984641;
         destination.longitude = 8.815055;
         destination.triggerRadius = 40;
         plugin.setDestination(destination);*/
    }
    void Update()
    {
        if(walkStart==true & pause==false)
        ShowTime();
    }

    public void OnStartLocationServiceBtn()
    {
        plugin.StartLocationService(30000, 25000, 0, fname, lname, contactno, start_time, current_date);
        walkStart=true;
        pause=false;
        time1 = Time.unscaledTime;
        //Debug.Log("Time 1 "+time1);
    }
    public void OnStopLocationServiceBtn()
    {
        appQuit = true;
        walkStart=false;
        pause=false;
        plugin.StopLocationService();
        //UpdateAWSinfo();
        //Application.Quit();
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickPauseTimer()
    {
        if(pause==false)
        {
            pause=true;
            pauseTime1=Time.unscaledTime;
            //new_duration=0;


        }
        else
        {
            pause=false;
            pauseTime2=Time.unscaledTime;
            new_duration=new_duration+(pauseTime2-pauseTime1);
        }

    }

    private void OnLocationReceived(LocationData _location)
    {
        Debug.Log($"Lat: {_location.latitude} Lng: {_location.longitude} Alt: {_location.altitude}");
        //time2 = Time.time;
        WriteLocationToUI(_location);
    }

    private void OnLocationAvailability(bool _isAvailable)
    {
        locationAvailabilityText.text = _isAvailable.ToString();
    }

    private void OnDistanceChanged(double _distance)
    {
         Debug.Log("Inside distance changed");
        //if(distanceText is null) distanceText = GameObject.Find("distance").GetComponent<Text>();
        distanceText.text = $"Distance walked: {_distance} miles";

       
        
    }
   /* private void onTimeChanged(long _duration)
    {
        long diffSeconds = _duration / 1000;
        long seconds = diffSeconds % 60;
        long diffMinutes = diffSeconds / 60;
        long diffHours = diffMinutes / 60;

        if (diffHours > 0)
        {
            //timestamp.text = $"Duration: {diffHours} hrs {diffMinutes} mins {seconds} seconds";

        }
        else if (diffHours == 0 && diffMinutes > 0)
        {
           // timestamp.text = $"Duration: {diffMinutes} mins {seconds} seconds";

        }
        else
        {
           // timestamp.text = $"Duration: {diffSeconds} seconds";
        }

    }*/

    private void OnApplicationPause(bool _isPaused)
    {
        if (!_isPaused)
        {
            WriteLocationToUI(plugin.LastLocation);
        }
    }
   

    private void WriteLocationToUI(LocationData _location)
    {

        string status;



        string path = Application.persistentDataPath + "/userlocation.json";
        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            // fileContents="{ \"allgpsdata\" : "+fileContents+"}";

            status = "true";
            //Debug.Log("filecontent: "+ fileContents);

        }
        else
        {
            fileContents = "empty";
            status = "false";
            Debug.Log("File not found!");

        }
           

            
            
           /* if (Time_duration > 0)
            {
    
                
                 if ((int)minutes % 5 == 0)
            {
                var r = new System.Random();
                var randomLineNumber = r.Next(0, lines.Length - 1);

                string line = lines[randomLineNumber];
                GPSStatus.text = line;

            }
        }
        

        else
        {
            //timestamp.text = "Total time: 0 seconds";
            GPSStatus.text = "Let's get started!!";

        }*/




       // latText.text = fileContents.Length.ToString();
        //providerText.text =  fileContents.Length.ToString();
        // GPSStatus.text = lines[1];

        /* lngText.text = "Longitude: " + _location.longitude.ToString();
         altText.text = "Altitude: " + (plugin.HasAltitude() ? _location.altitude.ToString() : "-");
         accuracyText.text = "Accuracy: " + (plugin.HasAccuracy() ? _location.accuracy.ToString() : "-");
         bearingText.text = "Bearing: " + (plugin.HasBearing() ? _location.bearing.ToString() : "-");
         speedText.text = "Speed: " + (plugin.HasSpeed() ? _location.speed.ToString() : "-");*/

    }

    
    public int oldminutes=0;

    public int MessageLoaded=0;
     public void ShowTime()
    {
         time2 = Time.unscaledTime;
         //Debug.Log("Time 2 "+time2);
        Time_duration=(time2-time1)-new_duration;
        //new_duration=0;

        if(GPSStatus is null) GPSStatus = GameObject.Find("GPSMsg").GetComponent<Text>();
        if(timestamp is null) timestamp = GameObject.Find("timestamp").GetComponent<Text>();
        if(distanceText is null) distanceText = GameObject.Find("distance").GetComponent<Text>();
       
          if (Time_duration > 0)
            {
    
                hours = (int)(Time_duration / 3600);
                minutes =(int) ((Time_duration % 3600) / 60);
                seconds = (int)(Time_duration % 60);

                if (hours > 0)
                    timestamp.text = $"Duration: {hours} hrs {minutes} mins {seconds} seconds";
                else if(hours==0 && minutes>0)
                 {
                    timestamp.text = $"Duration: {minutes} mins {seconds} seconds";

                }
                else
                {
                     timestamp.text = $"Duration: {seconds} seconds";
                }

                
              if ((int)minutes>0 & ((int)minutes%5==0))
            {
               if(MessageLoaded!=(int)minutes)
               {
                  MessageLoaded=(int)minutes; 
               int randomLineNumber=(int)(minutes/5);

                string line = lines[randomLineNumber];
                GPSStatus.text = line;
               // GetDistance();
               }
               UpdateAWSinfo();

            }

            distanceText.text = "Distance walked: "+NewDistance.ToString()+" miles";

           
           
            }
            else
        {
            //timestamp.text = "Total time: 0 seconds";
            GPSStatus.text = "Let's get started!!";

        }

        

        


    }

    void UpdateAWSinfo()
    {

        Debug.Log("Update AWS called");
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet");
        }
        else
        {
            GPSList newgpslist = SaveData.LoadGPSData();

            foreach (newLocation newuse in newgpslist.allgpsdata)
            {


                FinalGPSData newUser = new FinalGPSData
                {


                    //latitudeData=newuse.latitude

                    FirstName = newuse.firstName,
                    LastName = newuse.lastName,
                    ContactNumber = newuse.contactNo,
                    latitudeData = newuse.latitude,
                    longitudeData = newuse.longitude,
                    UserKey = newuse.contactNo + newuse.currentDate + newuse.currentTime,
                    Date = newuse.currentDate,
                    StartTime = newuse.startTime,
                    CurrentTime = newuse.currentTime

                };
                Context.SaveAsync(newUser, (result) =>
                {
                    if (result.Exception == null)
                        Debug.Log("GPS saved");
                });

                var request = new DescribeTableRequest
                {
                    TableName = @"FinalGPSData"
                };

                DateTime newTime = DateTime.Parse(newuse.currentTime, System.Globalization.CultureInfo.CurrentCulture);

                if(newuse.totaldistance>NewDistance && DateTime.Compare(CheckTime,newTime)<=0)
                {
                    NewDistance=newuse.totaldistance;
                    Debug.Log("new total distance "+NewDistance);
                }

                


            }


        }


    }
    private void LoadMessages()
    {
        string[] paths = { Application.streamingAssetsPath, "Routines", "messages.txt" };


        if (Application.platform == RuntimePlatform.Android)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
            www.SendWebRequest();
            while (!www.isDone)
            {
            }
            lines = www.downloadHandler.text.Split('\n');
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            lines = File.ReadAllLines(Application.streamingAssetsPath + "/Routines/" + "messages.txt");
        }
    }

}
