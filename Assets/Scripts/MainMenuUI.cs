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
            // do not destroy this instance on scene changes.

            var www = new WWW(Config.ApiUrlQuestionCount);

            // Wait for download to complete
            yield return www;

            GameObject.Find("QuestionProgressText").GetComponent<Text>().text = string.Format("Fragen: {0}/{1}",
                GlobalState.Instance.UnlockedCoinCount(), www.text);
            GameObject.Find("CoinProgressText").GetComponent<Text>().text = string.Format("Münzen: {0}/{1}",
                GlobalState.Instance.CollectedCoinCount(), www.text);

            // TODO: show win screen
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GlobalState.Save();
                Application.Quit();
            }
        }

        public void OnGoClick()
        {
            SceneManager.LoadScene(Config.CameraScene);
        }

        public void OnHelpClick()
        {
            // TODO: consider help
            Debug.Log("Help");
        }
    }
}