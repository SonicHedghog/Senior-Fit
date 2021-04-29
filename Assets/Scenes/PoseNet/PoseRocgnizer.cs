using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Poses;

public class PoseRocgnizer : MonoBehaviour
{
    [SerializeField, FilePopup("*.tflite")] string fileName;
    [SerializeField] RawImage cameraView = null;
    [SerializeField, Range(0f, 1f)] float threshold = 0.5f;
    [SerializeField, Range(0f, 1f)] float lineThickness = 0.5f;
    [SerializeField] string file = "test";
    [SerializeField] private GameObject ResultUI = null;

    private bool menu = false;
    private bool completed = true;
    private Poses.Pose curPose;
    private Interpreter script;
    WebCamTexture webcamTexture;
    PoseNet poseNet;
    Vector3[] corners = new Vector3[4];
    PrimitiveDraw draw;

    public Text exerciseName;
    public Text resultText;

    public PoseNet.Result[] results;

    private bool isFlipped = false;
    private bool higherAccuracy = false;

    void Start()
    {
        fileName = !isFlipped ? "posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite"
                            : "posenet_mobilenet_v1_100_513x513_multi_kpt_stripped.tflite";

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        poseNet = new PoseNet(path);

        // Init camera
        string cameraName = WebCamUtil.FindName();
        Debug.Log(cameraName);
        webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height);
        webcamTexture.Play();
        cameraView.texture = webcamTexture;

        draw = new PrimitiveDraw()
        {
            color = Color.green,
        };

        try
        {
            script = new Interpreter(file);
        }
        catch(Exception e)
        {
            exerciseName.text =e.Message;
            exerciseName.fontSize = 20;
        }
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseNet?.Dispose();
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
            }
            else
            {
                try
                {
                    webcamTexture = new WebCamTexture(WebCamTexture.devices[1].name, Screen.width, Screen.height);
                    Debug.Log("Flipped to " + webcamTexture.deviceName);
                }
                catch (Exception e)
                {  
                    webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height); 
                    Debug.Log(e.Message);
                }
            }
            cameraView.texture = webcamTexture;
        }

        if (PauseMenu.GetAccuracyLevel() != higherAccuracy)
        {
            higherAccuracy = !higherAccuracy;

            fileName = !higherAccuracy ? "posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite" 
                                        : "posenet_mobilenet_v1_100_513x513_multi_kpt_stripped.tflite";

            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            poseNet = new PoseNet(path);
        }

        if(PauseMenu.GetIsPaused() && !menu)
        {
            webcamTexture.Pause();
            menu = true;
        }
        else if(menu && !PauseMenu.GetIsPaused())
        {
            webcamTexture.Play();
            menu = false;
        }

        poseNet.Invoke(webcamTexture);
        results = poseNet.GetResults();

        cameraView.material = poseNet.transformMat;
        // cameraView.texture = poseNet.inputTex;

        DrawResult();

        if(completed && !script.isDone)
        {
            completed = false;
            curPose = script.AdvanceScript();
            resultText.text+= exerciseName.text + "\n";
            Debug.Log(curPose);
        }
        else if(completed && script.isDone)
        {
            resultText.text+= exerciseName.text;
            resultText.text = resultText.text.Replace("Exercise Name\n","");
            ResultUI.SetActive(true);
            Time.timeScale = 0;
            webcamTexture.Pause();
            curPose = null;
            completed = false;
        }

        if(curPose!=null)
        {
            // Debug.Log("Yes");
            completed = curPose.IsFinished(results, exerciseName);
        }
        // else if(!completed)
        // {
        //     // string commmand = script.GetCommand();
        //     // Debug.Log(commmand);
        //     completed = true;
        // }
    }

    void DrawResult()
    {
        var rect = cameraView.GetComponent<RectTransform>();
        rect.GetWorldCorners(corners);
        Vector3 min = corners[0];
        Vector3 max = corners[2];

        var connections = PoseNet.Connections;
        int len = connections.GetLength(0);
        for (int i = 0; i < len; i++)
        {
            var a = results[(int)connections[i, 0]];
            var b = results[(int)connections[i, 1]];
            if (a.confidence >= threshold && b.confidence >= threshold)
            {
                draw.Line3D(
                    MathTF.Lerp(min, max, new Vector3(a.x, 1f - a.y, 0)),
                    MathTF.Lerp(min, max, new Vector3(b.x, 1f - b.y, 0)),
                    lineThickness
                );
            }
        }

        draw.Apply();
    }
}
