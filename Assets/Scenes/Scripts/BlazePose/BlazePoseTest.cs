using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// BlazePose form MediaPipe
/// https://github.com/google/mediapipe
/// https://viz.mediapipe.dev/demo/pose_tracking
/// </summary>
public sealed class BlazePoseTest : MonoBehaviour
{

    public enum Mode
    {
        UpperBody,
        FullBody,
    }

    [SerializeField, FilePopup("*.tflite")] string poseDetectionModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField, FilePopup("*.tflite")] string poseLandmarkModelFile = "coco_ssd_mobilenet_quant.tflite";
    [SerializeField] Mode mode = Mode.FullBody;
    [SerializeField] RawImage cameraView = null;
   // [SerializeField] RawImage debugView = null;
    [SerializeField] bool useLandmarkFilter = true;
    [SerializeField] Vector3 filterVelocityScale = Vector3.one * 10;
    [SerializeField] bool runBackground;
     [SerializeField] private GameObject ResultUI = null;
    public Text exerciseName;
    public Text resultText;

    [SerializeField] string file = "test";

    PoseDetect poseDetect;
    PoseLandmarkDetect poseLandmark;

    Vector3[] rtCorners = new Vector3[4]; // just cache for GetWorldCorners
    Vector4[] worldJoints;
    private Interpreter script;
    UnityEngine.Video.VideoPlayer videoPlayer;
    PrimitiveDraw draw;
    PoseDetect.Result poseResult;
    PoseLandmarkDetect.Result landmarkResult;
    UniTask<bool> task;
    CancellationToken cancellationToken;
    public string filename="SeatedMarch";
    PoseClassifierProcessor processor;
    
    bool start = false;
    string curCommand;
    

    void Start()
    {
        GameObject camera = GameObject.Find("Main Camera");

        // Init model
        string detectionPath = Path.Combine(Application.streamingAssetsPath, poseDetectionModelFile);
        string landmarkPath = Path.Combine(Application.streamingAssetsPath, poseLandmarkModelFile);
        
        // Init model
        poseDetect = new PoseDetect(poseDetectionModelFile);
        poseLandmark = new PoseLandmarkDetect(poseLandmarkModelFile);

        // Init camera 
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.isLooping = true;
        videoPlayer.url = Application.platform == RuntimePlatform.Android 
                            ? Path.Combine(Application.streamingAssetsPath, "WIN_20210414_01_23_11_Pro.mp4")
                            : Path.Combine(Application.streamingAssetsPath, "WIN_20210414_01_23_11_Pro.mp4");
        
       
        draw = new PrimitiveDraw(Camera.main, gameObject.layer);
        worldJoints = new Vector4[PoseLandmarkDetect.JointCount];

        cancellationToken = this.GetCancellationTokenOnDestroy();

        try{
            script = new Interpreter(file);
        }
        catch(Exception e)
        {
            exerciseName.text =e.Message;
            exerciseName.fontSize = 20;
        }

        curCommand = script.GetCommand();
        // Set up Pose Classifier Processor
        processor = new PoseClassifierProcessor(curCommand.Split(' ')[0], true);
    }

    void OnDestroy()
    {
        videoPlayer?.Stop();
        poseDetect?.Dispose();
        poseLandmark?.Dispose();
        draw?.Dispose();
    }

    int exit = 0;
    int enter = 0;

    void Update()
    {
        if(Test.start)
        {
            if(exit < 10)
            {
                if(enter < 11)
                {
                    trainOnVideo();
                }
            }
            else
            {

                if(curCommand != "last")
                    processor = new PoseClassifierProcessor(filename, true);
                else
                {
                    Test.start = false;
                }
            }
        }
    }

    void trainOnVideo()
    {
        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                task = InvokeAsync();
            }
        }
        else
        {
            Invoke();
        }

        if (poseResult == null || poseResult.score < 0f) return;

        if (landmarkResult == null || landmarkResult.score < 0.2f) return;

        List<string> poses = processor.getPoseResult(landmarkResult);
        foreach(string s in poses)
        {
            Debug.Log(s);
        }
        
        resultText.text =poses[0];
        exerciseName.text =poses[1];

    }

    
    void Invoke()
    {
        poseDetect.Invoke(videoPlayer.texture);
        cameraView.material = poseDetect.transformMat;
        cameraView.rectTransform.GetWorldCorners(rtCorners);

        poseResult = poseDetect.GetResults(0.7f, 0.3f);
        if (poseResult.score < 0) return;

        poseLandmark.Invoke(videoPlayer.texture, poseResult);
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
        poseResult = await poseDetect.InvokeAsync(videoPlayer.texture, cancellationToken, PlayerLoopTiming.FixedUpdate);

        if (poseResult.score < 0) return false;

        if (useLandmarkFilter)
        {
            poseLandmark.FilterVelocityScale = filterVelocityScale;
        }
        landmarkResult = await poseLandmark.InvokeAsync(videoPlayer.texture, poseResult, useLandmarkFilter, cancellationToken, PlayerLoopTiming.Update);

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
