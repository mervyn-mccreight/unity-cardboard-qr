using System;

namespace Assets.Scripts
{
    [Serializable]
    public class Data {

        public int Id { get; set; }
        public DataType Type { get; set; }

        public string Text { get; set; }
        public string[] Answers { get; set; }
        public int CorrectAnswer { get; set; }

        public Question ToQuestion()
        {
            return new Question(Id, Text, Answers, CorrectAnswer);
        }
    }
}
