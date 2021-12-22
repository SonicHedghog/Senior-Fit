using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


/// <summary>
/// BlazePose form MediaPipe
/// https://github.com/google/mediapipe
/// https://viz.mediapipe.dev/demo/pose_tracking
/// </summary>
public sealed class BlazePoseRunner : MonoBehaviour
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
    public long contactno;

    [SerializeField, FilePopup("*.tflite")] string poseDetectionModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField, FilePopup("*.tflite")] string poseLandmarkModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField] Mode mode = Mode.FullBody;
    [SerializeField] RawImage cameraView = null;
    // [SerializeField] RawImage debugView = null;
    [SerializeField] bool useLandmarkFilter = true;
    [SerializeField] Vector3 filterVelocityScale = Vector3.one * 10;
    [SerializeField] bool runBackground;
    [SerializeField] private GameObject ResultUI = null;
    [SerializeField] Canvas canvas = null;
    [SerializeField] private bool _drawStickFigure = true;
    public Text exerciseName;
    public Text resultText;
    [SerializeField, TooltipAttribute("Set FPS of WebCamTexture.")]
    public int requestedFPS;

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
    public string filename;

    public string filenametest;

    // public string filenametest="Sit_To_Stand";
    PoseClassifierProcessor processor;
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

    [DynamoDBTable("SeniorFitDemo")]
    public class LoginInfo
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
        public string time { get; set; }

    }


    //**********************************************************************



    void Start()
    {
        // Init model
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        string detectionPath = Path.Combine(Application.streamingAssetsPath, poseDetectionModelFile);
        string landmarkPath = Path.Combine(Application.streamingAssetsPath, poseLandmarkModelFile);
        
        // Init model
        poseDetect = new PoseDetect(poseDetectionModelFile);
        poseLandmark = new PoseLandmarkDetect(poseLandmarkModelFile);

        // Init camera 
        requestedFPS = SceneChange.GetFPS();
        string cameraName = WebCamUtil.FindName();
        webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height, requestedFPS);
        height = Screen.height;
        width = Screen.width;

        cameraView.texture = webcamTexture;
        webcamTexture.Play();
        Debug.Log($"Starting camera: {cameraName}");

        draw = new PrimitiveDraw(Camera.main, gameObject.layer);
        worldJoints = new Vector4[PoseLandmarkDetect.JointCount];

        cancellationToken = this.GetCancellationTokenOnDestroy();
        exercisenumber = SceneChange.GetExerciseNumber();
        Debug.Log("EXERCISE NUMBER: " + exercisenumber);
        switch (exercisenumber)
        {
            case 1:
                filenametest = "Seated_March_CSV";
                Exercise = "Seated March";
                break;
            case 2:
                filenametest = "Single_Leg_Stance";
                Exercise = "Single Leg Stance";
                break;
            case 3:
                filenametest = "Shoulder_touch";
                Exercise = "Shoulder Touch";
                break;
            default:
                break;
        }
        userdata data = SaveUserData.LoadUser();

        fname = data.fname;
        lname = data.lname;
        contactno = data.contactno;
        time = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
        SaveData.SaveBlazePoseRunnerData(this);

        //AppUse.SaveAppUse(this);


        //filenametest="new_SeatedMarch";


        // Set up Pose Classifier Processor
        processor = new PoseClassifierProcessor(filenametest, true);

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

                LoginInfo newUser = new LoginInfo
                {
                    FirstName = newuse.fname,
                    LastName = newuse.lname,
                    ContactNumber = newuse.contactno,
                    ExerciseName = newuse.exercise,
                    time = newuse.time,
                    UserKey = contactno.ToString()+newuse.time
                };
                Context.SaveAsync(newUser, (result) =>
                {
                    if (result.Exception == null)
                        Debug.Log("user saved"+ newuse.exercise);
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



        if (SceneChange.GetIsFlipped() != isFlipped)
        {
            isFlipped = !isFlipped;
            webcamTexture.Stop();
            if (!isFlipped)
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
                    foreach (var camDevice in webCamDevices)
                    {
                        if (camDevice.isFrontFacing)
                        {
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

        if (ResultUI != null) ResultUI.SetActive(true);
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

        if (poseResult == null || poseResult.score < 0f) return;
        DrawFrame(poseResult);

        if (landmarkResult == null || landmarkResult.score < 0.2f) return;
        DrawCropMatrix(poseLandmark.CropMatrix);
        DrawJoints(landmarkResult.joints);




        List<string> poses = processor.getPoseResult(landmarkResult);
        foreach (string s in poses)
        {
            Debug.Log(s);
        }

        resultText.text = poses[0];
        exerciseName.text = poses[1];

    }

    void DrawFrame(PoseDetect.Result pose)
    {
        Vector3 min = rtCorners[0];

        Vector3 max = rtCorners[2];

        draw.color = Color.green;
        draw.Rect(MathTF.Lerp(min, max, pose.rect, true), 0.02f, min.z);

        foreach (var kp in pose.keypoints)
        {
            draw.Point(MathTF.Lerp(min, max, (Vector3)kp, true), 0.05f);
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

        draw.Quad(a, b, c, d, 0.02f);
        draw.Apply();
    }

    void DrawJoints(Vector4[] joints)
    {
        draw.color = Color.blue;

        // Vector3 min = rtCorners[0];
        // Vector3 max = rtCorners[2];
        // Debug.Log($"rtCorners min: {min}, max: {max}");

        // Apply webcam rotation to draw landmarks correctly
        Matrix4x4 mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, false, webcamTexture.videoVerticallyMirrored);

        // float zScale = (max.x - min.x) / 2;
        float zScale = 1;
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
            p = Vector3.Scale(p, scale) + offset;
            p = camera.ViewportToWorldPoint(p);

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
                draw.Line3D(a, b, 0.05f);
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

        poseResult = poseDetect.GetResults(0.7f, 0.3f);
        if (poseResult.score < 0) return;

        poseLandmark.Invoke(webcamTexture, poseResult);
        //  debugView.texture = poseLandmark.inputTex;

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
        /* if (debugView != null)
         {
             debugView.texture = poseLandmark.inputTex;
         }*/

        return true;
    }


}
