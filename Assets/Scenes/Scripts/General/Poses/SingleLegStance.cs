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
        public static new Part[] required 
        {
            get
            {
                return new Part[] {
                    Part.LEFT_KNEE,
                    Part.RIGHT_KNEE,
                    Part.LEFT_ANKLE,
                    Part.RIGHT_ANKLE,
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

        public SingleLegStance(string repCount) : base(repCount) 
        { 
            name = "Single Leg Stance";

            // Set up Pose Classifier Processor
            processor = new PoseClassifierProcessor("Single_Leg_Stance", true);
        }        public override bool IsFinished(Result[] result, Text t)
        {
            float i = 0f;
            for(int x = 11; x < 17; x++){i+=result[x].confidence;}

            // Debug.Log(i);

            switch(legCheck)
            {
                case 0:
                    return CheckLeftLeg(result, t, i);
                case 2:
                    return CheckRightLeg(result, t, i);
                default:
                    return CountDown(t);
            }
        }

        bool CheckLeftLeg(Result[] result, Text t, float i)
        {
            if(i > 4)
            {
                double A = Math.Sqrt(
                            Math.Pow(result[(int)Part.LEFT_KNEE].x - result[(int)Part.LEFT_HIP].x, 2) +
                            Math.Pow(result[(int)Part.LEFT_KNEE].y - result[(int)Part.LEFT_HIP].y, 2));

                double B = Math.Sqrt(
                            Math.Pow(result[(int)Part.LEFT_KNEE].x - result[(int)Part.LEFT_ANKLE].x, 2) +
                            Math.Pow(result[(int)Part.LEFT_KNEE].y - result[(int)Part.LEFT_ANKLE].y, 2));

                double C = Math.Sqrt(
                            Math.Pow(result[(int)Part.LEFT_HIP].x - result[(int)Part.LEFT_ANKLE].x, 2) +
                            Math.Pow(result[(int)Part.LEFT_HIP].y - result[(int)Part.LEFT_ANKLE].y, 2));

                double theta = Math.Acos((Math.Pow(C, 2) - Math.Pow(A, 2) - Math.Pow(B, 2))/(-2*A*B)) * (180/Math.PI);

                Debug.Log(theta);
                if(theta < 165)
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
                    // if(waitTime < 3 && waitTime > 2.7 && incorrectCounts > correctCounts)
                    // {
                    //     waitTime = 3;
                    // }
                }
            }
            return false;
        }

        bool CheckRightLeg(Result[] result, Text t, float i)
        {
            if(i > 4)
            {
                double A = Math.Sqrt(
                            Math.Pow(result[(int)Part.RIGHT_KNEE].x - result[(int)Part.RIGHT_HIP].x, 2) +
                            Math.Pow(result[(int)Part.RIGHT_KNEE].y - result[(int)Part.RIGHT_HIP].y, 2));

                double B = Math.Sqrt(
                            Math.Pow(result[(int)Part.RIGHT_KNEE].x - result[(int)Part.RIGHT_ANKLE].x, 2) +
                            Math.Pow(result[(int)Part.RIGHT_KNEE].y - result[(int)Part.RIGHT_ANKLE].y, 2));

                double C = Math.Sqrt(
                            Math.Pow(result[(int)Part.RIGHT_HIP].x - result[(int)Part.RIGHT_ANKLE].x, 2) +
                            Math.Pow(result[(int)Part.RIGHT_HIP].y - result[(int)Part.RIGHT_ANKLE].y, 2));

                double theta = Math.Acos((Math.Pow(C, 2) - Math.Pow(A, 2) - Math.Pow(B, 2))/(-2*A*B)) * (180/Math.PI);

                if(theta < 165)
                {
                    if(waitTime <= 0)
                    {
                        if(correctCounts/4 > incorrectCounts)
                        {
                            RepAction(t);
                            legCheck = 3;
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
                    // if(waitTime < 3 && waitTime > 2.7 && incorrectCounts > correctCounts)
                    // {
                    //     waitTime = 3;
                    // }
                }

            }

            return false;
        }

        bool CountDown(Text t)
        {
            if(_repCount <= 0)
            {
                if (legCheck == 3)
                    return true;
                legCheck = 2;
            }
            
            _repCount -= Time.deltaTime;
            RepAction(t);
            return false;
        }

    public override bool IsFinished(TensorFlowLite.PoseLandmarkDetect.Result result, Text t)
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
        }    }

}