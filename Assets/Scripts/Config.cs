using UnityEngine;

namespace Assets.Scripts
{
    public class Config
    {
        // Scene names
        public enum Scenes
        {
            MainMenu = 0,
            Camera = 1,
            Question = 2,
            None = -1
        }

        private static readonly string[] _sceneNames =
        {
            "MainMenuScene",
            "CameraScene",
            "QuestionScene"
        };

        public static string SceneName(Scenes scene)
        {
            return _sceneNames[(int) scene];
        }

        // API
        // tim
//        public const string ApiUrlQuestionCount =   "http://192.168.1.30/cardboard-qr-questionform/api.php/questioncount";
//        public const string ApiUrlQuestions =       "http://192.168.1.30/cardboard-qr-questionform/api.php/questions/";

        // mervyn
        //		public const string ApiUrlQuestionCount = "http://192.168.178.32/cardboard-qr-marker-frontend/api.php/questioncount";

        // fh wedel merv
        public const string ApiUrlQuestionCount = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questioncount";
        public const string ApiUrlQuestions = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questions/";

        // storage
        public static readonly string StatePath = Application.persistentDataPath + "/globalState.dat";
    }
}