using UnityEngine;
using TensorFlowLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Linq;
using System.IO;

namespace TensorFlowLite 
{
  public class PoseClassifierProcessor {
    private static readonly String TAG = "PoseClassifierProcessor";
    private static readonly String POSE_SAMPLES_FILE = "pose/fitness_pose_samples.csv";

    // Specify classes for which we want rep counting.
    // These are the labels in the given {@code POSE_SAMPLES_FILE}. You can set your own class labels
    // for your pose samples.
    private static readonly String PUSHUPS_CLASS = "seated_march_left";
    private static readonly String SQUATS_CLASS = "seated_march_right";
    private static readonly String[] POSE_CLASSES = {
      PUSHUPS_CLASS, SQUATS_CLASS
    };

    private readonly bool isStreamMode;

    private EMASmoothing emaSmoothing;
    private List<RepetitionCounter> repCounters;
    private PoseClassifier poseClassifier;
    private String lastRepResult;

    //@WorkerThread
    public PoseClassifierProcessor(string filename, bool isStreamMode) {
      // Preconditions.checkState(Looper.myLooper() != Looper.getMainLooper());
      this.isStreamMode = isStreamMode;
      if (isStreamMode) {
        emaSmoothing = new EMASmoothing();
        repCounters = new List<RepetitionCounter>();
        lastRepResult = "";
      }
      loadPoseSamples(filename);
    }

    private void loadPoseSamples(string filename) {
      string[] paths = {Application.streamingAssetsPath, "Samples", filename + ".csv"};
      List<string> fileLines;
      List<PoseSample> poseSamples = new List<PoseSample>();
      try 
      {
        if(Application.platform == RuntimePlatform.Android)
          {
              var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
              www.SendWebRequest();
              while (!www.isDone)
              {
              }
              fileLines = www.downloadHandler.text.Split('\n').ToList();
              Debug.Log(www.downloadHandler.text);
          }
          else
          {
              fileLines = File.ReadAllLines(Application.streamingAssetsPath + "/Samples/" + filename + ".csv").ToList();
          }
    
        foreach (string line in fileLines) 
        {
          // If line is not a valid {@link PoseSample}, we'll get null and skip adding to the list.
          PoseSample poseSample = PoseSample.getPoseSample(line, ",");
          if (poseSample != null) {
            poseSamples.Add(poseSample);
          }
        }
      } 
      catch (IOException e) 
      {
        Debug.Log("Error when loading pose samples.\n" + e);
      }
    
      poseClassifier = new PoseClassifier(poseSamples);
      if (isStreamMode) {
        foreach (String className in POSE_CLASSES) {
          repCounters.Add(new RepetitionCounter(className));
        }
      }
    }

    /**
    * Given a new {@link Pose} input, returns a list of formatted {@link String}s with Pose
    * classification results.
    *
    * <p>Currently it returns up to 2 strings as following:
    * 0: PoseClass : X reps
    * 1: PoseClass : [0.0-1.0] confidence
    */
    //@WorkerThread
    public List<String> getPoseResult(PoseLandmarkDetect.Result pose) {
      List<String> result = new List<String>();
      Debug.Log(poseClassifier == null);
      ClassificationResult classification = poseClassifier.classify(pose);
        
      Debug.Log(classification);
      // Update {@link RepetitionCounter}s if {@code isStreamMode}.
      if (isStreamMode) {
        // Feed pose to smoothing even if no pose found.
        classification = emaSmoothing.getSmoothedResult(classification);

        // Return early without updating repCounter if no pose found.
        if (pose.joints.Length == 0) {
          result.Add(lastRepResult);
          return result;
        }

        foreach (RepetitionCounter repCounter in repCounters) {
          int repsBefore = repCounter.getNumRepeats();
          int repsAfter = repCounter.addClassificationResult(classification);
          if (repsAfter > repsBefore) {
            // Play a fun beep when rep counter updates.
            // 
            Debug.Log("BEEEEP");
            break;
          }
        }
        result.Add(lastRepResult);
      }

      // Add maxConfidence class of current frame to result if pose is found.
      if (pose.joints.Count() != 0) {
        String maxConfidenceClass = classification.getMaxConfidenceClass();
        String maxConfidenceClassResult = String.Format(
          
            "%s : %.2f confidence",
            maxConfidenceClass,
            classification.getClassConfidence(maxConfidenceClass)
                / poseClassifier.confidenceRange());
        result.Add(maxConfidenceClassResult);
      }

      return result;
    }

  }
}