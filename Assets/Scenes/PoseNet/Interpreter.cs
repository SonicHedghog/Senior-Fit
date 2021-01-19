using System.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Interpreter
{
    private Queue<Poses.Pose> poses;
    private Queue<String> sceneCommands;
    
    public Interpreter(string filename)
    {
        List<string> fileLines = File.ReadAllLines(Application.streamingAssetsPath + "/Routines/" + filename + ".txt").ToList();
        string[] temp = {"switch ", "background ", "run ", "voice ", "image ", "wait ", "pause ", "words ", "; ", "next "};

        poses = new Queue<Poses.Pose>();
        sceneCommands = new Queue<string>();

        foreach (var line in fileLines)
        {
            // try
            // {
                Debug.Log(line.Split()[0] );
                poses.Enqueue((Poses.Pose)Activator.CreateInstance(Type.GetType("Poses." + line.Split(' ')[0]), new System.Object[]{line.Split(' ')[1]}));
            // }
            // catch
            // {
            //     // Poses.ArmCross 
            //     poses.Enqueue(null);
            //     sceneCommands.Enqueue(line);
            // }
        }
    }

   public Poses.Pose AdvanceScript()
   {
       return poses.Dequeue();
   }

   public string GetCommand()
   {
       return sceneCommands.Dequeue();
   }

   public bool isDone
   {
       get{
           return poses.Count == 0;
       }
   }

}