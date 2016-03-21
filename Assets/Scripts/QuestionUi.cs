using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    /// <summary>
    /// Controls the question UI behavior. Is attached to Unity QuestionScene.Canvas.
    /// </summary>
    public class QuestionUi : MonoBehaviour
    {
        // Currently displayed question.
        private Question _question;

        // Correct answer to compare user selected answer to.
        private string _correctAnswer;

        /// <summary>
        /// Correct feedback sound. Referenced in Unity QuestionScene.Canvas.
        /// </summary>
        public AudioSource AudioCorrect;

        /// <summary>
        /// Fail feedback sound. Referenced in Unity QuestionScene.Canvas.
        /// </summary>
        public AudioSource AudioFail;

        protected virtual void Start()
        {
            GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;

            // From all questions, get the one that matches the scanned QR code (CurrentQuestion).
            _question =
                GlobalState.Instance.AllQuestions.questions.First(x => x.id == GlobalState.Instance.CurrentQuestion)
                    .ToQuestion();

            // Save value of correct answer so we can compare to it later.
            _correctAnswer = _question.Answers[_question.CorrectAnswer];

            // Shuffle answers.
            var shuffledAnswers = _question.Answers.OrderBy(x => Random.value * 4).ToArray();

            // Fill UI.
            GameObject.Find("QuestionText").GetComponent<Text>().text = _question.Text;
            GameObject.Find("Answer1Button").GetComponentInChildren<Text>().text = shuffledAnswers[0];
            GameObject.Find("Answer2Button").GetComponentInChildren<Text>().text = shuffledAnswers[1];
            GameObject.Find("Answer3Button").GetComponentInChildren<Text>().text = shuffledAnswers[2];
            GameObject.Find("Answer4Button").GetComponentInChildren<Text>().text = shuffledAnswers[3];
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Reset current question and go back to CameraScene.
                GlobalState.Instance.CurrentQuestion = -1;
                SceneManager.LoadScene(Config.SceneName(Config.Scenes.Camera));
            }
        }

        /// <summary>
        /// Handles user response. Attached to Unity QuestionScene.AnswerXButtons.
        /// </summary>
        /// <param name="answerIndex">Null-based index of clicked button.</param>
        public void OnAnswerClick(int answerIndex)
        {
            var button = GameObject.Find(string.Format("Answer{0}Button", answerIndex + 1)).GetComponent<Button>();
            var colors = button.colors;

            if (button.GetComponentInChildren<Text>().text.Equals(_correctAnswer))
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

            // Return to camera after waiting a little bit, so the user actually sees the button color feedback.
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