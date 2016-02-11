namespace Assets.Scripts
{
    public class Question {

        public int Id { get; set; }
        public string Text { get; set; }
        public string[] Answers { get; set; }
        public int CorrectAnswer { get; set; }

        public Question(int id, string text, string[] answers, int correctAnswer)
        {
            this.Id = id;
            this.Text = text;
            this.Answers = answers;
            this.CorrectAnswer = correctAnswer;
        }

    }
}
