using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalSetting : MonoBehaviour
{

    [SerializeField]
    public Text NEWGOALText;
    public Image rpe_image;
   
  

    public string warmup,cooldown;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        
        
    }

    public void WarmUp()
    {
       rpe_image.gameObject.SetActive(false);
       NEWGOALText.text=SaveData.LoadGoal("warmup.txt");
    }
    
    public void WeeklyRecommendation()
    {
       rpe_image.gameObject.SetActive(false);
       NEWGOALText.text=SaveData.LoadGoal("weekly_rec.txt");
    }

    public void Intensity()
    {
       //TitleText.text=" Weekly Strengthening Recommendations";
       NEWGOALText.text=SaveData.LoadGoal("intensity.txt");
       rpe_image.gameObject.SetActive(true);
    }
      public void Type_of_Activity()
    {
        rpe_image.gameObject.SetActive(false);
       //TitleText.text=" Weekly Strengthening Recommendations";
       NEWGOALText.text=SaveData.LoadGoal("types_of_activity.txt");
      
    }

    // Update is called once per frame


    void Update()
    {
        
    }
}
