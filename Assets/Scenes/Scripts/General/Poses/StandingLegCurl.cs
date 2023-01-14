using static TensorFlowLite.PoseLandmarkDetect;
using UnityEngine.UI;
using UnityEngine;
using System;
using TensorFlowLite;
using System.Collections.Generic;

namespace Poses
{
    public class StandingLegCurl : Pose
    {
        PoseClassifierProcessor processor;
        String lastExercise = "";
        public static new int[] required
        {
            get
            {
                return new int[] {
                    
                    PoseLandmarkDetect.LEFT_KNEE,
                    PoseLandmarkDetect.RIGHT_KNEE,
                    PoseLandmarkDetect.LEFT_ANKLE,
                    PoseLandmarkDetect.RIGHT_ANKLE
                };
            }
        }

        public StandingLegCurl(string repCount) : base(repCount) 
        { 
            name = "Standing Leg Curl";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Standing_Leg_Curl", true,7.0f,6.0f);
        }     

        public override bool IsFinished(Result result, Text t)
        {
            if(result == null) return false;
            foreach (int x in required)
            {
                if(result.joints[x].w < .5f) return false;
            }

            List<string> poses = processor.getPoseResult(result);
            foreach(string s in poses)
            {
                Debug.Log("Important: " + s);
            }
            
            if(poses[0] != lastExercise)
            {
                RepAction(t);
                _repCount --;
                lastExercise = poses[0];
            }
            else NoRepAction(t);
            return _repCount == 0;
        }

        public override string GetTutorialAddress()
        {
            if(lastExercise.Contains("Leg_Curl_Right"))
                return Application.streamingAssetsPath + "/TutorialClips/" + name.Replace(' ', '_').ToLower() + "_right_tutorial.mp4";
            else return Application.streamingAssetsPath + "/TutorialClips/" + name.Replace(' ', '_').ToLower() + "_left_tutorial.mp4";
        }

        public override bool IsFinished(TensorFlowLite.PoseNet.Result[] result, Text t){ return false; }
    }
}