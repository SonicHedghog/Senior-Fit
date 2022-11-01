using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Mono.Data.Sqlite;
using System.Data;


public class ActivityDetails : MonoBehaviour
{
     public string date,start_time,message="";
    
    public Sprite exerciseSprite;
    public Sprite walkSprite;

    //*******sqlite database
    private SqliteConnection dbconn;
    private string conn;
    private SqliteCommand dbcmd;
    private string sqlQuery;
    string filepath;
    private static IDataReader reader;
    private GameObject DetailsButton;
    private GameObject g;
    public static string queryDate="";
    public Button infoButton;
    private int checkIfFirstTime=0;
    
   // private GameObject activityContainer;
   // public GameObject activityDetails;
    public TextMeshProUGUI details;
    private Dictionary<string, List<string>> dictionary =
    new Dictionary<string,List<string>>();
   
    // Start is called before the first frame update
    void Start()
    {
        
        Debug.Log("start method called "+queryDate);
        checkIfFirstTime=1;
        
       filepath = Application.persistentDataPath + "/SeniorFitDB.s3db";
        conn = "URI=file:" + filepath;
       // DetailsButton=transform.GetChild(0).gameObject;
       //updateDetails(queryDate);
    }

    void OnEnable()
    {
        Destroy(DetailsButton);
        Invoke("callUpdate", 2.0f);
        
       
    }
    void callUpdate()
    {
        if(checkIfFirstTime!=0)
        {
        
        Debug.Log("enabled "+queryDate);
        
        DetailsButton=transform.GetChild(0).gameObject;
        
        updateDetails(queryDate);

        }

    }
    
     void updateDetails(string date)
    {
        
        
         // Save/Update contents to SQLite DB
        using (dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
                    
		//string query ="SELECT * FROM ExerciseData";
        string query="SELECT DISTINCT Exercise FROM ExerciseData where Date=\"" +date+"\"" ;
        Debug.Log(query);
		dbcmd.CommandText = query;
		reader = dbcmd.ExecuteReader();
        Debug.Log("From database "+reader.ToString());

		while (reader.Read())
		{
			Debug.Log("from database: " + reader[0].ToString());

            g=Instantiate(DetailsButton,transform);

            string eName=reader[0].ToString();
            
            
            Debug.Log(eName);
            

            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=eName;
            g.transform.GetComponent<Button>().onClick.AddListener(()=>detailsButtonClick(eName,date));

            Debug.Log(g);
			
		}
        dbconn.Close();}

        //********get walk data***************//

        using (dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
                    

        string query1="SELECT * FROM WalkData where Date=\"" +date+"\"";
        
		dbcmd.CommandText = query1;
		reader = dbcmd.ExecuteReader();
       Debug.Log("walk record "+reader[0]);

		if (reader.Read())
		{
			Debug.Log("from walking database: " + reader[0].ToString());

            g=Instantiate(DetailsButton,transform);

            string eName="Outdoor Walking";
            
            

            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=eName;
            g.transform.GetComponent<Button>().onClick.AddListener(()=>detailsButtonClick(eName,date));

            Debug.Log(g);
			
		}

		// Close connection
		dbconn.Close();


                }
                Destroy(DetailsButton);
    }

    public void setData(string date){

        
        queryDate=date;
        Debug.Log("date set:"+queryDate);
         


    }

    public void detailsButtonClick(string name,string date)
    {
        string detail="";
        Debug.Log("details for "+name+" "+date);
        if(name!="Outdoor Walking")
        {
            

        infoButton.GetComponent<Image>().overrideSprite = exerciseSprite;

         using (dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
                    
		//string query ="SELECT * FROM ExerciseData";
        string query="SELECT * FROM ExerciseData where Exercise=\"" +name+"\" and Date=\"" +date+"\"" ;
        Debug.Log(query);
		dbcmd.CommandText = query;
		reader = dbcmd.ExecuteReader();
        Debug.Log("From database "+reader.ToString());

		while (reader.Read())
		{
			Debug.Log("from database: " + reader[0].ToString());

            detail+="Start time: "+reader[0].ToString()+"\n"+"Duration: "+reader[3].ToString()+"\n"+"Repetitions: "+reader[4].ToString()+"\n\n\n";
            
            details.text=detail;
            Debug.Log(detail);
			
		}

		// Close connection
		dbconn.Close();


         }

        }
        else
        {
            infoButton.GetComponent<Image>().overrideSprite = walkSprite;
             using (dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
                    
		//string query ="SELECT * FROM ExerciseData";
        string query="SELECT * FROM WalkData where Date=\"" +date+"\"" ;
        Debug.Log(query);
		dbcmd.CommandText = query;
		reader = dbcmd.ExecuteReader();
        Debug.Log("From database "+reader.ToString());

		while (reader.Read())
		{
			Debug.Log("from walking database: count");

            detail+="Start time: "+reader[0].ToString()+"\n"+"End Time: "+reader[2].ToString()+"\n"+"Miles Walked: "+reader[3].ToString()+"\n\n\n";
            
            details.text=detail;
            Debug.Log(g);
			
		}

		// Close connection
		dbconn.Close();


         }

        }
        
    }
}
