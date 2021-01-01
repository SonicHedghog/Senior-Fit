using static TensorFlowLite.PoseNet;

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
        public new static bool IsPose(Result[] result)
        {
            float i = 0f;
            for(int x = 7; x < 11; x++){i+=result[x].confidence;}

            if(result[9].y < result[7].y && 
                    result[10].y < result[8].y && 
                    (result[10].x - result[9].x) > -.05 && i > 2.6)
            {
                return true;
            }
            else
                return false;
        }
    }
}