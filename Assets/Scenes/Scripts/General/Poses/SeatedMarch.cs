using static TensorFlowLite.PoseNet;
using UnityEngine.UI;
using UnityEngine;
using System;
using TensorFlowLite;
using System.Collections.Generic;

namespace Poses
{
    public class SeatedMarch : Pose
    {
        PoseClassifierProcessor processor;
        String lastExercise = "";
        
        // PoseNet Part List
        // public static new Part[] required 
        // {
        //     get
        //     {
        //         return new Part[] {
        //             Part.LEFT_HIP,
        //             Part.RIGHT_HIP,
        //             Part.LEFT_KNEE,
        //             Part.RIGHT_KNEE,
        //             Part.LEFT_ANKLE,
        //             Part.RIGHT_ANKLE,
        //         };
        //     }
        // }

        public static new int[] required
        {
            get
            {
                return new int[] {
                    PoseLandmarkDetect.LEFT_HIP,
                    PoseLandmarkDetect.RIGHT_HIP,
                    PoseLandmarkDetect.LEFT_KNEE,
                    PoseLandmarkDetect.RIGHT_KNEE,
                    PoseLandmarkDetect.LEFT_ANKLE,
                    PoseLandmarkDetect.RIGHT_ANKLE
                };
            }
        }

        bool legCheck = false;

        public SeatedMarch(string repCount) : base(repCount) 
        { 
            name = "Seated March";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("new_SeatedMarch", true,7f,6f);
        }
        public override bool IsFinished(Result[] result, Text t)
        {
            float i = 0f;
            for(int x = 7; x < 11; x++){i+=result[x].confidence;}

            switch(legCheck)
            {
                case false:
                    return CheckLeftLeg(result, t, i);
                default:
                    return CheckRightLeg(result, t, i);
            }
        }

        bool CheckLeftLeg(Result[] result, Text t, float i)
        {
            if(Math.Min(result[(int)Part.LEFT_HIP].y, result[(int)Part.RIGHT_KNEE].y) >
                                result[(int)Part.LEFT_KNEE].y && result[(int)Part.LEFT_ANKLE].y <  
                                result[(int)Part.RIGHT_ANKLE].y && i > 2.6)
                        {
                            _repCount--;
                            RepAction(t);
                            legCheck = true;
                            return _repCount == 0;
                        }
                        else
                        {
                            NoRepAction(t);
                            return false;
                        }
        }

        bool CheckRightLeg(Result[] result, Text t, float i)
        {
            if(Math.Min(result[(int)Part.RIGHT_HIP].y, result[(int)Part.LEFT_KNEE].y) >
                                result[(int)Part.RIGHT_KNEE].y && result[(int)Part.RIGHT_ANKLE].y <  
                                result[(int)Part.LEFT_ANKLE].y && i > 2.2)
                        {
                            _repCount--;
                            RepAction(t);
                            legCheck = false;
                            return _repCount == 0;
                        }
                        else
                        {
                            NoRepAction(t);
                            return false;
                        }
        }

        public override bool IsFinished(TensorFlowLite.PoseLandmarkDetect.Result result, Text t)
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
    }

}