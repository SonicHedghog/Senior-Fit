using UnityEngine;
using System;
using System.Collections.Generic;
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
   // private static readonly String SEATED_MARCH_LEFT = "Seated_March_Left";
   // private static readonly String SEATED_MARCH_RIGHT = "Seated_March_Right";
    private static readonly String Sit_To_Stand_Sit = "Sit_To_Stand_Sit";
    private static readonly String Sit_To_Stand_Stand= "Sit_To_Stand_Stand";
    private static readonly String Single_Leg_Stance_Left= "Single_Leg_Stance_Left";
    private static readonly String Single_Leg_Stance_Right= "Single_Leg_Stance_Right";
    private static readonly String Shoulder_touch= "Shoulder_touch";
    private static readonly String Touch_down= "Touch_down";
    private static readonly String Seated_Hamstring_Stretch_Right= "Seated_Hamstring_Stretch_Right";
    private static readonly String Seated_Hamstring_Stretch_Left= "Seated_Hamstring_Stretch_Left";

    private static readonly String Chair_Sit="Chair_Sit";
    private static readonly String Chair_Stand="Chair_Stand";

     private static readonly String March_Left="March_Left";

      private static readonly String March_Right="March_Right";



    private static readonly String[] POSE_CLASSES = {
      Seated_Hamstring_Stretch_Right,Seated_Hamstring_Stretch_Left,Single_Leg_Stance_Left,Single_Leg_Stance_Right,Shoulder_touch,Touch_down,Chair_Sit,Chair_Stand,March_Right,March_Left    };

    private readonly bool isStreamMode;

    private EMASmoothing emaSmoothing;
    private List<RepetitionCounter> repCounters;
    private PoseClassifier poseClassifier;
    private String lastRepResult;

    private float ENTER_THRESHOLD = 0;
    private float EXIT_THRESHOLD = 0;

    //@WorkerThread
    public PoseClassifierProcessor(string filename, bool isStreamMode) {

      Debug.Log("File name "+ filename);
      // Preconditions.checkState(Looper.myLooper() != Looper.getMainLooper());
      this.isStreamMode = isStreamMode;
      if (isStreamMode) {
        emaSmoothing = new EMASmoothing();
        repCounters = new List<RepetitionCounter>();
        lastRepResult = "";
      }
      loadPoseSamples(filename,ENTER_THRESHOLD,EXIT_THRESHOLD);
     
    }

    public PoseClassifierProcessor(string filename, bool isStreamMode, float enterThreshold, float exitThreshold) {
      Debug.Log("File name "+ filename);
      // Preconditions.checkState(Looper.myLooper() != Looper.getMainLooper());
      this.isStreamMode = isStreamMode;
      if (isStreamMode) {
        emaSmoothing = new EMASmoothing();
        repCounters = new List<RepetitionCounter>();
        lastRepResult = "";
      }
     
     
      ENTER_THRESHOLD = enterThreshold;
      EXIT_THRESHOLD = exitThreshold;
      loadPoseSamples(filename,ENTER_THRESHOLD,EXIT_THRESHOLD);

    }

    private void loadPoseSamples(string filename,float enterThreshold, float exitThreshold) {

     
      string[] paths = {Application.streamingAssetsPath, "BlazePoseData", filename + ".csv"};
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
              fileLines = File.ReadAllLines(Application.streamingAssetsPath + "/BlazePoseData/" + filename + ".csv").ToList();
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
          //if(EXIT_THRESHOLD == 0) repCounters.Add(new RepetitionCounter(className));
          //else
           repCounters.Add(new RepetitionCounter(className, enterThreshold, exitThreshold));
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

      string coordinatevalue = "";

      foreach( Vector3 v in pose.joints)
      {
          coordinatevalue+= v + " ";
      }
      
      
      //Debug.Log("Coordinate : "+ coordinatevalue);
      List<String> result = new List<String>();

    
     // Debug.Log(poseClassifier == null);
      ClassificationResult classification = poseClassifier.classify(pose);
        
      //Debug.Log(classification==null);
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
            lastRepResult=repCounter.getClassName() + " : " + repsAfter;
            break;
          }
        }
        result.Add(lastRepResult);

      }

      // Add maxConfidence class of current frame to result if pose is found.
      if (pose.joints.Count() != 0) {
       String maxConfidenceClass = classification.getMaxConfidenceClass();
        String maxConfidenceClassResult = maxConfidenceClass + " : "
        +classification.getClassConfidence(maxConfidenceClass) / poseClassifier.confidenceRange()
        + " confidence";
               
        result.Add(maxConfidenceClassResult);

      }

      return result;
    }
  }
}