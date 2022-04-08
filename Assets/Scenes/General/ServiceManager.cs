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
         }  
         else            ;
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
    string[] lines ;
    public string fname;
    public string lname;
    public long contactno;
    public string current_date, start_time;
    public float time1 = 0, time2 = 0;
     public int minute, second;
     public Text timestamp;

     private bool appQuit=false;
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

    [DynamoDBTable("demoGPS")]
    public class demoGPS
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
        plugin.onTimeChanged+=onTimeChanged;
        
        plugin.checkPermission();
        Screen.orientation = ScreenOrientation.Portrait;
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactno = data.contactno;
        current_date = DateTime.Now.ToString("yyyy/MM/dd");
        start_time = DateTime.Now.ToString("HH:mm:ss");
      
        time1 = Time.time;

        LoadMessages();

       /* Destination destination = new Destination();
        destination.destinationName = "Post";
        destination.latitude = 47.984641;
        destination.longitude = 8.815055;
        destination.triggerRadius = 40;
        plugin.setDestination(destination);*/
    }

    public void OnStartLocationServiceBtn()
    {
        plugin.StartLocationService(30000, 25000, 0,fname,lname,contactno,start_time,current_date);
    }
    public void OnStopLocationServiceBtn()
    {
        appQuit=true;
        plugin.StopLocationService();
        //UpdateAWSinfo();
        Application.Quit();
        //SceneManager.LoadScene("MainMenu");
    }

    private void OnLocationReceived(LocationData _location)
    {
        Debug.Log($"Lat: {_location.latitude} Lng: {_location.longitude} Alt: {_location.altitude}");
        time2 = Time.time;
        WriteLocationToUI(_location);
    }

    private void OnLocationAvailability(bool _isAvailable)
    {
        locationAvailabilityText.text = _isAvailable.ToString();
    }

    private void OnDistanceChanged(double _distance)
    {
        distanceText.text = $"Distance walked: {_distance} miles";
    }
    private void onTimeChanged(long _duration)
    {
        long diffSeconds = _duration / 1000;
        long seconds=diffSeconds%60;
        long diffMinutes = diffSeconds / 60;
        long diffHours= diffMinutes/60;

        if(diffHours>0)
        {
           timestamp.text=$"Duration: {diffHours} hrs {diffMinutes} mins {seconds} seconds";

        }
        else if(diffHours==0 && diffMinutes>0)
        {
            timestamp.text=$"Duration: {diffMinutes} mins {seconds} seconds";

        }
        else
        {
            timestamp.text=$"Duration: {diffSeconds} seconds";
        }
        
    }

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
           
            status="true";
            //Debug.Log("filecontent: "+ fileContents);

        }
        else
        {
            fileContents="empty";
            status="false";
            Debug.Log("File not found!");
            
        }
        if ((time2 - time1) > 0)
            {
                minute = (int)((time2 - time1) / 60);
                second = (int)((time2 - time1) % 60);

                //if (minute > 0)
                   // timestamp.text = "Duration : " + minute.ToString() + " minute " + second.ToString() + " seconds";
               // else
                   // timestamp.text = "Duration : " + second.ToString() + " seconds";



                if (minute % 5 == 0)
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

            }

        


        latText.text =fileContents.Length.ToString();
        //providerText.text =  fileContents.Length.ToString();
       // GPSStatus.text = lines[1];
        
       /* lngText.text = "Longitude: " + _location.longitude.ToString();
        altText.text = "Altitude: " + (plugin.HasAltitude() ? _location.altitude.ToString() : "-");
        accuracyText.text = "Accuracy: " + (plugin.HasAccuracy() ? _location.accuracy.ToString() : "-");
        bearingText.text = "Bearing: " + (plugin.HasBearing() ? _location.bearing.ToString() : "-");
        speedText.text = "Speed: " + (plugin.HasSpeed() ? _location.speed.ToString() : "-");*/
       
    }

     void UpdateAWSinfo()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet");
        }
        else
        {
            GPSList newgpslist = SaveData.LoadGPSData();

            foreach (newLocation newuse in newgpslist.allgpsdata)
            {


                demoGPS newUser = new demoGPS
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
                    CurrentTime=newuse.currentTime
                   
                };
                Context.SaveAsync(newUser, (result) =>
                {
                    if (result.Exception == null)
                        Debug.Log("GPS saved");
                });

                var request = new DescribeTableRequest
                {
                    TableName = @"demoGPS"
                };


            }


        }

        
    }
    private void LoadMessages()
    {
        string[] paths = {Application.streamingAssetsPath, "Routines",  "messages.txt"};
        
        
        if(Application.platform == RuntimePlatform.Android)
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
            lines = File.ReadAllLines(Application.streamingAssetsPath + "/Routines/" +  "messages.txt");
        }
    }

}
