using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using UnityEngine.UI;

public class HelpUi : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		GameObject.Find ("HelpText").GetComponent<Text> ().text = StringResources.HelpText;
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
