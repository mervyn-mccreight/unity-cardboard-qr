using System.Collections;

namespace Assets.Scripts 
{
	public class StringResources 
	{
        // main menu texts
        public const string MainMenuHeading = "FH Wedel QR Schnitzeljagd";
	    public const string QuestionProgressText = "Fragen: {0}/{1}";
	    public const string CoinProgressText = "Münzen: {0}/{1}";
	    public const string GoButtonText = "Los!";
	    public const string HelpButtonText = "Hilfe";

        // help-scene texts
        public const string HelpText = "In den Gebäuden der Fachhochschule sind verschiedene QR-Codes versteckt.\nEs gibt Fragen- und Münz-QR-Codes.\nFinde die Fragen-QR-Codes und beantworte die Fragen, um die Münz-QR-Codes freizuschalten.\nNachdem eine Frage eine Münze freigeschaltet hat, sammle diese ein.\nFinde und sammle alle Münzen, um einen Preis zu gewinnen.";
		public const string HelpSceneHeading = "Anleitung";
		public const string BackButtonText = "Zurück";

        // camera scene texts
	    public const string WinToastMessage = "Congratulations! Go collect your prize :D";
	    public const string QuestionAlreadyAnsweredToastMessage = "Already unlocked!";
	    public const string CoinAlreadyCollectedToastMessage = "Coin is already collected!";
        public const string TapCoinToCollectToastMessage = "Tap the screen to capture the coin!";
        public const string AnswerQuestionFirstToastMessage = "Answer the question first!";
	}
}
