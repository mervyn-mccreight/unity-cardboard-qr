using System;
using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    // TODO: FH Wedel server URL
    private const string Url = "http://192.168.1.30/cardboard-qr-questionform/api.php?action=get_question_count";

    // Use this for initialization
	IEnumerator Start () {
        var www = new WWW(Url);

        // Wait for download to complete
        yield return www;

        GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format("Fragen: {0}/{1}", GlobalState.UnlockedCoins.Count, www.text);
        GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format("Münzen: {0}/{1}", GlobalState.CollectedCoins.Count, www.text);
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
