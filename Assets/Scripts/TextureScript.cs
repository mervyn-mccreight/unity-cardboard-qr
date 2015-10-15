using UnityEngine;
using System.Collections;

public class TextureScript : MonoBehaviour {
    private Renderer renderer;
    private WebCamTexture webcamTexture;

    // Use this for initialization
    void Start () {
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("access to webcam granted!");
            webcamTexture = new WebCamTexture();
            webcamTexture.Play();
            renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = webcamTexture;
        } else
        {
            Debug.Log("No User Authorization for Camera Device.");
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (webcamTexture.didUpdateThisFrame)
        {
            renderer.material.mainTexture = webcamTexture;
        }
	}
}
