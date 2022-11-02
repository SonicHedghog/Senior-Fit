using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Mono.Data.Sqlite;
using System.Data;

public class ActivityRecord : MonoBehaviour
{
   
    public string date,start_time,message="";
    
    public Sprite exerciseSprite;

    //*******sqlite database
     private SqliteConnection dbconn;
    private string conn;
    private SqliteCommand dbcmd;
    private string sqlQuery;
    string filepath;
    private static IDataReader reader;
    private GameObject activityButton;
    private GameObject g;
    public static string setDate="";
    
   // private GameObject activityContainer;
   // public GameObject activityDetails;
    public TextMeshProUGUI exerciseName;
    private Dictionary<string, List<string>> dictionary =
    new Dictionary<string,List<string>>();
   // private string queryDate;
   
    // Start is called before the first frame update
    void Start()
    {
         activityButton=transform.GetChild(0).gameObject;
         

        filepath = Application.persistentDataPath + "/SeniorFitDB.s3db";
        conn = "URI=file:" + filepath;

        updateRecord();

        
    }

    void updateRecord()
    {
        List<string>unique_dates=new List<string>();
         // Save/Update contents to SQLite DB
        using (dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
        string query="SELECT DISTINCT Date FROM (SELECT Date FROM ExerciseData UNION SELECT Date FROM WalkData)";
        
		//string query ="SELECT DISTINCT Date FROM ExerciseData";
		dbcmd.CommandText = query;
		reader = dbcmd.ExecuteReader();
        Debug.Log("From database "+reader.ToString());



		while (reader.Read())
		{
            unique_dates.Add(reader[0].ToString());
            Debug.Log("unique date "+reader[0].ToString());
				
		}

		// Close connection
		dbconn.Close();

                }

        foreach (var item in unique_dates)
            {
               
                int exerciseCount=0,walkcount=0;
                 Debug.Log("new unique date "+item);
                using (dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open(); //Open connection to the database.
                    dbcmd = dbconn.CreateCommand();
                    string query="SELECT COUNT(DISTINCT Exercise) FROM ExerciseData WHERE Date=\"" +item+"\"";                         
		                //string query ="SELECT DISTINCT Date FROM ExerciseData";
		            dbcmd.CommandText = query;
		            reader = dbcmd.ExecuteReader();
                    exerciseCount=Int16.Parse(reader[0].ToString());
                    Debug.Log("From database exercise count"+reader[0].ToString());
                    dbconn.Close();
                }

                using (dbconn = new SqliteConnection(conn))
                    {
                        dbconn.Open(); //Open connection to the database.
                        dbcmd = dbconn.CreateCommand();

                        string query2="SELECT * FROM WalkData WHERE Date=\"" +item+"\""; 
                        dbcmd.CommandText = query2;
		                reader = dbcmd.ExecuteReader();
                        if(reader.Read())
                        {
                             walkcount=1;
                        }
                       
                        Debug.Log("From database walk count"+walkcount);
                       
                        dbconn.Close();
                    }
                string summary="";

                if(exerciseCount!=0 && walkcount!=0)
                    {
                        summary="\nexercise: "+exerciseCount.ToString()+"\nOutdoor Walking";

                    }
                else{
                    if(walkcount==0)
                        summary="\nexercise: "+exerciseCount.ToString();

                    if(exerciseCount==0)
                        summary="\nOutdoor Walking";

                    }

     

                            g=Instantiate(activityButton,transform);

                            //string date_time=reader[0].ToString();
                            System.DateTime dateTime = System.DateTime.Parse(item);
            
                            date=dateTime.ToString("dddd, MMMM d, yyyy");
                            
                            string queryDate=item;
                            Debug.Log("distinct date "+queryDate);
                            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=date+summary;
                            //g.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text=message;
                            g.transform.GetComponent<Button>().onClick.AddListener(()=>activityButtonClick(queryDate));
                            //ActivityDetails activityDetails=new ActivityDetails();
                            //g.transform.GetComponent<Button>().onClick.AddListener(()=>activityDetails.updateDetails(queryDate));

                            Debug.Log(g);
			
		                
            }
        
        
        Destroy(activityButton);
    }

    void activityButtonClick(string date)
    {
        setDate=date;
        //ActivityDetails activityDetails=new ActivityDetails();
        ActivityDetails.queryDate=date;
        //activityDetails.setData(date);
        
        Debug.Log("button clicked "+date);
        //Debug.Log(activityContainer);
        //exerciseName.text=dictionary[date][0];
        //activityContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=dictionary[date][0];}
    }

    public static string getDate()
    {
        return setDate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
