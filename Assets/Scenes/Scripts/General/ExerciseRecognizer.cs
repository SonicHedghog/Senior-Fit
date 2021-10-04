using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Cysharp.Threading.Tasks;
using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;


/// <summary>
/// BlazePose form MediaPipe
/// https://github.com/google/mediapipe
/// https://viz.mediapipe.dev/demo/pose_tracking
/// </summary>
public sealed class ExerciseRecognizer : MonoBehaviour
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
    public enum Mode
    {
        UpperBody,
        FullBody,
    }

    public int exercisenumber;
    public string Exercise;
    public string time;
    public string fname;
    public string lname;
    public int contactno;
    private bool completed = true;
    bool addedExercise = false;
    private Poses.Pose curPose;   
    [SerializeField, FilePopup("*.tflite")] string poseDetectionModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField, FilePopup("*.tflite")] string poseLandmarkModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField] Mode mode = Mode.FullBody;
    [SerializeField] RawImage cameraView = null;
    [SerializeField] bool useLandmarkFilter = true;
    [SerializeField, Range(2f, 30f)] float filterVelocityScale = 10;
    [SerializeField] bool runBackground;
     [SerializeField] private GameObject ResultUI = null;
    [SerializeField] string file = "test";
    public UnityEngine.Video.VideoPlayer videoPlayer;
    private bool menu = false;
    public Text exerciseName;
    public Text resultText;
    [SerializeField, TooltipAttribute("Set FPS of WebCamTexture.")]
    public int requestedFPS;

    private Interpreter script;

    WebCamTexture webcamTexture;
    PoseDetect poseDetect;
    PoseLandmarkDetect poseLandmark;
    private bool isFlipped = false;
    Vector3[] rtCorners = new Vector3[4]; // just cache for GetWorldCorners
    Vector3[] worldJoints;
    PrimitiveDraw draw;
    PoseDetect.Result poseResult;
    PoseLandmarkDetect.Result landmarkResult;
    UniTask<bool> task;
    CancellationToken cancellationToken;
    public int width;
    public int height;

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

    [DynamoDBTable("SeniorFitDemo")]
    public class LoginInfo
    {
       [DynamoDBProperty]
        public string FirstName { get; set; }
        [DynamoDBProperty]
        public string LastName { get; set; }
        [DynamoDBProperty]
        public int ContactNumber { get; set; }
         [DynamoDBProperty]
        public string ExerciseName { get; set; }
        [DynamoDBProperty]
        public string time { get; set; }
        
    }


    //**********************************************************************



    void Start()
    {
        // Init model
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient= AWSConfigs.HttpClientOption.UnityWebRequest;
        string detectionPath = Path.Combine(Application.streamingAssetsPath, poseDetectionModelFile);
        string landmarkPath = Path.Combine(Application.streamingAssetsPath, poseLandmarkModelFile);
        switch (mode)
        {
            case Mode.UpperBody:
                poseDetect = new PoseDetectUpperBody(detectionPath);
                poseLandmark = new PoseLandmarkDetectUpperBody(landmarkPath);
                break;
            case Mode.FullBody:
                poseDetect = new PoseDetectFullBody(detectionPath);
                poseLandmark = new PoseLandmarkDetectFullBody(landmarkPath);
                break;
            default:
                throw new System.NotSupportedException($"Mode: {mode} is not supported");
        }

        

        // Init camera 
        requestedFPS=SceneChange.GetFPS();
        string cameraName = WebCamUtil.FindName();
        webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height,requestedFPS);
        height=Screen.height;
        width= Screen.width;
        
        cameraView.texture = webcamTexture;
        webcamTexture.Play();
        Debug.Log($"Starting camera: {cameraName}");
       
        draw = new PrimitiveDraw(Camera.main, gameObject.layer);
        worldJoints = new Vector3[poseLandmark.JointCount];

        cancellationToken = this.GetCancellationTokenOnDestroy();
        exercisenumber= SceneChange.GetExerciseNumber();
        Debug.Log("EXERCISE NUMBER: "+ exercisenumber);
        switch(exercisenumber)
        {
            case 1:
                Exercise="Seated March";
                script = new Interpreter();
                script.AddCommand("SeatedMarch -1");
                break;
            case 2:
                Exercise="Single Leg Stance";
                script = new Interpreter();
                script.AddCommand("SingleLegStance -1");
                break;
            case 3:
                Exercise="Shoulder Touch";
                script = new Interpreter();
                script.AddCommand("ShoulderTouch -1");
                break;
            case 0:
                script = new Interpreter(file);
                break;
            default:
                break;
        }
        userdata data = SaveUserData.LoadUser();

        fname=data.fname;
        lname=data.lname;
        contactno=data.contactno;
        time= DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
        // AppUse.SaveAppUse(this);

        
        //filenametest="";


        //************sending to dynamodbtable******************
        InvokeRepeating(nameof(updateAWSTable), 5.0f, 5.0f);

         

        /*UnityWebRequest webrequest = UnityWebRequest.Get("https://www.google.com/");
        webrequest.SendWebRequest();

        if (webrequest.error == null)
        {
            //updateAWSTable();
            //return false;
            Debug.Log("No internet");
        }
        else
        {
           
           Debug.LogWarning("No internet connection");
        }*/

        

    }
    
    void updateAWSTable()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("No internet");
            }

        else
        {
            AppUse newuse = AppUse.LoadAppUse();

        LoginInfo newUser = new LoginInfo
        {
            FirstName = newuse.fname,
            LastName=newuse.lname,
            ContactNumber = newuse.contactno,
            ExerciseName= newuse.exercise,
            time=newuse.time
        };
        Context.SaveAsync(newUser, (result) =>
        {
            if (result.Exception == null)
                Debug.Log("user saved");
        });

        var request = new DescribeTableRequest
        {
            TableName = @"SeniorFitDemo"
        };
        Client.DescribeTableAsync(request, (result) =>
        {
            if (result.Exception != null)
            {
               //resultText.text += result.Exception.Message;
               Debug.Log(result.Exception);
                return;
            }
            var response = result.Response;
            TableDescription description = response.Table;
            Debug.Log("Name: " + description.TableName + "\n");
            Debug.Log("# of items: " + description.ItemCount + "\n");
            

        }, null);
        }
        

    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseDetect?.Dispose();
        poseLandmark?.Dispose();
        draw?.Dispose();
    }

   

    void Update()
    {
        if (PauseMenu.GetIsFlipped() != isFlipped)
        {
            isFlipped = !isFlipped;
            webcamTexture.Stop();
            if(!isFlipped)
            {
                webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height);
                Debug.Log("Flipped to " + webcamTexture.deviceName);
                cameraView.texture = webcamTexture;
                webcamTexture.Play();
            }
            else
            {
                try
                {
                    string frontCamName = null;
                    var webCamDevices = WebCamTexture.devices;
                    foreach(var camDevice in webCamDevices){ 
                        if(camDevice.isFrontFacing){
                            frontCamName = camDevice.name;
                            break;
                        }
                    }
                    webcamTexture = new WebCamTexture(frontCamName, width, height);
                    Debug.Log("Flipped to " + webcamTexture.deviceName);
                    cameraView.texture = webcamTexture;
                    webcamTexture.Play();
                }
                catch (Exception e)
                {  
                    webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height); 
                    cameraView.texture = webcamTexture;
                    webcamTexture.Play();
                    Debug.Log(e.Message);
                }
            }
            cameraView.texture = webcamTexture;
        }

        if(PauseMenu.GetIsPaused() && !menu)
        {
            webcamTexture.Pause();
            videoPlayer.Pause();
            menu = true;
        }
        else if(menu && !PauseMenu.GetIsPaused())
        {
            webcamTexture.Play();
            videoPlayer.Play();
            menu = false;
        }

        if(ResultUI!=null) ResultUI.SetActive(true);
        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                task = InvokeAsync();
            }
        }
        else
        {
           //if(this.gameObject!=null)
            Invoke();
        }
        
        if(completed && !script.isDone)
        {
            completed = addedExercise = false;
            curPose = script.AdvanceScript();
            Debug.Log(curPose);

            videoPlayer.url = curPose.GetTutorialAddress();
            videoPlayer.Play();
        }
        else if(completed && script.isDone)
        {
            ResultUI.SetActive(true);
            Time.timeScale = 0;
            webcamTexture.Pause();
            videoPlayer.Pause();
            curPose = null;
            completed = false;
        }

        if(curPose!=null)
        {
            completed = curPose.IsFinished(landmarkResult, exerciseName);

            if (!addedExercise)
            {
                addedExercise = true;
                resultText.text+= exerciseName.text + "\n";
            }
        }


        if (poseResult == null || poseResult.score < 0f) return;
        DrawFrame(poseResult);

        if (landmarkResult == null || landmarkResult.score < 0.2f) return;
        DrawCropMatrix(poseLandmark.CropMatrix);
        DrawJoints(landmarkResult.joints);
    }

    void DrawFrame(PoseDetect.Result pose)
    {
        Vector3 min = rtCorners[0];
        
        Vector3 max = rtCorners[2];
         
        draw.color = Color.green;
        draw.Rect(MathTF.Lerp(min, max, pose.rect, true), 0.2f, min.z);

        foreach (var kp in pose.keypoints)
        {
            draw.Point(MathTF.Lerp(min, max, (Vector3)kp, true), 0.5f);
        }
        draw.Apply();
    }

    void DrawCropMatrix(in Matrix4x4 matrix)
    {
        draw.color = Color.red;

        Vector3 min = rtCorners[0];
        Vector3 max = rtCorners[2];

        var mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, false, webcamTexture.videoVerticallyMirrored)
            * matrix.inverse;
        Vector3 a = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(0, 0, 0)));
        Vector3 b = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(1, 0, 0)));
        Vector3 c = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(1, 1, 0)));
        Vector3 d = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(0, 1, 0)));

        draw.Quad(a, b, c, d, 0.2f);
        draw.Apply();
    }

    void DrawJoints(Vector3[] joints)
    {
        // Apply webcam rotation to draw landmarks correctly
        Matrix4x4 mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, false, webcamTexture.videoVerticallyMirrored);
        Vector3 min = rtCorners[0];
        Vector3 max = rtCorners[2];

        draw.color = Color.blue;

        // Update world joints
        for (int i = 0; i < joints.Length; i++)
        {
            var p = mtx.MultiplyPoint3x4(joints[i]);
            p = MathTF.Lerp(min, max, p);
            worldJoints[i] = p;
        }

        // Draw
        for (int i = 0; i < worldJoints.Length; i++)
        {
            draw.Cube(worldJoints[i], 0.2f);
        }
        var connections = poseLandmark.Connections;
        for (int i = 0; i < connections.Length; i += 2)
        {
            draw.Line3D(
                worldJoints[connections[i]],
                worldJoints[connections[i + 1]],
                0.5f);
        }
        draw.Apply();
    }

    void Invoke()
    {
        poseDetect.Invoke(webcamTexture);
        cameraView.material = poseDetect.transformMat;
        cameraView.rectTransform.GetWorldCorners(rtCorners);

        poseResult = poseDetect.GetResults(0.7f, 0.3f);
        if (poseResult.score < 0) return;

        poseLandmark.Invoke(webcamTexture, poseResult);

        if (useLandmarkFilter)
        {
            poseLandmark.FilterVelocityScale = filterVelocityScale;
        }
        landmarkResult = poseLandmark.GetResult(useLandmarkFilter);

    }

    async UniTask<bool> InvokeAsync()
    {
        // Note: `await` changes PlayerLoopTiming from Update to FixedUpdate.
        poseResult = await poseDetect.InvokeAsync(webcamTexture, cancellationToken, PlayerLoopTiming.FixedUpdate);

        if (poseResult.score < 0) return false;

        if (useLandmarkFilter)
        {
            poseLandmark.FilterVelocityScale = filterVelocityScale;
        }
        landmarkResult = await poseLandmark.InvokeAsync(webcamTexture, poseResult, useLandmarkFilter, cancellationToken, PlayerLoopTiming.Update);

        // Back to the update timing from now on 
        if (cameraView != null)
        {
            cameraView.material = poseDetect.transformMat;
            cameraView.rectTransform.GetWorldCorners(rtCorners);
        }

        return true;
    }

    
}
