using System.Collections.Generic;
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

    public static void SaveUser(loginscript newlogin)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/user.data";
        FileStream stream = new FileStream(path, FileMode.Create);

        userdata data = new userdata(newlogin);

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
        string query2 = "CREATE TABLE WalkData (StartTime TEXT PRIMARY KEY,Date TEXT, EndTime TEXT, MilesWalked INTEGER)";
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

    


    public static userdata LoadUser()
    {
        string path = Application.persistentDataPath + "/user.data";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream=new FileStream(path,FileMode.Open);
            userdata data= formatter.Deserialize(stream) as userdata;
            stream.Close();
            return data;

        }
        else
        {
            Debug.Log("Saved file not found");
            return null;
        }
    }

     public static void UpdateUserVersion(string newVersion)
    {
         string path = Application.persistentDataPath + "/user.data";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream=new FileStream(path,FileMode.Open);
            userdata data= formatter.Deserialize(stream) as userdata;
            data.version=newVersion;
            Debug.Log("inside update version : "+data.version);            
            stream.Close();

            
                BinaryFormatter OutputFormatter =new BinaryFormatter();
        
        FileStream outStream= new FileStream(path, FileMode.Create);

        userdata NewData= data;

        OutputFormatter.Serialize(outStream, data);
        outStream.Close();

            
            
           

        }
        else
        {
            Debug.Log("Saved file not found");
            
        }
        

    }


    


    
}
