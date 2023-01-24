using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveData
{
    public static List<ExerciseInfo> AllUserInfo = new List<ExerciseInfo>();

    public static void SaveIntoJson(ExerciseRecognizer newappuse)
    {
        string path = Application.persistentDataPath + "/UserData.json";
        string fileContents;
        ExerciseData gameData = new ExerciseData();

        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            if(fileContents.Length != 0)
            {
                gameData = JsonUtility.FromJson<ExerciseData>(fileContents);
            }
            
            gameData.alluserdata.Add(new ExerciseInfo()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date = newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise,
                repCount = newappuse.repCount,
                duration = newappuse.Time_duration
            });

            fileContents = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, fileContents);
        }
        else
        {
            
            gameData.alluserdata.Add(new ExerciseInfo()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date = newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise,
                repCount = newappuse.repCount,
                duration = newappuse.Time_duration
            });

            fileContents = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, fileContents);
        }
    }

    //For no camera
    public static void SaveIntoJson(NoCameraWorkout newappuse)
    {
        string path = Application.persistentDataPath + "/UserData.json";
        string fileContents;
        ExerciseData gameData = new ExerciseData();

        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            if(fileContents.Length!= 0)
            {
                gameData = JsonUtility.FromJson<ExerciseData>(fileContents);
            }
            
            gameData.alluserdata.Add(new ExerciseInfo()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date = newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise,
                duration = newappuse.Time_duration,
                repCount = "-1"
            });

            fileContents = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, fileContents);
        }
        else
        {
            gameData.alluserdata.Add(new ExerciseInfo()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date = newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise,
                duration = newappuse.Time_duration
            });

            fileContents = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, fileContents);
        }
    }

    //For BlazeposeRunner
    public static void SaveBlazePoseRunnerData(BlazePoseRunner newappuse)
    {
        string path = Application.persistentDataPath + "/UserData.json";
        string fileContents;
        ExerciseData gameData = new ExerciseData();

        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            if(fileContents.Length!=0)
            {
                gameData = JsonUtility.FromJson<ExerciseData>(fileContents);
            }
            
            gameData.alluserdata.Add(new ExerciseInfo()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date = newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise
            });

            fileContents = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, fileContents);
        }
        else
        {
            gameData.alluserdata.Add(new ExerciseInfo()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date = newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise
            });

            fileContents = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, fileContents);
        }
    }

  
    public static ExerciseData LoadData()
    {
        string path = Application.persistentDataPath + "/UserData.json";
        if (File.Exists(path))
        {
            string fileContents = File.ReadAllText(path);

            ExerciseData gameData = JsonUtility.FromJson<ExerciseData>(fileContents)
                            ?? new ExerciseData();
            File.WriteAllText(path, string.Empty);
            return gameData;
        }
        else
        {
            Debug.Log("File Not Found!");
            return null;
        }
    }

    public static GPSList LoadGPSData()
    {
        string path = Application.persistentDataPath + "/userlocation.json";
        if (File.Exists(path))
        {
            string fileContents = File.ReadAllText(path);
            string jsonData="{ \"allgpsdata\" : " + fileContents + "}";
            Debug.Log(jsonData);
            
            try
            {
                GPSList gpsData = JsonUtility.FromJson<GPSList>(jsonData)
                                ?? new GPSList();
                File.WriteAllText(path, string.Empty);
                return gpsData;
            }
            catch(Exception e)
            {
                Debug.Log(e);
                File.WriteAllText(path, string.Empty);
                return null;
            }
        }
        else
        {
            Debug.Log("File Not Found!");
            return null;
        }
    }

    public static NotificationList LoadNotifications()
    {
        string[] paths = {Application.streamingAssetsPath, "Routines", "PushNotifications.json"};
        string fileContents;
        
        if(Application.platform == RuntimePlatform.Android)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
            www.SendWebRequest();
            while (!www.isDone)
            {
            }
            fileContents = www.downloadHandler.text;
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            fileContents = File.ReadAllText(Application.streamingAssetsPath + "/Routines/" + "PushNotifications.json");
        }

        NotificationList Notification_Data = JsonUtility.FromJson<NotificationList>(fileContents)
                            ?? new NotificationList();
        return Notification_Data;
    }

    public static string LoadGoal(string textfile)
    {
        string[] paths = {Application.streamingAssetsPath, "Routines", textfile};
        string fileContents;
        
        if(Application.platform == RuntimePlatform.Android)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
            www.SendWebRequest();
            while (!www.isDone)
            {
            }
            fileContents = www.downloadHandler.text;
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            fileContents = File.ReadAllText(Application.streamingAssetsPath + "/Routines/" + textfile);
        }

        return fileContents;
    }

    public static string IsNotificationScheduled()
    {
        string[] paths = {Application.streamingAssetsPath, "Routines", "Scheduled.txt"};
        string fileContents;
        
        if(Application.platform == RuntimePlatform.Android)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(Path.Combine(paths));
            www.SendWebRequest();
            while (!www.isDone)
            {
            }
            fileContents = www.downloadHandler.text;
            Debug.Log(www.downloadHandler.text);            
        }
        else
        {
            fileContents = File.ReadAllText(Application.streamingAssetsPath + "/Routines/" + "Scheduled.txt");
        }

        return fileContents;
    }

    public static void SaveCameraState(int newCameraState )
    {
        string path = Application.persistentDataPath + "/CameraData.json";
        string fileContents;

        CameraState cameraData = new CameraState();
        cameraData.cameraState = newCameraState;
        fileContents = JsonUtility.ToJson(cameraData);
        File.WriteAllText(path, fileContents);
    }

    public static int LoadCameraData()
    {
        string path = Application.persistentDataPath + "/CameraData.json";
        if (File.Exists(path))
        {
            string fileContents = File.ReadAllText(path);
            CameraState cameraData = JsonUtility.FromJson<CameraState>(fileContents)
                            ?? new CameraState();
            return cameraData.cameraState;
        }
        else
        {
            Debug.Log("Camera Data File Not Found!");
            return -1;
        }
    }

    public static void UpdateSoundData(bool newSoundState )
    {
        string path = Application.persistentDataPath + "/SoundData.data";

        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path,FileMode.Open);
            SOundState data = formatter.Deserialize(stream) as SOundState;
            data.soundState = newSoundState;
            Debug.Log("Inside Sound Update: " + data.soundState);            
            stream.Close();

            BinaryFormatter OutputFormatter = new BinaryFormatter();
            FileStream outStream= new FileStream(path, FileMode.Create);
            SOundState NewData= data;
            OutputFormatter.Serialize(outStream, data);
            outStream.Close();
        }
        else
        {
            Debug.Log("Sound config not found");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
            SOundState data = new SOundState(true);
            formatter.Serialize(stream, data);
            stream.Close();
        }
    }

     public static SOundState LoadSoundState()
    {
        string path = Application.persistentDataPath + "/SoundData.data";
    
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path,FileMode.Open);
            SOundState data = formatter.Deserialize(stream) as SOundState;
            Debug.Log("Sound data: " + data.soundState.ToString());
            stream.Close();
            return data;
        }
        else
        {
            Debug.Log("Saved File Not Found");
            return null;
        }
    }
}

