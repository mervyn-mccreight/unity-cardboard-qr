namespace Assets.Scripts 
{
	public class StringResources 
	{
        // main menu texts
        public const string MainMenuHeading = "FH Wedel QR Schnitzeljagd";
	    public const string QuestionProgressText = "Fragen: {0}/{1}";
	    public const string CoinProgressText = "Münzen: {0}/{1}";
	    public const string GoButtonText = "Los!";
	    public const string HelpButtonText = "Anleitung";

        // help-scene texts
        public const string HelpText = "In den Gebäuden der Fachhochschule sind verschiedene QR-Codes versteckt.\nEs gibt Fragen- und Münz-QR-Codes.\nFinde die Fragen-QR-Codes und beantworte die Fragen, um die Münz-QR-Codes freizuschalten.\nNachdem eine Frage eine Münze freigeschaltet hat, sammle diese ein.\nFinde und sammle alle Münzen, um einen Preis zu gewinnen.";
		public const string HelpSceneHeading = "Anleitung";
		public const string BackButtonText = "Zurück";

        // camera scene texts
	    public const string WinToastMessage = "Herzlichen Glückwunsch! Du hast alle Münzen gesammelt. Hole dir deinen Preis am Stand ab.";
	    public const string QuestionAlreadyAnsweredToastMessage = "Du hast diese Frage bereits beantwortet.";
	    public const string CoinAlreadyCollectedToastMessage = "Du hast diese Münze bereits eingesammelt.";
        public const string TapCoinToCollectToastMessage = "Berühre den Bildschirm um diese Münze einzusammeln.";
        public const string AnswerQuestionFirstToastMessage = "Diese Münze ist noch gesperrt. Suche und beantworte zunächst die richtige Frage.";
	}
}
