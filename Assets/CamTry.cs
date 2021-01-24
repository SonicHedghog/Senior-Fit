using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;

public class CamTry : MonoBehaviour
{
    // Start is called before the first frame update
    WebCamTexture webcamTexture;
    [SerializeField] RawImage cameraView = null;

    void Start()
    {
        string cameraName = WebCamUtil.FindName();
        webcamTexture = new WebCamTexture(cameraName, Screen.width, Screen.height);
        webcamTexture.Play();
        cameraView.texture = webcamTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
