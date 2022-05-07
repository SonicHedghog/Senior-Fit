using static TensorFlowLite.PoseNet;
using UnityEngine.UI;
using UnityEngine;
using System;
using TensorFlowLite;
using System.Collections.Generic;

namespace Poses
{
    public class SingleLegStance : Pose
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
        byte legCheck = 0;
        float setTime = 7;
        float waitTime = 3;
        int correctCounts = 0;
        int incorrectCounts = 0;
        bool loop = false;

        public SingleLegStance(string repCount) : base(repCount) 
        { 
            name = "Single Leg Stance";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Single_Leg_Stance", true,7.5f,6.5f);
            loop = Int32.Parse(repCount) < 0 ? true : false;
        }        
        public override bool IsFinished(Result[] result, Text t)
        {
            float i = 0f;
            for(int x = 11; x < 17; x++){i+=result[x].confidence;}

            // Debug.Log(i);

            switch(legCheck)
            {
                // case 0:
                //     return CheckLeftLeg(result, t, i);
                // case 2:
                //     return CheckRightLeg(result, t, i);
                default:
                    return CountDown(t);
            }
        }

        bool CheckLeftLeg(List<string> poses, Text t)
        {
            if(poses[0] == "Single_Leg_Stance_Left")
            {
                if(waitTime <= 0)
                {
                    if(correctCounts/4 > incorrectCounts)
                    {
                        RepAction(t);
                        legCheck++;
                        _repCount = setTime;
                    }
                    
                    waitTime = 3;
                    correctCounts = 0;
                    incorrectCounts = 0;
                }
                else { waitTime -= Time.deltaTime; NoRepAction(t); }
                correctCounts++;
            }
            else
            {
                incorrectCounts++;
                NoRepAction(t);

            }

            return false;

        }

        bool CheckRightLeg(List<string> poses, Text t)
        {

            if(poses[0] == "Single_Leg_Stance_Right")
            {
                if(waitTime <= 0)
                {
                    if(correctCounts/4 > incorrectCounts)
                    {
                        RepAction(t);
                        legCheck++;
                        _repCount = setTime;
                    }
                    
                    waitTime = 3;
                    correctCounts = 0;
                    incorrectCounts = 0;
                }
                else { waitTime -= Time.deltaTime; NoRepAction(t); }
                correctCounts++;
            }
            else
            {
                incorrectCounts++;
                NoRepAction(t);

            }
            
            return false;

        }

        bool CountDown(Text t)
        {
            if(_repCount <= 0)
            {
                if (legCheck == 3)
                {
                    if(!loop)
                        return true;
                    legCheck = 0;
                    return false;
                }
                legCheck = 2;
            }
            
            _repCount -= Time.deltaTime;
            RepAction(t);
            return false;
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
            
            switch(legCheck)
            {
                case 0:
                    return CheckLeftLeg(poses, t);
                case 2:
                    return CheckRightLeg(poses, t);
                default:
                    return CountDown(t);
            }
        } 
    }   

}