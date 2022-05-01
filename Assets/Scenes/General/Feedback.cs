using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
using System;
using UnityEngine.SceneManagement;

public class Feedback : MonoBehaviour
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
    public InputField feedbackInput;

    public string fname,lname;
    public long contactNo;
    public string currentDate,currentTime;
    public string feedback;



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

    [DynamoDBTable("UserFeedback")]
    public class UserFeedback
    {
        [DynamoDBProperty]
        public string userkey { get; set; }
        [DynamoDBProperty]
        public string FirstName { get; set; }
        [DynamoDBProperty]
        public string LastName { get; set; }
        [DynamoDBProperty]
        public long ContactNumber { get; set; }
        
        [DynamoDBProperty]
        public string Date { get; set; }

        [DynamoDBProperty]
        public string Time { get; set; }
        [DynamoDBProperty]
        public string Feedback { get; set; }


       



    }


    //**********************************************************************

    // Start is called before the first frame update
    void Start()
    {
         UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactNo = data.contactno;
        currentDate = DateTime.Now.ToString("yyyy/MM/dd");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SubmitFeedback()
    {
        feedback=feedbackInput.text;
        currentTime = DateTime.Now.ToString("HH:mm:ss");
        UpdateAWSinfo();
        SceneManager.LoadScene("MainMenu");
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
           

                UserFeedback newFeedback = new UserFeedback
                {


                    //latitudeData=newuse.latitude

                    FirstName = fname,
                    LastName = lname,
                    ContactNumber = contactNo,
                    
                    userkey = contactNo + currentDate +currentTime,
                    Date = currentDate,
                    Time = currentTime,
                    Feedback=feedback

                };
                Context.SaveAsync(newFeedback, (result) =>
                {
                    if (result.Exception == null)
                        Debug.Log("Feedback saved");
                });

                var request = new DescribeTableRequest
                {
                    TableName = @"UserFeedback"
                };


            }


        }


    
}
