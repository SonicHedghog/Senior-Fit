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


public class Walk : MonoBehaviour
{
#if UNITY_ANDROID
    public void UsedOnlyForAOTCodeGeneration() {
        //Bug reported on github https://github.com/aws/aws-sdk-net/issues/477
        //IL2CPP restrictions: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
        //Inspired workaround: https://docs.unity3d.com/ScriptReference/AndroidJavaObject.Get.html
 
        AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
        int valueString = jo.Get<int>("what");
    }
#endif
    public static Walk Instance { set; get; }
    public float latitude;
    public float longitude;

    public Text Lat;
    public Text Long;
    public Text TotalDistance;
    public Text updatecalled;
    public Text timestamp;
    public Text GPSStatus;
    public string fname;
    public string lname;
    public long contactno;
    public string current_date, start_time, current_time;
    public int count = 0;

    public float time1 = 0, time2 = 0;
    public int minute, second;

    public double lat1, lat2, long1, long2, totaldistance = 0.0, currentdistance = 0.0, new_lat, new_long;
    string[] lines ;

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

    [DynamoDBTable("GPSINFO")]
    public class GPSINFO
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




    // Start is called before the first frame update
    private void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        Screen.orientation = ScreenOrientation.Portrait;
        // InputSystem.EnableDevice(StepCounter.current);
        // InputSystem.AddDevice<StepCounter>();
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactno = data.contactno;
        //DontDestroyOnLoad(gameObject);

        StartCoroutine(StartLocationService());
        Debug.Log(DateTime.Now.ToString("HH:mm:ss"));
        LoadMessages();
        



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

    private void Awake()
    {

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        Application.runInBackground=true;
    }
   

    public void backbutton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        Input.location.Start();
        int maxWait = 20;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            GPSStatus.text = "Timed out";
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            GPSStatus.text = "Unable to determine location";
            //Debug.Log("Unable to determine location");
            yield break;
        }
        else
        {


            InvokeRepeating("UpdateGPSData", 0.5f, 30f);
        }

    }

    private void UpdateGPSData()
    {



        count++;
        if (Input.location.status == LocationServiceStatus.Running)
        {


            //Lat.text = "Lat: " + Input.location.lastData.latitude.ToString();
            latitude = Input.location.lastData.latitude;
            //lat2=Convert.ToDouble(latitude);
            Long.text = "Long: " + Input.location.lastData.longitude.ToString();
            longitude = Input.location.lastData.longitude;
            //long2=Convert.ToDouble(longitude);
            //timestamp.text = "timestamp: " + Time.time.ToString();

            updatecalled.text = "Update called:" + count.ToString() + " times";


            //SaveData.SaveGPSData(this);
           // UpdateAWSinfo();

            if (count <= 1)
            {
                current_date = DateTime.Now.ToString("yyyy/MM/dd");
                start_time = DateTime.Now.ToString("HH:mm:ss");
                current_time = start_time;
                time1 = Time.time;
                lat1 = Convert.ToDouble(latitude);
                long1 = Convert.ToDouble(longitude);
                lat2 = Convert.ToDouble(latitude);
                long2 = Convert.ToDouble(longitude);
            }
            else
            {
                current_time = DateTime.Now.ToString("HH:mm:ss");
                time2 = Time.time;
                lat1 = lat2;
                long1 = long2;
                new_lat = Convert.ToDouble(latitude);
                new_long = Convert.ToDouble(longitude);

                if (lat1 != new_lat || new_long != new_long)
                {
                    lat2 = new_lat;
                    long2 = new_long;
                }
            }

            if ((time2 - time1) > 0)
            {
                minute = (int)((time2 - time1) / 60);
                second = (int)((time2 - time1) % 60);

                if (minute > 0)
                    timestamp.text = "Duration : " + minute.ToString() + " minute " + second.ToString() + " seconds";
                else
                    timestamp.text = "Duration : " + second.ToString() + " seconds";



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
                timestamp.text = "Total time: 0 seconds";
                GPSStatus.text = "Let's get started!!";

            }


            currentdistance = distance(lat1, lat2, long1, long2);
            //GPSStatus.text = "Running";
            Lat.text = "Current time " + current_time.ToString();

            totaldistance += currentdistance;
            totaldistance=(float)Math.Round(totaldistance * 100) / 100;

            TotalDistance.text = "Distance: " + (totaldistance * 0.62).ToString() + " miles";


            //*******************************aws update****************************





        }
        else
        {
            GPSStatus.text = "Stop";
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


                GPSINFO newUser = new GPSINFO
                {
                    /*FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,
                    latitudeData = newuse.latitudedata,
                    longitudeData = newuse.longitudedata,
                    UserKey = newuse.contactno + newuse.current_date + newuse.current_time,
                    Date = newuse.current_date,
                    StartTime = newuse.start_time,
                    CurrentTime = newuse.current_time*/

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
                    TableName = @"GPSINFO"
                };


                /****************************update****************************

                GPSINFO GPSRetrieved = null;

                
                Context.LoadAsync<GPSINFO>(user_key, (result) =>
                 {
                     if (result.Exception == null)
                     {
                         GPSRetrieved = result.Result as GPSINFO;
                         // Update few properties.
                         GPSRetrieved.Position_Lat = position_lat;
                         GPSRetrieved.Position_Long = position_long;
                         // Replace existing authors list with this

                         Context.SaveAsync<GPSINFO>(GPSRetrieved, (res) =>
             {
                 if (res.Exception == null)
                     Debug.Log("\nTable updated");
             });
                     }
                     else
                     {
                         GPSINFO newUser = new GPSINFO
                {
                    FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,
                    latitudeData = newuse.latitudedata,
                    longitudeData = newuse.longitudedata,
                    UserKey = user_key,
                    Position_Lat = position_lat,
                    Position_Long = position_long,
                    //Datetime = newuse.time
                    Date = newuse.current_date,
                    StartTime = newuse.start_time
                };
                Context.SaveAsync(newUser, (save_result) =>
                {
                    if (save_result.Exception == null)
                        Debug.Log("GPS saved");
                });

                var request = new DescribeTableRequest
                {
                    TableName = @"GPSINFO"
                };

                     }
                 });*/
            }


        }
    }



    static double distance(double lat1,
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


}
