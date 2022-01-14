
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveData
{
    //[SerializeField] private UserData newuserdata = new UserData();
    public static List<UserData> AllUserInfo = new List<UserData>();


    public static void SaveIntoJson(ExerciseRecognizer newappuse)
    {
        //UserData data = new UserData(newappuse);
        string path = Application.persistentDataPath + "/UserData.json";
        string fileContents;
        UserList gameData = new UserList();

        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            //Debug.Log("filecontent: "+ fileContents);           
            if(fileContents.Length!=0)
            {
                gameData = JsonUtility.FromJson<UserList>(fileContents);
            }
            
            gameData.alluserdata.Add(new UserData()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date=newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise,
                repCount=newappuse.repCount
            });





            fileContents = JsonUtility.ToJson(gameData);

            File.WriteAllText(path, fileContents);

        }
        else
        {
            
            gameData.alluserdata.Add(new UserData()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date=newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise,
                repCount=newappuse.repCount
            });





            fileContents = JsonUtility.ToJson(gameData);

            File.WriteAllText(path, fileContents);
        }



    }

    /********for BlazeposeRunner***/


    public static void SaveBlazePoseRunnerData(BlazePoseRunner newappuse)
    {
        //UserData data = new UserData(newappuse);
        string path = Application.persistentDataPath + "/UserData.json";
        string fileContents;
        UserList gameData = new UserList();

        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            //Debug.Log("filecontent: "+ fileContents);           
            if(fileContents.Length!=0)
            {
                gameData = JsonUtility.FromJson<UserList>(fileContents);
            }
            
            gameData.alluserdata.Add(new UserData()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date=newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise
            });





            fileContents = JsonUtility.ToJson(gameData);

            File.WriteAllText(path, fileContents);

        }
        else
        {
            
            gameData.alluserdata.Add(new UserData()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                date=newappuse.date,
                time = newappuse.time,
                exercise = newappuse.Exercise
            });





            fileContents = JsonUtility.ToJson(gameData);

            File.WriteAllText(path, fileContents);
        }



    }

    public static void SaveGPSData(Walk newappuse)
    {
        string path = Application.persistentDataPath + "/GPSData.json";
        string fileContents;
        GPSList gpsData = new GPSList();

        if (File.Exists(path))
        {
            fileContents = File.ReadAllText(path);
            //Debug.Log("filecontent: "+ fileContents);           
            if(fileContents.Length!=0)
            {
                gpsData = JsonUtility.FromJson<GPSList>(fileContents);
            }
            
            gpsData.allgpsdata.Add(new GPSData()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                start_time = newappuse.start_time,
                longitudedata = newappuse.longitude,
                latitudedata=newappuse.latitude,
                current_time = newappuse.current_time,
                current_date=newappuse.current_date
            });





            fileContents = JsonUtility.ToJson(gpsData);

            File.WriteAllText(path, fileContents);

        }
        else
        {
            
            gpsData.allgpsdata.Add(new GPSData()
            {
                fname = newappuse.fname,
                lname = newappuse.lname,
                contactno = newappuse.contactno,
                start_time = newappuse.start_time,
                longitudedata = newappuse.longitude,
                latitudedata=newappuse.latitude,
                current_time = newappuse.current_time,
                current_date=newappuse.current_date
            });





            fileContents = JsonUtility.ToJson(gpsData);

            File.WriteAllText(path, fileContents);
        }


    }
    public static UserList LoadData()
    {
        string path = Application.persistentDataPath + "/UserData.json";
        if (File.Exists(path))
        {
            string fileContents = File.ReadAllText(path);
            //Debug.Log("filecontent: "+ fileContents);

            UserList gameData = JsonUtility.FromJson<UserList>(fileContents)
                            ?? new UserList();
            File.WriteAllText(path, string.Empty);
            return gameData;
        }
        else
        {
            Debug.Log("File not found!");
            return null;
        }


    }

    public static GPSList LoadGPSData()
    {
        string path = Application.persistentDataPath + "/GPSData.json";
        if (File.Exists(path))
        {
            string fileContents = File.ReadAllText(path);
            //Debug.Log("filecontent: "+ fileContents);

            GPSList gpsData = JsonUtility.FromJson<GPSList>(fileContents)
                            ?? new GPSList();
            File.WriteAllText(path, string.Empty);
            return gpsData;
        }
        else
        {
            Debug.Log("File not found!");
            return null;
        }


    }
}
[System.Serializable]
public class UserList
{
    public List<UserData> alluserdata;

    public UserList()
    {
        alluserdata=new List<UserData>();
    }
}

[System.Serializable]
public class UserData
{
    public string fname;
    public string lname;
    public long contactno;

    public string date,time;
    public string exercise;
    public string repCount;
}


[System.Serializable]
public class GPSList
{
    public List<GPSData> allgpsdata;

    public GPSList()
    {
        allgpsdata=new List<GPSData>();
    }
}


[System.Serializable]
public class GPSData
{
    public float latitudedata;
     public string fname;
    public string lname;
    public long contactno;
    public string position_lat,position_long;
    public string current_date;

    public string start_time,current_time;
    public float longitudedata;


}



