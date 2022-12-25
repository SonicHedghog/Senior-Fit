using static TensorFlowLite.PoseLandmarkDetect;
using UnityEngine.UI;
using UnityEngine;
using System;
using TensorFlowLite;
using System.Collections.Generic;

namespace Poses
{
    public class SideStepping : Pose
    {
        public static new int[] required
        {
            get
            {
                return new int[] {
                    PoseLandmarkDetect.LEFT_EYE,
                    PoseLandmarkDetect.RIGHT_EYE,
                    PoseLandmarkDetect.LEFT_HIP,
                    PoseLandmarkDetect.RIGHT_HIP,
                    PoseLandmarkDetect.LEFT_KNEE,
                    PoseLandmarkDetect.RIGHT_KNEE,
                    PoseLandmarkDetect.LEFT_ANKLE,
                    PoseLandmarkDetect.RIGHT_ANKLE
                };
            }
        }

        PoseClassifierProcessor processor;
        String lastExercise = "";

        
        bool legCheck = false;
        public SideStepping(string repCount) : base(repCount) 
        { 
            name = "Side Stepping";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Side_Stepping", true,8.5f,7.0f);
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

        

        public override bool IsFinished(TensorFlowLite.PoseNet.Result[] result, Text t){ return false; }
    }
       
}