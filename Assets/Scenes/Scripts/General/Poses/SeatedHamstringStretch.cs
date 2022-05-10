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

        int legCheck = 0;
        float setTime = 7;
        float waitTime = 3;
        int correctCounts = 0;
        int incorrectCounts = 0;
        bool loop = false;

        public SeatedHamstringStretch(string repCount) : base(repCount) 
        { 
            name = "Seated Hamstring Stretch";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Seated_Hamstring_Stretch_CSV", true);
            loop = Int32.Parse(repCount) < 0 ? true : false;
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

        bool CheckLeftLeg(List<string> poses, Text t)
        {
            if(poses[0] == "Seated_Hamstring_Stretch_Left")
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

            if(poses[0] == "Seated_Hamstring_Stretch_Right")
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

        public override bool IsFinished(TensorFlowLite.PoseNet.Result[] result, Text t){ return false; }
    }

}