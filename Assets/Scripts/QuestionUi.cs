using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class QuestionUi : MonoBehaviour
    {
        private Question _question;

        public AudioSource AudioCorrect;
        public AudioSource AudioFail;

        protected virtual void Start()
        {
            GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;

            _question = GlobalState.Instance.AllQuestions.questions.First(x => x.id == GlobalState.Instance.CurrentQuestion).ToQuestion();

            // init UI
            GameObject.Find("QuestionText").GetComponent<Text>().text = _question.Text;
            GameObject.Find("Answer1Button").GetComponentInChildren<Text>().text = _question.Answers[0];
            GameObject.Find("Answer2Button").GetComponentInChildren<Text>().text = _question.Answers[1];
            GameObject.Find("Answer3Button").GetComponentInChildren<Text>().text = _question.Answers[2];
            GameObject.Find("Answer4Button").GetComponentInChildren<Text>().text = _question.Answers[3];
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GlobalState.Instance.CurrentQuestion = -1;
                SceneManager.LoadScene(Config.SceneName(Config.Scenes.Camera));
            }
        }

        public void OnAnswerClick(int answerIndex)
        {
            var button = GameObject.Find(string.Format("Answer{0}Button", answerIndex+1)).GetComponent<Button>();
            var colors = button.colors;
            if (_question != null && _question.CorrectAnswer == answerIndex)
            {
                GlobalState.Instance.UnlockCoin();
                AudioCorrect.Play();
                colors.highlightedColor = Color.green;
            }
            else
            {
                AudioFail.Play();
                colors.highlightedColor = Color.red;
            }

            button.colors = colors;
            StartCoroutine(WaitAndReturnToCamera());
        }

        private static IEnumerator WaitAndReturnToCamera()
        {
            yield return new WaitForSeconds(.75f);
            GlobalState.Instance.CurrentQuestion = -1;
            SceneManager.LoadScene(Config.SceneName(Config.Scenes.Camera));
        }
    }
}