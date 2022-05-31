using static TensorFlowLite.PoseLandmarkDetect;
using UnityEngine.UI;
using UnityEngine;
using System;
using TensorFlowLite;
using System.Collections.Generic;

namespace Poses
{
public class ChairSitToStand : Pose
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
                    PoseLandmarkDetect.LEFT_ANKLE,
                    PoseLandmarkDetect.RIGHT_ANKLE,
                    

                };
            }
        }

        PoseClassifierProcessor processor;
        String lastExercise = "";
        bool legCheck = false;

        public ChairSitToStand(string repCount) : base(repCount) 
        { 
            name = "Chair Sit to Stand";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Chair_Sit_to_Stand", true,7.5f,6.5f);
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