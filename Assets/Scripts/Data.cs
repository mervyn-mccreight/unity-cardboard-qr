using System;

namespace Assets.Scripts
{
    [Serializable]
    public class Data
    {

        public int id;
        public DataType type;

        public string question;
        public string[] answers;
        public int correctAnswer;

        public Question ToQuestion()
        {
            return new Question(id, question, answers, correctAnswer);
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Type: {1}, Question: {2}, Answers: {3}, CorrectAnswer: {4}", id, type, question, answers, correctAnswer);
        }
    }
}
