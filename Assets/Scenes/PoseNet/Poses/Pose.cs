using static TensorFlowLite.PoseNet;
using System.Linq;

namespace Poses
{
    public abstract class Pose
    {
        public static Part[] required { get; }
        
        public static Part[] disabilities;
        string name;

        public static bool IsPose(Result[] result)
        {
            return false;
        }

        public static bool IsPossiblePose()
        {
            return required.Any(el => disabilities.Contains(el));
        }
        
    }
}