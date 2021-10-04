using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
using System.IO;
using System;



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
    public static Walk Instance {set; get; }
    public string latitude;
    public string longitude;
    public Text Lat;
    public Text Long;
    public Text TotalDistance;
    public Text updatecalled;
    public Text timestamp;
    public Text GPSStatus;
    public string fname;
    public string lname;
    public long contactno;
    public string time;
    public int count=0;

    
    public double lat1,lat2,long1,long2,totaldistance=0.0;

    // ***************AWS set up*******************************************


    public string IdentityPoolId = "";
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
        public string latitudeData { get; set; }
        [DynamoDBProperty]
       public string longitudeData { get; set; }
        [DynamoDBProperty]
        public string Datetime { get; set; }

    }


    //**********************************************************************

    

    
    // Start is called before the first frame update
    private void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        InputSystem.EnableDevice(StepCounter.current);
        InputSystem.AddDevice<StepCounter>();
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactno = data.contactno;
        //DontDestroyOnLoad(gameObject);
        
        StartCoroutine(StartLocationService());
        
 
       
 
    }

     private void Awake()
    {
        
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    public void backbutton()
    {
         SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator StartLocationService()
    {
        if(!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        Input.location.Start();
        int maxWait=20;

        while(Input.location.status == LocationServiceStatus.Initializing && maxWait>0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if(maxWait <= 0)
        {
            GPSStatus.text="Timed out";
            yield break;
        }
        if(Input.location.status == LocationServiceStatus.Failed)
        {
            GPSStatus.text="Unable to determine location";
            //Debug.Log("Unable to determine location");
            yield break;
        }
        else
        {
            InvokeRepeating("UpdateGPSData",0.5f,30f);
        }

    }

    private void UpdateGPSData()
    {
        count++;
        if(Input.location.status == LocationServiceStatus.Running)
        {

            GPSStatus.text="Running";
            Lat.text="Lat: "+ Input.location.lastData.latitude.ToString();
            latitude=Lat.text;
            //lat2=Convert.ToDouble(latitude);
            Long.text="Long: "+ Input.location.lastData.longitude.ToString();
            longitude=Long.text;
            //long2=Convert.ToDouble(longitude);
            timestamp.text="timestamp: "+Input.location.lastData.timestamp.ToString();
            
            updatecalled.text="Update called:" + count.ToString()+" times";
            time = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");

            SaveData.SaveGPSData(this);
            UpdateAWSinfo();

            if(count<=1)
            {
                lat1=Convert.ToDouble(latitude);
                long1=Convert.ToDouble(longitude);
            }
            else
            {
                lat2=Convert.ToDouble(latitude);
                long2=Convert.ToDouble(longitude);
            }

            totaldistance=distance(lat1,lat2,long1,long2)*1000;
            TotalDistance.text=totaldistance.ToString();


            //*******************************aws update****************************

            


             
        }
        else
        {
            GPSStatus.text="Stop";
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

                foreach (GPSData newuse in newgpslist.allgpsdata)
                {
                GPSINFO newUser = new GPSINFO
                {
                    FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,                 
                    latitudeData=newuse.latitudedata,
                    longitudeData=newuse.longitudedata,
                    UserKey = contactno.ToString()+newuse.time,
                    Datetime=newuse.time
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
                }
                

            }
    }

    static double toRadians(
           double angleIn10thofaDegree)
    {
        // Angle in 10th
        // of a degree
        return (angleIn10thofaDegree * 
                       Math.PI) / 180;
    }

    static double distance(double lat1,
                           double lat2,
                           double lon1,
                           double lon2)
    {
 
        // The math module contains
        // a function named toRadians
        // which converts from degrees
        // to radians.
        lon1 = toRadians(lon1);
        lon2 = toRadians(lon2);
        lat1 = toRadians(lat1);
        lat2 = toRadians(lat2);
 
        // Haversine formula
        double dlon = lon2 - lon1;
        double dlat = lat2 - lat1;
        double a = Math.Pow(Math.Sin(dlat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Pow(Math.Sin(dlon / 2),2);
             
        double c = 2 * Math.Asin(Math.Sqrt(a));
 
        // Radius of earth in
        // kilometers. Use 3956
        // for miles
        double r = 6371;
 
        // calculate the result
        return (c * r);
    }


}
