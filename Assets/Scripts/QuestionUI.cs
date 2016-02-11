using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class QuestionUI : MonoBehaviour
    {
        private Question question;

        public void Start()
        {
            // TODO: debug question
            string[] answers = { "Ich", "Du", "Deine Mama", "Keiner" };
            GlobalState.CurrentQuestion = new Question(0, "Hier steht auch manchmal eine sehr lange Frage, aber auch das sollte gehen. Das war jetzt eher ein Satz, deshalb: Wer ist der King?", answers, 2);

            this.question = GlobalState.CurrentQuestion;

            // init UI
            GameObject.Find("QuestionText").GetComponent<Text>().text = this.question.Text;
            GameObject.Find("Answer1Button").GetComponentInChildren<Text>().text = this.question.Answers[0];
            GameObject.Find("Answer2Button").GetComponentInChildren<Text>().text = this.question.Answers[1];
            GameObject.Find("Answer3Button").GetComponentInChildren<Text>().text = this.question.Answers[2];
            GameObject.Find("Answer4Button").GetComponentInChildren<Text>().text = this.question.Answers[3];
        }

        public void OnAnswerClick(int answerIndex)
        {
            if (this.question != null && this.question.CorrectAnswer == answerIndex)
            {
                GlobalState.Score += 1;
                Debug.Log("Correct! Score is " + GlobalState.Score);
            }
            else
            {
                Debug.Log("Wrong! Score is " + GlobalState.Score);
            }

            StartCoroutine(WaitAndReturnToCamera());
        }

        IEnumerator WaitAndReturnToCamera()
        {
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(0);
        }
    }
}
