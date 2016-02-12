using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnGoClick()
    {
        SceneManager.LoadScene("CameraScene");
    }

    public void OnHelpClick()
    {
        Debug.Log("Help");

    }
}
