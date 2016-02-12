namespace Assets.Scripts
{
    public class Config
    {
        // Scene names
        public const string MainMenuScene = "MainMenuScene";
        public const string CameraScene = "CameraScene";
        public const string QuestionScene = "QuestionScene";

        // API
        // TODO: FH Wedel server URL
        public const string ApiUrlQuestionCount = "http://192.168.1.30/cardboard-qr-questionform/api.php?action=get_question_count";
    }
}
