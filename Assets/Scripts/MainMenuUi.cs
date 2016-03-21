using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    
    /// <summary>
    /// Controls the main menu behavior. Is attached to Unity MainMenuScene.Canvas.
    /// </summary>
    public class MainMenuUi : MonoBehaviour
    {
        protected virtual IEnumerator Start()
        {
            Application.targetFrameRate = 30;
            GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;

            // Initialize all text fields and button texts.
            GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format(StringResources.QuestionProgressText, "?", "?");
            GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format(StringResources.CoinProgressText, "?", "?");
            GameObject.Find("TitleText").GetComponent<Text>().text = StringResources.MainMenuHeading;
            GameObject.Find("GoButton").GetComponentInChildren<Text>().text = StringResources.GoButtonText;
            GameObject.Find("HelpButton").GetComponentInChildren<Text>().text = StringResources.HelpButtonText;

            // Load questions from API.
            var questionsWww = new WWW(Config.ApiUrlQuestions);

            // Wait for download to complete
            yield return questionsWww;

            // Store loaded questions in GlobalState.
            GlobalState.Instance.AllQuestions = JsonUtility.FromJson<Questions>(questionsWww.text);

            questionsWww.Dispose();

            // Update progress texts.
            GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format(StringResources.QuestionProgressText,
                GlobalState.Instance.UnlockedCoinCount(), GlobalState.Instance.AllQuestions.questions.Length);
            GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format(StringResources.CoinProgressText,
                GlobalState.Instance.CollectedCoinCount(), GlobalState.Instance.AllQuestions.questions.Length);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
				SaveStateAndCloseApplication();
            }
        }

		public void SaveStateAndCloseApplication() 
		{
			GlobalState.Save();
			Application.Quit();
		}

        /// <summary>
        /// Switches to CameraScene. Is attached to Unity MainScene.GoButton.
        /// </summary>
        public void OnGoClick()
        {
            SceneManager.LoadScene(Config.SceneName(Config.Scenes.Camera));
        }

        /// <summary>
        /// Switches to HelpScene. Is attached to Unity MainScene.HelpButton.
        /// </summary>
        public void OnHelpClick()
        {
			SceneManager.LoadScene(Config.SceneName(Config.Scenes.Help));
        }
    }
}