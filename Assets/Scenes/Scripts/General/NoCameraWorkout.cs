using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TensorFlowLite;
#if UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
using Mono.Data.Sqlite;

public class NoCameraWorkout : MonoBehaviour
{
    #if UNITY_ANDROID
        public void UsedOnlyForAOTCodeGeneration() {   
            AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
            int valueString = jo.Get<int>("what");
        }
    #endif

    public int exercisenumber;
    public string Exercise;
    public string videoPath;
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public Text timeText;
    public float time1 = 0, time2 = 0, Time_duration = 0, new_duration = 0, pauseTime1 = 0, pauseTime2 = 0;
    public float hours, minutes, seconds;
    public string date, time, fname, lname;
    public long contactno;
    public bool CamPermission = false;
    public GameObject cameraButton;

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
    private SqliteConnection dbconn;
    private string conn;
    private SqliteCommand dbcmd;
    private string sqlQuery;
    string filepath;
    private AWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
                _credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoPoolRegion);
            return _credentials;
        }
    }

    private IAmazonDynamoDB Client
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

    [DynamoDBTable("ExerciseData")]
    public class ExerciseData
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
        public string ExerciseName { get; set; }
        [DynamoDBProperty]
        public string date { get; set; }
        [DynamoDBProperty]
        public string startTime { get; set; }
        [DynamoDBProperty]
        public string repCount { get; set; }

        [DynamoDBProperty]
        public int duration { get; set; }


    }

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            var poseDetect = new PoseDetect("ML Models/pose_detection.tflite");
        }
        catch (DllNotFoundException e)
        {
            Debug.Log("Phone not compatible");
            cameraButton.SetActive(false);
        }

        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        exercisenumber = SceneChange.GetExerciseNumber();
        switch(exercisenumber)
        {
            case 1:
                Exercise="Seated March";
                break;
            case 2:
                Exercise="Single Leg Stance";
                break;
            case 3:
                Exercise="Shoulder Touch";
                break;
            case 4:
                Exercise="Chair Sit To Stand";
                break;
            case 5:
                Exercise="Marching In Place";
                break;
            case 6:
                Exercise = "Seated Hamstring Stretch";
                break;
            case 7:
                Exercise = "Rock The Boat";
                break;
            case 8:
                Exercise = "Shoulder Stretch";
                break;
            case 9:
                Exercise = "Standing Calf Stretch";
                break;
            case 10:
                Exercise = "Standing Leg Curl";
                break;
            case 11:
                Exercise = "Standing Thigh Stretch";;
                break;
            case 12:
                Exercise = "Wall Push Up";
                break;
            case 13:
                Exercise = "Side Stepping";
                break;
            case 14:
                Exercise = "Heel To Toe Walking";
                break;
            default:
                Exercise="Seated Hamstring Stretch";
                break;

            
        }
        UserData data = SaveUserData.LoadUser();

        fname=data.fname;
        lname=data.lname;
        contactno=data.contactno;
        date=DateTime.Now.ToString("yyyy/MM/dd");
        time= DateTime.Now.ToString("HH:mm:ss");
        SaveData.SaveIntoJson(this);

        SetVideoAddress();
        InvokeRepeating(nameof(updateAWSTable), 5.0f, 5.0f);

        filepath = Application.persistentDataPath + "/SeniorFitDB.s3db";
        conn = "URI=file:" + filepath;
    }

    void Update()
    {
        ShowTime();
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);

        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        if (webCamPermission && CamPermission == true)
        {
            CamPermission = true;
            SceneManager.LoadScene("WorkoutSpace");
        }
    }

    public void SetVideoAddress()
    {
        videoPath = Application.streamingAssetsPath + "/TutorialClips/" + Exercise.Replace(' ', '_').ToLower() + "_tutorial.mp4";
        videoPlayer.url = videoPath;
        videoPlayer.Play();
        time1 = Time.unscaledTime;
    }

    
    public void EnableCamera()
    {        
        bool webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);
        #if PLATFORM_ANDROID
            webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
        #endif

        
        if (webCamPermission)
    {
            SaveData.SaveCameraState(1);
            CamPermission = true;
            SceneManager.LoadScene("WorkoutSpace");
        }

        else
        {
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            webCamPermission = Application.HasUserAuthorization(UserAuthorization.WebCam);

            #if PLATFORM_ANDROID
                Permission.RequestUserPermission(Permission.Camera);
                webCamPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            #endif
        }
    }

     public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ShowTime()
    {
        time2 = Time.unscaledTime;
        Time_duration = (time2 - time1);

        if(timeText is null) timeText = GameObject.Find("Time").GetComponent<Text>();
        if (Time_duration > 0)
        {

            hours = (int)(Time_duration / 3600);
            minutes = ((Time_duration % 3600) / 60);
            seconds = (int)(Time_duration % 60);

            if (hours > 0)
                timeText.text = $"{hours} hrs {(int)minutes} mins {seconds} seconds";
            else if(hours == 0 && minutes > 0)
            {
                timeText.text = $"{(int)minutes} mins {seconds} seconds";

            }
            else
            {
                timeText.text = $"{seconds} seconds";
            }
        }
    }
    
     void updateAWSTable()
    {
        SaveData.SaveIntoJson(this);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet");
        }

        else
        {
            global::ExerciseData newuserlist = SaveData.LoadData();

            foreach (ExerciseInfo newuse in newuserlist.alluserdata)
            {
                Debug.Log("Time duration " + newuse.duration);
                ExerciseData newUser = new ExerciseData
                {
                    FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,
                    ExerciseName = newuse.exercise,
                    date=newuse.date,
                    startTime = newuse.time,
                   
                    UserKey = contactno.ToString() + newuse.date+newuse.time,
                    duration = (int)newuse.duration
                };
                Context.SaveAsync(newUser, (result) =>
                {
                    if (result.Exception == null)
                        Debug.Log("user saved"+ newuse.exercise);
                });

                var request = new DescribeTableRequest
                {
                    TableName = @"ExerciseData"
                };

                // Save/Update contents to SQLite DB
                using (dbconn = new SqliteConnection(conn))
                {
                    
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
                    sqlQuery = string.Format("replace into ExerciseData (Start_Time,Date, Exercise, ElapsedTime, Repcount) values (\"{0} {1}\",\"{0}\",\"{2}\",{3},-1)", newuse.date, newuse.time, newuse.exercise, (int)newuse.duration);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteScalar();
                    dbconn.Close();

                    Debug.Log("exercise Insert Done "+newuse.date);
                }

                
            }
        }
    }
}
