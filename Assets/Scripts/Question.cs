namespace Assets.Scripts
{
    /// <summary>
    /// Class representing a question.
    /// </summary>
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string[] Answers { get; set; }
        public int CorrectAnswer { get; set; }

        public Question(int id, string text, string[] answers, int correctAnswer)
        {
            Id = id;
            Text = text;
            Answers = answers;
            CorrectAnswer = correctAnswer;
        }
    }
}