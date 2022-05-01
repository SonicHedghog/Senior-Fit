﻿using System.Collections;
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
            if(SceneChange.GetTutorialNumber()==1)
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
    }
}
