using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using System.IO;
using System;
using Amazon.DynamoDBv2.Model;

public class BackgroundiOS : MonoBehaviour {

	#if UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void backgroundLaunch (string _fname, string _lname, long _contactno, string _start_time, string _current_date, string _filename);
		[DllImport ("__Internal")]
		private static extern void backgroundStop ();		

        void OnApplicationFocus ( bool focus )
     {
         if ( focus )  
         {
             InvokeRepeating("UpdateAWSinfo", 0.1f, 30f);
         }  
     }
	#endif

    [SerializeField] public Text latText;
    [SerializeField] private Text lngText;
    [SerializeField] public Text distanceText;
    [SerializeField] public Text GPSStatus;
    public string fileContents;
    string[] lines ;
    public string fname;
    public string lname;
    public long contactno;
    public string current_date, start_time;
    public float time1 = 0, time2 = 0, Time_duration=0;
    public float hours,minutes, seconds;
     public int minute, second;
     public double currentdistance;
     public double totaldistance;
     public Text timestamp;
    public bool walkStart=false;


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

    public void Start() {
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        
        Screen.orientation = ScreenOrientation.Portrait;
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactno = data.contactno;
        current_date = DateTime.Now.ToString("yyyy/MM/dd");
        start_time = DateTime.Now.ToString("HH:mm:ss");
      
        time1 = Time.time;

        LoadMessages();
    }

	// Start background task
	public void StartTask () {
		#if UNITY_EDITOR		
		#elif UNITY_IOS

        backgroundLaunch (fname,lname,contactno,start_time,current_date, Application.persistentDataPath + "/userlocation.json");
        walkStart = true;
		#endif
	}

	// Stop background task
	public void StopTask () {
		#if UNITY_EDITOR				
		#elif UNITY_IOS
			backgroundStop ();
            walkStart=false;
		#endif
        SceneManager.LoadScene("MainMenu");
	}	

    private void WriteLocationToUI(string message)
    {  
        string status;

        if(GPSStatus is null) GPSStatus = GameObject.Find("GPSMsg").GetComponent<Text>();
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
        if(latText is null) latText = GameObject.Find("Latitude").GetComponent<Text>();
        latText.text =fileContents.Length.ToString();       
    }

    private void OnDistanceChanged(string message)
    {
        Debug.Log(message);
        currentdistance = distance(double.Parse(message.Split(' ')[0]), double.Parse(message.Split(' ')[1])
            , double.Parse(message.Split(' ')[2]), double.Parse(message.Split(' ')[3]));

        totaldistance += currentdistance;
        totaldistance=(float)Math.Round(totaldistance * 100) / 100;
        if(distanceText is null) distanceText = GameObject.Find("distance").GetComponent<Text>();
        distanceText.text = "Distance walked: " + (totaldistance * 0.62).ToString("N2") + " miles";
    }

    public void ShowTime()
    {
         time2 = Time.unscaledTime;
         //Debug.Log("Time 2 "+time2);
        Time_duration=time2-time1;

        if(GPSStatus is null) GPSStatus = GameObject.Find("GPSMsg").GetComponent<Text>();
        if(timestamp is null) timestamp = GameObject.Find("timestamp").GetComponent<Text>();
          if (Time_duration > 0)
            {
    
                hours = (int)(Time_duration / 3600);
                minutes = (int)((Time_duration % 3600) / 60);
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

                 if (minutes>0 & minutes % 5 == 0)
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
                FinalGPSData newUser = new FinalGPSData
                {
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
                    TableName = @"FinalGPSData"
                };
            }
        }   
    }

    public double distance(double lat1,
                           double lat2,
                           double lon1,
                           double lon2)
    {

        double dLat = (Math.PI / 180) * (lat2 - lat1);
        double dLon = (Math.PI / 180) * (lon2 - lon1);

        // convert to radians
        lat1 = (Math.PI / 180) * (lat1);
        lat2 = (Math.PI / 180) * (lat2);

        // apply formulae
        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Pow(Math.Sin(dLon / 2), 2) *
                   Math.Cos(lat1) * Math.Cos(lat2);
        double rad = 6371;
        double c = 2 * Math.Asin(Math.Sqrt(a));
        return rad * c;
    }

    private void LoadMessages()
    {
        string[] paths = {Application.streamingAssetsPath, "Routines",  "messages.txt"};
        lines = File.ReadAllLines(Application.streamingAssetsPath + "/Routines/" +  "messages.txt");
    }

    void Update()
    {
        if(walkStart==true)
        ShowTime();
    }

}
