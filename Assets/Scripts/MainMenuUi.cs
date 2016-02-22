using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MainMenuUi : MonoBehaviour
    {
        // Use this for initialization
        protected virtual IEnumerator Start()
        {
            Application.targetFrameRate = 30;
            GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;

            GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format(StringResources.QuestionProgressText, "?", "?");
            GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format(StringResources.CoinProgressText, "?", "?");
            GameObject.Find("TitleText").GetComponent<Text>().text = StringResources.MainMenuHeading;
            GameObject.Find("GoButton").GetComponentInChildren<Text>().text = StringResources.GoButtonText;
            GameObject.Find("HelpButton").GetComponentInChildren<Text>().text = StringResources.HelpButtonText;

            var questionsWww = new WWW(Config.ApiUrlQuestions);

            // Wait for download to complete
            yield return questionsWww;

            GlobalState.Instance.AllQuestions = JsonUtility.FromJson<Questions>(questionsWww.text);

            questionsWww.Dispose();

            GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format(StringResources.QuestionProgressText,
                GlobalState.Instance.UnlockedCoinCount(), GlobalState.Instance.AllQuestions.questions.Length);
            GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format(StringResources.CoinProgressText,
                GlobalState.Instance.CollectedCoinCount(), GlobalState.Instance.AllQuestions.questions.Length);

            // TODO: show win screen
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
				SaveStateAndCloseApplication ();
            }
        }

		public void SaveStateAndCloseApplication() 
		{
			GlobalState.Save();
			Application.Quit();
		}

        public void OnGoClick()
        {
            SceneManager.LoadScene(Config.SceneName(Config.Scenes.Camera));
        }

        public void OnHelpClick()
        {
			SceneManager.LoadScene(Config.SceneName(Config.Scenes.Help));
        }
    }
}