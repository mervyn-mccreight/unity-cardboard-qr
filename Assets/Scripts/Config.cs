using UnityEngine;

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
        // tim
        //public const string ApiUrlQuestionCount =
        //    "http://192.168.1.30/cardboard-qr-questionform/api.php/questioncount";

        // mervyn
        //		public const string ApiUrlQuestionCount = "http://192.168.178.32/cardboard-qr-marker-frontend/api.php/questioncount";

		// fh wedel merv
		public const string ApiUrlQuestionCount = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questioncount";
        public const string ApiUrlQuestions = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questions/";

        // storage
        public static readonly string StatePath = Application.persistentDataPath + "/globalState.dat";

        // camera
        public const int CamWidth = 1024;
        public const int CamHeight = 768;
    }
}