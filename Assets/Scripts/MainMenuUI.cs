using System;
using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class MainMenuUI : MonoBehaviour
{
    // Use this for initialization
	IEnumerator Start () {
		// do not destroy this instance on scene changes.

        var www = new WWW(Config.ApiUrlQuestionCount);

        // Wait for download to complete
        yield return www;

        GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format("Fragen: {0}/{1}", GlobalState.Instance.UnlockedCoinCount(), www.text);
        GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format("Münzen: {0}/{1}", GlobalState.Instance.CollectedCoinCount(), www.text);
    }
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Escape))
	    {
			GlobalState.Save();
            Application.Quit();
        }
    }

    public void OnGoClick() {
        SceneManager.LoadScene(Config.CameraScene);
    }

    public void OnHelpClick()
    {
        Debug.Log("Help");
    }
}
