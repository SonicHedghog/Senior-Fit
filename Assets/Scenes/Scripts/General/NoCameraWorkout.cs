using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
public class NoCameraWorkout : MonoBehaviour
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

    public int exercisenumber;
    public string Exercise;
    public string videoPath;
     public UnityEngine.Video.VideoPlayer videoPlayer;
     public Text timeText;
     public float time1 = 0, time2 = 0, Time_duration=0,new_duration=0,pauseTime1=0,pauseTime2=0;
      public float hours,minutes, seconds;
    public string date,time;
    public string fname;
    public string lname;
    public long contactno;

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


    //**********************************************************************



    // Start is called before the first frame update
    void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient= AWSConfigs.HttpClientOption.UnityWebRequest;
         Screen.orientation = ScreenOrientation.LandscapeLeft;
        exercisenumber=SceneChange.GetExerciseNumber();
        switch(exercisenumber)
        {
            case 1:
                Exercise="Seated March";
                //script = new Interpreter();
                //script.AddCommand("SeatedMarch -1");
                break;
            case 2:
                Exercise="Single Leg Stance";
                //script = new Interpreter();
                //script.AddCommand("SingleLegStance -1");
                break;
            case 3:
                Exercise="Shoulder Touch";
                //script = new Interpreter();
                //script.AddCommand("ShoulderTouch -1");
                break;
            case 4:
                Exercise="Chair Sit To Stand";
                //script = new Interpreter();
                //script.AddCommand("ChairSitToStand -1");
                break;
            case 5:
                Exercise="Marching In Place";
                //script = new Interpreter();
                //script.AddCommand("MarchingInPlace -1");
                break;

            case 6:
                Exercise="Seated Hamstring Stretch";
                //script = new Interpreter();
                //script.AddCommand("SeatedHamstringStretch -1");
                break;

            default:
                Exercise="Seated Hamstring Stretch";
                break;

            
        }
        userdata data = SaveUserData.LoadUser();

        fname=data.fname;
        lname=data.lname;
        contactno=data.contactno;
        date=DateTime.Now.ToString("yyyy/MM/dd");
        time= DateTime.Now.ToString("HH:mm:ss");
        SaveData.SaveIntoJson(this);

        SetVideoAddress();
        InvokeRepeating(nameof(updateAWSTable), 5.0f, 5.0f);
    }

    public void SetVideoAddress()
    {
        videoPath= Application.streamingAssetsPath + "/TutorialClips/" + Exercise.Replace(' ', '_').ToLower() + "_tutorial.mp4";
        videoPlayer.url =   videoPath;
        videoPlayer.Play();
        time1 = Time.unscaledTime;
    }

    public void EnableCamera()
    {
        SceneChange.LoadWorkoutScene();
    }

     public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ShowTime()
    {
        time2 = Time.unscaledTime;
        Time_duration=(time2-time1);

        if(timeText is null) timeText = GameObject.Find("Time").GetComponent<Text>();
        if (Time_duration > 0)
        {

            hours = (int)(Time_duration / 3600);
            minutes = ((Time_duration % 3600) / 60);
            seconds = (int)(Time_duration % 60);

            if (hours > 0)
                timeText.text = $"{hours} hrs {(int)minutes} mins {seconds} seconds";
            else if(hours==0 && minutes>0)
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
            UserList newuserlist = SaveData.LoadData();

            foreach (UserData newuse in newuserlist.alluserdata)
            {
                //AppUse newuse = AppUse.LoadAppUse();
                Debug.Log("Time duration "+newuse.duration);
                ExerciseData newUser = new ExerciseData
                {
                    FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,
                    ExerciseName = newuse.exercise,
                    date=newuse.date,
                    startTime = newuse.time,
                   
                    UserKey = contactno.ToString()+newuse.date+newuse.time,
                    duration=(int)newuse.duration
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
                

            }

        }


    }
    // Update is called once per frame
    void Update()
    {
        ShowTime();
    }
}
