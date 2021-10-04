using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class AppUse
{
    public string fname;
    public string lname;
    public long contactno;

    public string time;
    public string exercise;

    public AppUse(BlazePoseRunner newuse)
    {
        fname = newuse.fname;
        lname = newuse.lname;
        contactno = newuse.contactno;
        time = newuse.time;
        exercise = newuse.Exercise;

    }

    public static void SaveAppUse(BlazePoseRunner newappuse)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/appuseinfo.data";

        if (File.Exists(path))
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            AppUse data = new AppUse(newappuse);

            // Debug.Log("New data: "+ data.exercise+ "done by "+ data.fname);

            formatter.Serialize(stream, data);
            stream.Close();

        }

        else
        {
            FileStream stream = new FileStream(path, FileMode.Create);
            AppUse data = new AppUse(newappuse);

            Debug.Log("New data: "+ data.exercise+ "done by "+ data.fname);

            formatter.Serialize(stream, data);
            stream.Close();
        }




    }

    public static AppUse LoadAppUse()
    {
        string path = Application.persistentDataPath + "/appuseinfo.data";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            AppUse useinfo = formatter.Deserialize(stream) as AppUse;
            stream.Close();
            return useinfo;

        }
        else
        {
            Debug.Log("Saved file not found");
            return null;
        }
    }
}

