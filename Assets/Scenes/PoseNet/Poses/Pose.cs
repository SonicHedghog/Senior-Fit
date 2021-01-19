using static TensorFlowLite.PoseNet;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace Poses
{
    public abstract class Pose
    {
        private Text exerciseName;
        public static Part[] required { get; }
        
        public static Part[] disabilities;
        public static string name = "";
        protected int _repCount = 0;

        public Pose(string repCount) { _repCount = Int32.Parse(repCount);}

        public abstract bool IsFinished(Result[] result, Text t);
        public bool IsPossiblePose()
        {
            return required.Any(el => disabilities.Contains(el));
        }

        public void RepAction(Text t)
        {
            t.text = name;
            Debug.Log(name);
        }

        public void NoRepAction(Text t)
        {
            t.text = "Exercise Name";
            Debug.Log("Pose");
        }
        
    }
}