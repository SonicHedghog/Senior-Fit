﻿using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Poses;

public class PoseRocgnizer : MonoBehaviour
{
    [SerializeField, FilePopup("*.tflite")] string fileName = "posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite";
    [SerializeField] RawImage cameraView = null;
    [SerializeField, Range(0f, 1f)] float threshold = 0.5f;
    [SerializeField, Range(0f, 1f)] float lineThickness = 0.5f;
    private bool menu = false;
    private bool completed = false;
    private Poses.Pose curPose;
    private Interpreter script;
    WebCamTexture webcamTexture;
    PoseNet poseNet;
    Vector3[] corners = new Vector3[4];
    PrimitiveDraw draw;

    public Text exerciseName;

    public PoseNet.Result[] results;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        poseNet = new PoseNet(path);

        // Init camera
        string cameraName = WebCamUtil.FindName();
        webcamTexture = new WebCamTexture(cameraName, 1280, 720, 30);
        webcamTexture.Play();
        cameraView.texture = webcamTexture;

        draw = new PrimitiveDraw()
        {
            color = Color.green,
        };

        script = new Interpreter("test");
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseNet?.Dispose();
        draw?.Dispose();
    }

    void Update()
    {
        poseNet.Invoke(webcamTexture);
        results = poseNet.GetResults();

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

        cameraView.material = poseNet.transformMat;
        // cameraView.texture = poseNet.inputTex;

        DrawResult();

        if(completed && !script.isDone)
        {
            completed = false;
            curPose = script.AdvanceScript();
        }

        if(curPose!=null)
        {
            // Debug.Log("Yes");
            completed = curPose.IsFinished(results, exerciseName);
        }
        else if(!completed)
        {
            // string commmand = script.GetCommand();
            // Debug.Log(commmand);
            completed = true;
        }
        Debug.Log(curPose);
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
