using static TensorFlowLite.PoseLandmarkDetect;
using UnityEngine.UI;
using UnityEngine;
using System;
using TensorFlowLite;
using System.Collections.Generic;

namespace Poses
{
    public class SeatedHamstringStretch : Pose
    {
        PoseClassifierProcessor processor;
        String lastExercise = "";
        // public static new Part[] required 
        // {
        //     get
        //     {
        //         return new Part[] {
        //         };
        //     }
        // }

        bool legCheck = false;

        public SeatedHamstringStretch(string repCount) : base(repCount) 
        { 
            name = "Seated Hamstring Stretch";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Seated_Hamstring_Stretch_CSV", true);
        }
        public override bool IsFinished(Result result, Text t)
        {
            if(result == null) return false;
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