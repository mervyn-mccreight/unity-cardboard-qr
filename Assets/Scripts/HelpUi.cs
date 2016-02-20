using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Assets.Scripts;

public class HelpUi : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void OnClickBackButton() 
	{
		SceneManager.LoadScene(Config.SceneName(Config.Scenes.MainMenu));
	}
}
