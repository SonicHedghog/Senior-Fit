using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;




public class UpdateGPS : MonoBehaviour
{
    public Text Lat;
    public Text Long;
    public Text steps;
    void Update()
    {
        Lat.text= "Lat: "+ Walk.Instance.latitude.ToString();
        Long.text="Long: "+ Walk.Instance.longitude.ToString();
       
    }

    void FixedUpdate()
    {
         if(StepCounter.current.enabled)
        {
            
            steps.text= $"Steps: {StepCounter.current.stepCounter.ReadValue()}";
            Debug.Log("step counter value: "+ StepCounter.current.stepCounter.ReadValue());


        }
        
        else
       steps.text = "999";
    }

}
