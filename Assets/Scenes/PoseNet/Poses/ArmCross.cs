using static TensorFlowLite.PoseNet;
using UnityEngine.UI;
using UnityEngine;

namespace Poses
{
    public class ArmCross : Pose
    {
        public static new Part[] required 
        {
            get
            {
                return new Part[] {
                    Part.RIGHT_SHOULDER,
                    Part.LEFT_SHOULDER,
                    Part.RIGHT_ELBOW,
                    Part.LEFT_ELBOW,
                };
            }
        }

        

        public ArmCross(string repCount) : base(repCount) { name = "Arm Cross"; }
        public override bool IsFinished(Result[] result, Text t)
        {
            float i = 0f;
            for(int x = 7; x < 11; x++){i+=result[x].confidence;}

            if(result[9].y < result[7].y && 
                    result[10].y < result[8].y && 
                    (result[10].x - result[9].x) > -.05 && i > 2.6)
            {
                _repCount--;
                RepAction(t);
                return true;
            }
            else
                NoRepAction(t);
                return false;
        }
    }
}