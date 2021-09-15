using static TensorFlowLite.PoseNet;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace Poses
{
    public class SeatedMarch : Pose
    {
        public static new Part[] required 
        {
            get
            {
                return new Part[] {
                    Part.LEFT_HIP,
                    Part.RIGHT_HIP,
                    Part.LEFT_KNEE,
                    Part.RIGHT_KNEE,
                    Part.LEFT_ANKLE,
                    Part.RIGHT_ANKLE,
                };
            }
        }

        bool legCheck = false;

        public SeatedMarch(string repCount) : base(repCount) { name = "Seated March"; }
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

        public override bool IsFinished(TensorFlowLite.PoseLandmarkDetect.Result[] result, Text t) { return false; }
    }

}