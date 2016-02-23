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
            Help = 3,
            None = -1
        }

        private static readonly string[] _sceneNames =
        {
            "MainMenuScene",
            "CameraScene",
            "QuestionScene",
            "HelpScene"
        };

        public static string SceneName(Scenes scene)
        {
            return _sceneNames[(int) scene];
        }

        // API
        // fh wedel merv
        public const string ApiUrlQuestionCount = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questioncount";
        public const string ApiUrlQuestions = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questions/";

        // storage
        public static readonly string StatePath = Application.persistentDataPath + "/globalState.dat";
    }
}