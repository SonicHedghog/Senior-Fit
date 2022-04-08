﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadPlugins : MonoBehaviour
{
    [SerializeField]
    public Text latText;
    [SerializeField]
    public Text distanceText;
    public Text GPSStatus;
    public Text timestamp;

    #if UNITY_ANDROID

    private ServiceManager locationService; 
    private LocationPlugin locationPlugin;

    #elif UNITY_IOS

    // iOS code goes here

    #endif

    void OnEnable()
    {
        #if UNITY_ANDROID

        locationPlugin = this.gameObject.AddComponent(typeof(LocationPlugin)) as LocationPlugin;

        locationService = this.gameObject.AddComponent(typeof(ServiceManager)) as ServiceManager;
        
        locationService.latText = latText;
        locationService.distanceText = distanceText;
        locationService.GPSStatus = GPSStatus;
        locationService.timestamp=timestamp;


        #elif UNITY_IOS

        // iOS code goes here

        #endif
    }

    public void startPlugin()
    {
        #if UNITY_ANDROID

        locationService.OnStartLocationServiceBtn();


        #elif UNITY_IOS

        // iOS code goes here

        #endif
    }
     public void stopPlugin()
    {
        #if UNITY_ANDROID

        locationService.OnStopLocationServiceBtn();


        #elif UNITY_IOS

        #endif
    }
}