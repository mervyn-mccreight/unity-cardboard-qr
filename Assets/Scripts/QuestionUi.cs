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

        protected virtual void Start()
        {
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
                SceneManager.LoadScene(Config.CameraScene);
            }
        }

        public void OnAnswerClick(int answerIndex)
        {
            if (_question != null && _question.CorrectAnswer == answerIndex)
            {
                GlobalState.Instance.UnlockCoin();
            }

            StartCoroutine(WaitAndReturnToCamera());
        }

        private static IEnumerator WaitAndReturnToCamera()
        {
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(Config.CameraScene);
        }
    }
}