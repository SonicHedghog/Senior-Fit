using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class Walk : MonoBehaviour
{
    public static Walk Instance {set; get; }
    public float latitude;
    public float longitude;

    public int stepcount;

    
    // Start is called before the first frame update
    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        StartCoroutine(StartLocationService());
        
 
       
 
    }

     private void Awake()
    {
        InputSystem.EnableDevice(StepCounter.current);
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    public void backbutton()
    {
         SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator StartLocationService()
    {
        if(!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        Input.location.Start();
        int maxWait=20;

        while(Input.location.status == LocationServiceStatus.Initializing && maxWait>0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if(maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }
        if(Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine location");
            yield break;
        }

        latitude=Input.location.lastData.latitude;
        longitude=Input.location.lastData.longitude;

        if(StepCounter.current.enabled)
        {
            
            stepcount=StepCounter.current.stepCounter.ReadValue();
            Debug.Log("step counter value: "+ StepCounter.current.stepCounter.ReadValue());


        }
        
        else
        stepcount=999;

       

        yield break;
    }

    
}
