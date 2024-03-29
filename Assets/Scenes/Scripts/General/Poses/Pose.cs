using TensorFlowLite;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;

namespace Poses
{
    public abstract class Pose
    {
        private Text exerciseName;
        public static int[] required { get; }
        
        public static int[] disabilities;
        protected string name = "";
        protected float _repCount = 0;

        public Pose(string repCount) { _repCount = Int32.Parse(repCount);}

        public abstract bool IsFinished(PoseNet.Result[] result, Text t);
        public abstract bool IsFinished(PoseLandmarkDetect.Result result, Text t);
        public bool IsPossiblePose()
        {
            return required.Any(el => disabilities.Contains(el));
        }

        public void RepAction(Text t)
        {
            string path = Application.persistentDataPath + "/RepCount.txt";
            t.text = name + " x " + Math.Abs((int)_repCount);
            File.WriteAllText(path, ( Math.Abs((int)_repCount)).ToString());
            Debug.Log("Rep count :"+( Math.Abs((int)_repCount)).ToString());
        }

        public void NoRepAction(Text t)
        {
            // t.text = "Exercise Name";
            t.text = name + " x " + Math.Abs((int)_repCount);
        }

        public virtual string GetTutorialAddress()
        {
            return Application.streamingAssetsPath + "/TutorialClips/" + name.Replace(' ', '_').ToLower() + "_tutorial.mp4";
        }
        
    }

    public static class PoseLandmarks {

        public const int NOSE = 0;

        public const int LEFT_EYE_INNER = 1;
        public const int LEFT_EYE = 2;
        public const int LEFT_EYE_OUTER = 3;

        public const int RIGHT_EYE_INNER = 4;
        public const int RIGHT_EYE = 5;
        public const int RIGHT_EYE_OUTER = 6;

        public const int LEFT_EAR = 7;
        public const int RIGHT_EAR = 8;

        public const int LEFT_MOUTH = 9;
        public const int RIGHT_MOUTH = 10;

        public const int LEFT_SHOULDER = 11;
        public const int RIGHT_SHOULDER = 12;

        public const int LEFT_ELBOW = 13;
        public const int RIGHT_ELBOW = 14;

        public const int LEFT_WRIST = 15;
        public const int RIGHT_WRIST = 16;

        public const int LEFT_PINKY = 17;
        public const int RIGHT_PINKY = 18;

        public const int LEFT_INDEX = 19;
        public const int RIGHT_INDEX = 20;

        public const int LEFT_THUMB = 21;
        public const int RIGHT_THUMB = 22;

        public const int LEFT_HIP = 23;
        public const int RIGHT_HIP = 24;

        public const int LEFT_KNEE = 25;
        public const int RIGHT_KNEE = 26;

        public const int LEFT_ANKLE = 27;
        public const int RIGHT_ANKLE = 28;

        public const int LEFT_HEEL = 29;
        public const int RIGHT_HEEL = 30;

        public const int LEFT_FOOT_INDEX = 31;
        public const int RIGHT_FOOT_INDEX = 32;

    }
}