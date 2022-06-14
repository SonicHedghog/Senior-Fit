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
    public string date,time,repCount="",newrepcount;
    public string fname;
    public string lname;
    public long contactno;
    private bool completed = true;
    bool addedExercise = false;
    private Poses.Pose curPose;   
    [SerializeField, FilePopup("*.tflite")] string poseDetectionModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField, FilePopup("*.tflite")] string poseLandmarkModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField] Mode mode = Mode.FullBody;
    [SerializeField] RawImage cameraView = null;
    [SerializeField] bool useLandmarkFilter = true;
    [SerializeField] Vector3 filterVelocityScale = Vector3.one * 10;
    [SerializeField] bool runBackground;
    [SerializeField] Canvas canvas = null;
    [SerializeField] private bool _drawStickFigure = true;
    [SerializeField] private GameObject ResultUI = null;
    [SerializeField] string file = "test";
    public UnityEngine.Video.VideoPlayer videoPlayer;
    private bool menu = false;
    public Text exerciseName;
    public Text timeText;
    public Text resultText;
    [SerializeField, TooltipAttribute("Set FPS of WebCamTexture.")]
    public int requestedFPS;
    public float time1 = 0, time2 = 0, Time_duration = 0, new_duration = 0, pauseTime1 = 0, pauseTime2 = 0;
    public float hours,minutes, seconds;
    private Interpreter script;

    [SerializeField, Range(0f, 1f)] float visibilityThreshold = 0.5f;

    WebCamTexture webcamTexture;
    PoseDetect poseDetect;
    PoseLandmarkDetect poseLandmark;
    private bool isFlipped = false;
    Vector3[] rtCorners = new Vector3[4]; // just cache for GetWorldCorners
    Vector4[] worldJoints;
    PrimitiveDraw draw;
    PoseDetect.Result poseResult;
    PoseLandmarkDetect.Result landmarkResult;
    UniTask<bool> task;
    CancellationToken cancellationToken;
    public int width;
    public int height;
    public bool nameSet = false;

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


    }


    //**********************************************************************



    void Start()
    {
        // Init model
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient= AWSConfigs.HttpClientOption.UnityWebRequest;
        string detectionPath = Path.Combine(Application.streamingAssetsPath, poseDetectionModelFile);
        string landmarkPath = Path.Combine(Application.streamingAssetsPath, poseLandmarkModelFile);
         
        // Init model
        poseDetect = new PoseDetect(poseDetectionModelFile);
        poseLandmark = new PoseLandmarkDetect(poseLandmarkModelFile);
        

        // Init camera 
        requestedFPS=SceneChange.GetFPS();
        try{

           
            string frontCamName = null;
            var webCamDevices = WebCamTexture.devices;
            foreach(var camDevice in webCamDevices){ 
                if(camDevice.isFrontFacing){
                    frontCamName = camDevice.name;
                    break;
                }
            }

            height=Screen.height;
            width= Screen.width;
            webcamTexture = new WebCamTexture(frontCamName, width, height, requestedFPS);
            
            
            cameraView.texture = webcamTexture;
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                cameraView.transform.Rotate(new Vector3(0f,180f,0f), Space.Self);
            }
            webcamTexture.Play();
            Debug.Log($"Starting camera: {frontCamName}");
            
        }
        catch (Exception e)
        {  
            webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height,requestedFPS); 
            cameraView.texture = webcamTexture;
            webcamTexture.Play();
            Debug.Log(e.Message);
        }
       
        draw = new PrimitiveDraw(Camera.main, gameObject.layer);
        worldJoints = new Vector4[PoseLandmarkDetect.JointCount];

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
            case 4:
                Exercise="Chair Sit to Stand";
                script = new Interpreter();
                script.AddCommand("ChairSitToStand -1");
                break;
            case 5:
                Exercise="Marching in Place";
                script = new Interpreter();
                script.AddCommand("MarchingInPlace -1");
                break;

            case 6:
                Exercise="Seated Hamstring Stretch";
                script = new Interpreter();
                script.AddCommand("SeatedHamstringStretch -1");
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
        date=DateTime.Now.ToString("yyyy/MM/dd");
        time= DateTime.Now.ToString("HH:mm:ss");
        SaveData.SaveIntoJson(this);
       


        
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

                ExerciseData newUser = new ExerciseData
                {
                    FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,
                    ExerciseName = newuse.exercise,
                    date=newuse.date,
                    startTime = newuse.time,
                    repCount=newuse.repCount,
                    UserKey = contactno.ToString()+newuse.date+newuse.time
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
                /*Client.DescribeTableAsync(request, (result) =>
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


                }, null);*/

            }

        }


    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseDetect?.Dispose();
        poseLandmark?.Dispose();
        draw?.Dispose();
    }

   public void ShowTime()
    {
        time2 = Time.unscaledTime;
        Time_duration=(time2-time1)-new_duration;

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

    void Update()
    {
        if(exerciseName.text != "Exercise Name" && !nameSet)
        {
            time1 = Time.unscaledTime;
            nameSet = true;
        }
        else if(exerciseName.text == "Excercise Name") nameSet = false;

        string path = Application.persistentDataPath + "/RepCount.txt";
        if (File.Exists(path))
        {
            newrepcount = File.ReadAllText(path);
        }

        if(repCount!=newrepcount)
        {
            repCount=newrepcount;
            SaveData.SaveIntoJson(this);
        }

        if (PauseMenu.GetIsFlipped() != isFlipped)
        {
            isFlipped = !isFlipped;
            webcamTexture.Stop();
            if(isFlipped)
            {
                webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height);
                Debug.Log("Flipped to " + webcamTexture.deviceName);
                cameraView.texture = webcamTexture;
                if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    cameraView.transform.Rotate(new Vector3(0f,180f,0f), Space.Self);
                }
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
                    if(Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        cameraView.transform.Rotate(new Vector3(0f,180f,0f), Space.Self);
                    }
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
            pauseTime1=Time.unscaledTime;
            Debug.Log("puz: " + pauseTime1 + " " + pauseTime2);
        }
        else if(menu && !PauseMenu.GetIsPaused())
        {
            webcamTexture.Play();
            videoPlayer.Play();
            menu = false;
            pauseTime2=Time.unscaledTime;
            new_duration=new_duration+(pauseTime2-pauseTime1);
            Debug.Log("puz: " + pauseTime1 + " " + pauseTime2);
        }
        if(!PauseMenu.GetIsPaused() && exerciseName.text != "Exercise Name") ShowTime();

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

    void DrawJoints(Vector4[] joints)
    {
        draw.color = Color.blue;

        Vector3 min = rtCorners[0];
        Vector3 max = rtCorners[2];
        Debug.Log($"rtCorners min: {min}, max: {max}");

        // Apply webcam rotation to draw landmarks correctly
        Matrix4x4 mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, false, webcamTexture.videoVerticallyMirrored);
        if(Application.platform == RuntimePlatform.IPhonePlayer && !isFlipped) 
        {
            mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, true, webcamTexture.videoVerticallyMirrored);
        }

        float zScale = (max.x - min.x) / 2;
        // float zScale = 1;
        float zOffset = canvas.planeDistance;
        float aspect = (float)Screen.width / (float)Screen.height;
        Vector3 scale, offset;
        if (aspect > 1)
        {
            scale = new Vector3(1f / aspect, 1f, zScale);
            offset = new Vector3((1 - 1f / aspect) / 2, 0, zOffset);
        }
        else
        {
            scale = new Vector3(1f, aspect, zScale);
            offset = new Vector3(0, (1 - aspect) / 2, zOffset);
        }

        // Update world joints
        var camera = canvas.worldCamera;
        for (int i = 0; i < joints.Length; i++)
        {
            Vector3 p = mtx.MultiplyPoint3x4((Vector3)joints[i]);
            // p = Vector3.Scale(p, scale) + offset;
            p = MathTF.Lerp(min, max, p);

            // w is visibility
            worldJoints[i] = new Vector4(p.x, p.y, p.z, joints[i].w);
        }

       

		// Draw
		if (_drawStickFigure){
        for (int i = 0; i < worldJoints.Length; i++)
        {
            Vector4 p = worldJoints[i];
            if (p.w > visibilityThreshold)
            {
                draw.Cube(p, 0.2f);
            }
        }
        var connections = PoseLandmarkDetect.Connections;
        for (int i = 0; i < connections.Length; i += 2)
        {
            var a = worldJoints[connections[i]];
            var b = worldJoints[connections[i + 1]];
            if (a.w > visibilityThreshold || b.w > visibilityThreshold)
            {
                draw.Line3D(a, b, 0.5f);
            }
        }
		}
        draw.Apply();
    }

    void Invoke()
    {
        poseDetect.Invoke(webcamTexture);
        cameraView.material = poseDetect.transformMat;
        cameraView.rectTransform.GetWorldCorners(rtCorners);
        Debug.Log("The World: " + rtCorners[0].x + " "  + rtCorners[3].x);
        if(Application.platform == RuntimePlatform.IPhonePlayer && !isFlipped)
        {
            Vector3 pivot = new Vector3(0f,0f,90f);
 
            rtCorners[0] = (Quaternion.Euler(new Vector3(0f, 180f, 0)) * (rtCorners[0] - pivot)) + pivot;
            rtCorners[1] = (Quaternion.Euler(new Vector3(0f, 180f, 0)) * (rtCorners[1] - pivot)) + pivot;
            rtCorners[2] = (Quaternion.Euler(new Vector3(0f, 180f, 0)) * (rtCorners[2] - pivot)) + pivot;
            rtCorners[3] = (Quaternion.Euler(new Vector3(0f, 180f, 0)) * (rtCorners[3] - pivot)) + pivot;
        }

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
