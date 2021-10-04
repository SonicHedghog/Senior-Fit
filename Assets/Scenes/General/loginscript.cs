using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;*/

public class loginscript : MonoBehaviour
{
    // Start is called before the first frame update
    public Button loginButton;
    public InputField firstname;
    public InputField lastname;

    public InputField ContactNumber;
    public long contactno;
    public string fname;

    public string lname;
    

     
        //**********************************************************************
    
    void Start()
    {
        //UnityInitializer.AttachToGameObject(this.gameObject);
        loginButton.onClick.AddListener(LoginButtonClick);
        //load.onClick.AddListener(loadUser);

        //AWSConfigs.HttpClient= AWSConfigs.HttpClientOption.UnityWebRequest;
    }

    

    void LoginButtonClick()
    {
        contactno= long.Parse(ContactNumber.text);
        fname=firstname.text;
        lname=lastname.text;
        SaveUserData.SaveUser(this);
        SceneManager.LoadScene("MainMenu");
        
        
    }
    public void loadUser()
    {
        userdata data = SaveUserData.LoadUser();

        Debug.Log("Saved user name : "+ data.fname+ " and number : "+ data.contactno);
    }


   /* void describeTableListener()
    {
        LoginInfo newUser = new LoginInfo
        {
            FirstName="John",
            LastName="McCarthy"
        };
        Context.SaveAsync(newUser,(result)=>{
                if(result.Exception == null)
                    Debug.Log("book saved");
            });
 
        var request = new DescribeTableRequest
       {
           TableName = @"Demo"
       };
       Client.DescribeTableAsync(request, (result) =>
       {
               if (result.Exception != null)
               {
                       //resultText.text += result.Exception.Message;
                       Debug.Log(result.Exception);
                       return;
               }
               var response = result.Response;
               TableDescription description = response.Table;
               Debug.Log ("Name: " + description.TableName + "\n");
               Debug.Log ("# of items: " + description.ItemCount + "\n");
               Debug.Log ("Provision Throughput (reads/sec): " +
                   description.ProvisionedThroughput.ReadCapacityUnits + "\n");
               Debug.Log ("Provision Throughput (reads/sec): " +
                   description.ProvisionedThroughput.WriteCapacityUnits + "\n");

       }, null);

      
   }*/
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
