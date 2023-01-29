using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System;
using Mono.Data.Sqlite;

public static class SaveUserData 
{ 
    static string conn, sqlQuery;
    static IDbConnection dbconn;
    static IDbCommand dbcmd;
    static string SeniorFitDB = "SeniorFitDB.s3db";
    private static IDataReader reader;

    public static void SaveUser(LoginSetUp newlogin)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/loginData.data";
        FileStream stream = new FileStream(path, FileMode.Create);
        UserData data = new UserData(newlogin);
        formatter.Serialize(stream, data);
        stream.Close();

        string filepath = Application.persistentDataPath + "/" + SeniorFitDB;
        Debug.Log("DB path:" + path);

        #if UNITY_ANDROID
            if (!File.Exists(filepath))
            {
                // If not found on android will create Tables and database
                Debug.LogWarning("File \"" + filepath + "\" does not exist. Attempting to create from \"" +
                                Application.dataPath + "!/assets/SeniorFitDB");
                WWW loadDB = new WWW("jar:file://" + Application.dataPath + "!/assets/SeniorFitDB.s3db");
                while (!loadDB.isDone) { }
                // then save to Application.persistentDataPath
                File.WriteAllBytes(filepath, loadDB.bytes);
            }
        #endif

        conn = "URI=file:" + filepath;

        Debug.Log("Stablishing connection to: " + conn);
        dbconn = new SqliteConnection(conn);
        dbconn.Open();
        
        string query1 = "CREATE TABLE ExerciseData (Start_Time TEXT PRIMARY KEY,Date TEXT, Exercise TEXT, ElapsedTime INTEGER, RepCount INTEGER)";
        string query2 = "CREATE TABLE WalkData (StartTime TEXT PRIMARY KEY,Date TEXT, EndTime TEXT, MilesWalked FLOAT)";
        try
        {
            dbcmd = dbconn.CreateCommand(); // create empty command
            dbcmd.CommandText = query1; // fill the command
            reader = dbcmd.ExecuteReader(); // execute command which returns a reader
            reader.Close();

            dbcmd.CommandText = query2; // fill the command
            reader = dbcmd.ExecuteReader(); // execute command which returns a reader
            reader.Close();
            Debug.Log("Tables Created");
        }
        catch (Exception e)
        {
            Debug.Log("Error: Tables were not all set up.");
            Debug.Log(e);
        }
    }


    //*****************change "userdata" to "UserData"***************///////////

    public static void ChangeClassName()
    {
        string path = Application.persistentDataPath + "/user.data";
         string newPath=Application.persistentDataPath + "/loginData.data";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream=new FileStream(path,FileMode.Open,FileAccess.Read);
            userdata previous_data= formatter.Deserialize(stream) as userdata;
            Debug.Log("Previous data "+previous_data.version.ToString());
            stream.Close();
            Debug.Log("STream closed");

            try
            {
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                    Debug.LogException(ex);
            }

           
           
            BinaryFormatter OutputFormatter =new BinaryFormatter();
            FileStream outStream = new FileStream(newPath, FileMode.Create,FileAccess.Write);
            UserData NewData =  new UserData(previous_data);
            OutputFormatter.Serialize(outStream, NewData);
            outStream.Close();
        }
        else
        {
            Debug.Log("File 'user.data' Not Found");
            
        }
    }


     public static UserData LoadUser()
    {
        string path = Application.persistentDataPath + "/user.data";
        string newPath=Application.persistentDataPath + "/loginData.data";
        if(File.Exists(path))
        {
           
            
                Debug.Log("change of userdata class");
                ChangeClassName();
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream newstream=new FileStream(newPath,FileMode.Open,FileAccess.Read);
                UserData data= formatter.Deserialize(newstream) as UserData;
                newstream.Close();
                return data;

        }
        else if(File.Exists(newPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
                FileStream newstream=new FileStream(newPath,FileMode.Open,FileAccess.Read);
                UserData data= formatter.Deserialize(newstream) as UserData;
                newstream.Close();
                return data;

        }
        else
        {
            Debug.Log("File 'user.data' Not Found");
            return null;
        }
    }


     public static void UpdateUserVersion(string newVersion)
    {
        string path = Application.persistentDataPath + "/loginData.data";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path,FileMode.Open,FileAccess.Read);
            UserData data = formatter.Deserialize(stream) as UserData;
            data.version = newVersion;
            Debug.Log("Inside Update Version: " + data.version);            
            stream.Close();

            
            BinaryFormatter OutputFormatter =new BinaryFormatter();
            FileStream outStream = new FileStream(path, FileMode.Create,FileAccess.Write);
            UserData NewData = data;
            OutputFormatter.Serialize(outStream, data);
            outStream.Close();
        }
        else
        {
            Debug.Log("Saved File Not Found");   
        }
    }

   
    
}


// Just used for changing 'userdata' back to 'UserData'////


[System.Serializable]
public class userdata 
{
    public string fname;
    public string lname;
    public long contactno;
    public string version;
    public DateTime LoginTime;

    public userdata(LoginSetUp newlogin)
    {
        fname = newlogin.fname;
        lname = newlogin.lname;
        contactno = newlogin.contactno;
        LoginTime = newlogin.LoginTime;
        version = newlogin.version;
    }
}
   