[System.Serializable]
public class ExerciseData
{
    public List<ExerciseInfo> alluserdata;
    public ExerciseData()
    {
        alluserdata = new List<ExerciseInfo>();
    }
}

[System.Serializable]
public class ExerciseInfo
{
    public string fname;
    public string lname;
    public long contactno;
    public string date,time;
    public string exercise;
    public string repCount;
    public float duration;
}


[System.Serializable]
public class GPSList
{
    public List<newLocation> allgpsdata;

    public GPSList()
    {
        allgpsdata = new List<newLocation>();
    }
}


[System.Serializable]
public class newLocation
{
   public long contactNo;
        public string currentDate;
        public string currentTime;
        public string firstName;
        public string lastName;
        public double latitude;
        public double longitude;
        public string startTime;
        public double totaldistance;
}

[System.Serializable]
public class NotificationList
{
    public List<UserNotification> allnotifications;

    public NotificationList()
    {
        allnotifications = new List<UserNotification>();
    }
}

[System.Serializable]
public class UserNotification
{
    public string message;
    public string url;
    public int interval;
}


[System.Serializable]
public class CameraState
{
    public int cameraState;
}

[System.Serializable]
public class SOundState
{
    public bool soundState;
    public SOundState(bool newSoundstate)
    {
        soundState = newSoundstate;
    }   
}