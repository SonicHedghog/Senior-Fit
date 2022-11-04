using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Mono.Data.Sqlite;
using System.Data;
using System.Linq;


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
    public static string d="";
    
    public Button infoButton;
    public Button backButton;
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
        //checkIfFirstTime=1;
        
       filepath = Application.persistentDataPath + "/SeniorFitDB.s3db";
        conn = "URI=file:" + filepath;
       // DetailsButton=transform.GetChild(0).gameObject;
       //updateDetails(queryDate);
    }

      void OnEnable()
    {
        Destroy(g);
         backButton.gameObject.SetActive(false);
       DetailsButton=transform.GetChild(0).gameObject;
       DetailsButton.GetComponent<Button>().enabled=false;
       
        
        
       Debug.Log("enabled "+queryDate);
      
        Invoke("callUpdate", 1.0f);

        
        
        
       
    }
    void callUpdate()
    {
        //if(checkIfFirstTime!=0)
        {
           
         g=Instantiate(DetailsButton,transform);
        
        Debug.Log("enabled "+queryDate);
        
       // DetailsButton=transform.GetChild(0).gameObject;
        
        updateDetails(queryDate);
        

        }

    }
    void OnDisable()
    {
        Debug.Log("what is g"+g);
    
        var count = transform.childCount;
        for (var i = 1; i != count; ++i)
            Destroy(transform.GetChild(i).gameObject);
         DetailsButton.gameObject.SetActive(true);
        
         
        
        
    }

    
     public void updateDetails(string date)
    {
        Debug.Log("update details called");
        Destroy(g);
        backButton.gameObject.SetActive(true); 

        
         

      
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
            DetailsButton.gameObject.SetActive(true);

            DetailsButton.GetComponent<Button>().enabled=true;

            g=Instantiate(DetailsButton,transform);
          DetailsButton.gameObject.SetActive(false);

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

            DetailsButton.gameObject.SetActive(true);

            

            g=Instantiate(DetailsButton,transform);
            DetailsButton.gameObject.SetActive(false);

            string eName="Outdoor Walking";
            
            

            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=eName;
            g.transform.GetComponent<Button>().onClick.AddListener(()=>detailsButtonClick(eName,date));

            Debug.Log(g);
            
			
		}

		// Close connection
		dbconn.Close();


                }
                
                //Destroy(DetailsButton);
                
              
    }

    public void setData(string date){

        
        queryDate=date;
        Debug.Log("date set:"+queryDate);
         


    }

     public string SetTime(int Time_duration)
     {
    
        string timeText="";
        int hours,minutes,seconds;
        
        //if (Time_duration > 0)
        {

            hours = (int)(Time_duration / 3600);
            minutes = ((Time_duration % 3600) / 60);
            seconds = (int)(Time_duration % 60);

            if (hours > 0)
                timeText = $"{hours} hrs {(int)minutes} mins {seconds} seconds";
            else if(hours==0 && minutes>0)
            {
                timeText = $"{(int)minutes} mins {seconds} seconds";

            }
            else
            {
                timeText = $"{seconds} seconds";
            }
        }
        return timeText;
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
            string durationText=SetTime(Int32.Parse(reader[3].ToString()));
            string repText;
            System.DateTime startTime = System.DateTime.Parse(reader[0].ToString());
            
            string startTimeText=startTime.ToString("MM/dd/yyyy hh:mm:ss tt");
            Debug.Log("no of repcounts "+Int32.Parse(reader[3].ToString()));
            if(Int32.Parse(reader[4].ToString())==-1)
            {
                repText="No camera workout";
            }
            else
            {
                repText=reader[4].ToString();
            }
            
			Debug.Log("from database exercise duration: " + durationText);


            detail+="Start time: "+startTimeText+"\n"+"Duration: "+durationText+"\n"+"Repetitions: "+repText+"\n\n\n";
            
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
            System.DateTime startTime = System.DateTime.Parse(reader[0].ToString());
            
            string startTimeText=startTime.ToString("MM/dd/yyyy hh:mm:ss tt");
            System.DateTime endTime = System.DateTime.Parse(reader[2].ToString());
            
            string endTimeText=endTime.ToString("MM/dd/yyyy hh:mm:ss tt");

            detail+="Start time: "+startTimeText+"\n"+"End Time: "+endTimeText+"\n"+"Miles Walked: "+reader[3].ToString()+"\n\n\n";
            
            details.text=detail;
            Debug.Log(g);
			
		}

		// Close connection
		dbconn.Close();


         }

        }
        
    }

   
}
