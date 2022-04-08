using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalSetting : MonoBehaviour
{

    [SerializeField]
    public Text NEWGOALText;
    [SerializeField]
    public Text TitleText;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        
    }

    public void WarmUp()
    {
       TitleText.text=" Warm-up / Cool – down";
       NEWGOALText.text=SaveData.LoadGoal("goal.txt");
    }
    
    public void Aerobic()
    {
       TitleText.text=" Weekly Aerobic Activity Recommendations";
       NEWGOALText.text=SaveData.LoadGoal("aerobic.txt");
    }
    // Update is called once per frame


    void Update()
    {
        
    }
}
