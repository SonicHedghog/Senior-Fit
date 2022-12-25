using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Video;
using UnityEngine.EventSystems;
public class VidelTutorial : MonoBehaviour, IDragHandler, IPointerDownHandler
{
   

    public UnityEngine.Video.VideoPlayer videoPlayer;
    public string videoPath;
    public Button play;
    public Button pause;
    public Button back;
    public Text name;
    //public GameObject tutorialType;

    public DateTime startTime;

    private bool ScreenTouch=false;
    public Image progress;

    private void Awake()
    {
       progress =  progress.gameObject.GetComponent<Image>();
    }
 
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SetTutorialAddress();
    }

    // Update is called once per frame
    void Update()
    {
        if (videoPlayer.frameCount > 0)
            progress.fillAmount = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
        float waitFor=5;
        if (Input.GetMouseButton(0))
        {
            ScreenTouch=true;
            //videoPlayer.Pause();
            play.gameObject.SetActive(true);
            pause.gameObject.SetActive(true);
            back.gameObject.SetActive(true);
            name.gameObject.SetActive(true);
            startTime=DateTime.Now;
                       
            
        }
         DisableButton(startTime);
    }
    public void DisableButton(DateTime start)
    {
        DateTime now=DateTime.Now;
        Debug.Log("time :"+(now-start).TotalSeconds.ToString());
        if((now-start).TotalSeconds>3 && ScreenTouch==true)
        {
            play.gameObject.SetActive(false);
            pause.gameObject.SetActive(false);
            back.gameObject.SetActive(false);
            name.gameObject.SetActive(false);
             ScreenTouch=false;
             
        }
           

    }

       public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(eventData.position.ToString());
        TrySkip(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TrySkip(eventData);
    } 

    private void TrySkip(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform, eventData.position, null, out localPoint))
        {
            float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax, localPoint.x);
            Debug.Log(progress.rectTransform.rect.xMin.ToString());
            SkipToPercent(pct);
        }
    }

    private void SkipToPercent(float pct)
    {
        var frame = videoPlayer.frameCount * pct;
        videoPlayer.frame = (long)frame;
    }

    public void SetTutorialAddress()
        {
            int tutorialNo = SceneChange.GetTutorialNumber();
switch (tutorialNo) 
{
  case 1:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/ShoulderTouchInstructions.mp4";
    name.text="Shoulder Touch";
    break;
  case 2:
    videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/ChairSitStandInstructions.mp4";
    name.text="Chair Sit to Stand";
    break;
  case 3:
    videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/SingleLegStanceInstructions.mp4";
    name.text="Single Leg Stance";
    break;
  case 4:
    videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/SeatedHamstringStretchInstructions.mp4";
    name.text="Seated Hamstring Stretch";
    break;
  case 5:
    videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/MarchingInPlaceInstructions.mp4";
    name.text="Marching in Place";
    break;
  case 6:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/HeeltoToeWalkingInstructions.mp4";
    name.text="Heel to Toe Walking";
    break;
  case 7:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/RocktheBoatInstructions.mp4";
    name.text="Rock the Boat";
    break;
  case 8:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/ShoulderStretchInstructions.mp4";
    name.text="Shoulder Stretch with Towel";
    break;
  case 9:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/SideSteppingCrossoversInstructions.mp4";
    name.text="Side Stepping Crossovers";
    break;
  case 10:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/StandingCalfStretchInstructions.mp4";
    name.text="Standing Calf Stretch";
    break;
  case 11:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/StandingLegCurlInstructions.mp4";
    name.text="Standing Leg Curl";
    break;
  case 12:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/StandingThighStretchInstructions.mp4";
    name.text="Standing Thigh Stretch";
    break;
  case 13:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/WalkingInstructions.mp4";
    name.text="Walking";
    break;
  case 14:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/WalkingwithArmSwingsInstructions.mp4";
    name.text="Walking with Arm Swings";
    break;
  case 15:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/WalkingwithHighKneesInstructions.mp4";
    name.text="Walking with High Knees";
    break;
  case 16:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/WallPushUpInstructions.mp4";
    name.text="Wall Push-Up";
    break;
  default:
    videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/ShoulderTouchInstructions.mp4";
    name.text="Shoulder Touch";
    break;

}
            /*if(SceneChange.GetTutorialNumber()==1)
            {
                 videoPath=Application.streamingAssetsPath +"/TutorialClips/InstructionalVideos/ShoulderTouchInstructions.mp4";
                 name.text="Shoulder Touch";

            }
            else if(SceneChange.GetTutorialNumber()==2)
            {
                videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/ChairSitStandInstructions.mp4";
                 name.text="Chair Sit to Stand";

            }
            else if(SceneChange.GetTutorialNumber()==3)
            {
                videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/SingleLegStanceInstructions.mp4";
                 name.text="Single Leg Stance";

            }
            else if(SceneChange.GetTutorialNumber()==4)
            {
                videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/SeatedHamstringStretchInstructions.mp4";
                 name.text="Seated Hamstring Stretch";

            }
            else if(SceneChange.GetTutorialNumber()==5)
            {
                videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/MarchingInPlaceInstructions.mp4";
                 name.text="Marching in Place";

            }
            else
            videoPath=Application.streamingAssetsPath+"/TutorialClips/InstructionalVideos/ShoulderTouchInstructions.mp4";
           */


            videoPlayer.url =   videoPath;
             videoPlayer.Play();

        }

    public void PauseTutorial()
    {
        videoPlayer.Pause();
    }
    public void PlayTutorial()
    {
        videoPlayer.Play();
        
    }
    public void backButton()
    {
        SceneManager.LoadScene("MainMenu");
        //tutorialType.SetActive(true);
    }
}
