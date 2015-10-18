﻿using System.Threading;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

public class TextureScript : MonoBehaviour {
    private WebCamTexture webcamTexture;
    private WebCamDevice backFacing;
    private Thread qrCodeThread;
    private bool runThread = true;

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
            webcamTexture = new WebCamTexture(backFacing.name, 1024, 768);
   
            GetComponent<Renderer>().material.SetTexture("_MainTex", webcamTexture);
            webcamTexture.Play();

            qrCodeThread = new Thread(DecodeQR);
            qrCodeThread.Start();

        } else
        {
            Debug.LogError("No User Authorization for Camera Device.");
        }
    }

    void DecodeQR()
    {
        while (runThread)
        {
            BarcodeReader reader = new BarcodeReader();
            Result result = reader.Decode(webcamTexture.GetPixels32(), webcamTexture.width, webcamTexture.height);
            if (result != null)
            {
                ResultPoint[] points = result.ResultPoints;
                Vector3 start = new Vector3((points[0].X - webcamTexture.width / 2) / (float)webcamTexture.width * 1.334f * 10 * -1, 0, (points[0].Y - webcamTexture.height / 2) / (float)webcamTexture.height * 10);
                Vector3 end1 = new Vector3((points[1].X - webcamTexture.width / 2) / (float)webcamTexture.width * 1.334f * 10 * -1, 0, (points[1].Y - webcamTexture.height / 2) / (float)webcamTexture.height * 10);
                Vector3 end2 = new Vector3((points[2].X - webcamTexture.width / 2) / (float)webcamTexture.width * 1.334f * 10 * -1, 0, (points[2].Y - webcamTexture.height / 2) / (float)webcamTexture.height * 10);
                Debug.Log(start);
                Debug.Log(end1);
                Debug.Log(end2);

                GameObject[] objects = GameObject.FindGameObjectsWithTag("c1");

                objects[0].transform.localPosition = start;
                objects[1].transform.localPosition = end1;
                objects[2].transform.localPosition = end2;

                Debug.Log(result.ToString());
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
    }

    // Sent to all game objects before the application is quit.
    void OnApplicationQuit()
    {
        runThread = false;
    }

    // This function is called when the MonoBehaviour will be destroyed.
    void OnDestroy()
    {
        qrCodeThread.Abort();
        webcamTexture.Stop();
    }
}
