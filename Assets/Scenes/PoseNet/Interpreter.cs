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
        string[] paths = {Application.streamingAssetsPath, "Routines", filename + ".txt"};
        List<string> fileLines;
        
        var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
        www.SendWebRequest();
        while (!www.isDone)
        {
        }
        fileLines = www.downloadHandler.text.Split('\n').ToList();
        Debug.Log(www.downloadHandler.text);

        string[] temp = {"switch ", "background ", "run ", "voice ", "image ", "wait ", "pause ", "words ", "; ", "next "};

        poses = new Queue<Poses.Pose>();
        sceneCommands = new Queue<string>();

        foreach (var line in fileLines)
        {
            try
            {
                Debug.Log(line.Split()[0] );
                poses.Enqueue((Poses.Pose)Activator.CreateInstance(Type.GetType("Poses." + line.Split(' ')[0]), new System.Object[]{line.Split(' ')[1]}));
            }
            catch
            {
                poses.Enqueue(null);
                sceneCommands.Enqueue(line);
            }
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