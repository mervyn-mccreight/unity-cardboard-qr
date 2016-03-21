using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Class containing application configuration parameters.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Scene names. Must match the order of <see cref="Config.SceneNames"/> array.
        /// </summary>
        public enum Scenes
        {
            MainMenu = 0,
            Camera = 1,
            Question = 2,
            Help = 3,
            None = -1
        }

        /// <summary>
        /// Names of Unity scenes. Must match scene file names.
        /// </summary>
        private static readonly string[] SceneNames =
        {
            "MainMenuScene",
            "CameraScene",
            "QuestionScene",
            "HelpScene"
        };

        /// <summary>
        /// Returns the name of a given scene.
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <returns>Name of corresponding Unity scene file</returns>
        public static string SceneName(Scenes scene)
        {
            return SceneNames[(int) scene];
        }

        // API
        // Calls to Mervyn McCreight FH Wedel hosted backend.
        public const string ApiUrlQuestionCount = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questioncount";
        public const string ApiUrlQuestions = "http://stud.fh-wedel.de/~inf101368/qrcode/api.php/questions/";

        /// <summary>
        /// Storage path for global state.
        /// </summary>
        public static readonly string StatePath = Application.persistentDataPath + "/globalState.dat";
    }
}