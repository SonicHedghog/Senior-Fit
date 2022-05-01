using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveUserData 
{ 
    

    public static void SaveUser(loginscript newlogin)
    {
        BinaryFormatter formatter =new BinaryFormatter();
        string path = Application.persistentDataPath + "/user.data";
        FileStream stream= new FileStream(path, FileMode.Create);

        userdata data= new userdata(newlogin);

        formatter.Serialize(stream, data);
        stream.Close();

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
