using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using UnityEngine.UI;

public class HelpUi : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		GameObject.Find ("HelpText").GetComponent<Text> ().text = StringResources.HelpText;
		GameObject.Find ("TitleText").GetComponent<Text> ().text = StringResources.HelpSceneHeading;
		GameObject.Find ("BackButton").GetComponentInChildren<Text>().text = StringResources.BackButtonText;
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
