using UnityEngine;
using System.Collections;

public class TextureScript : MonoBehaviour {
    private WebCamTexture webcamTexture;
    private WebCamDevice backFacing;

    // Use this for initialization
    void Start () {
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("access to webcam granted!");
            Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0).ToString());

            foreach (WebCamDevice device in WebCamTexture.devices)
            {
                Debug.Log("WebCamDevice: " + device.name);
                Debug.Log("FrontFacing? " + device.isFrontFacing);
                if (!device.isFrontFacing)
                {
                    backFacing = device;
                }
            }

            webcamTexture = new WebCamTexture(backFacing.name);
            GetComponent<Renderer>().material.SetTexture("_MainTex", webcamTexture);
            webcamTexture.Play();
        } else
        {
            Debug.LogError("No User Authorization for Camera Device.");
        }
    }
	
	// Update is called once per frame
	void Update () {
	}
}
