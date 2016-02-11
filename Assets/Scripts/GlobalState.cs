
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class GlobalState {

        public static Question CurrentQuestion { get; set; }

        public static int Score { get; set; }

        public static List<int> UnlockedCoins
        {
            get { return _unlockedCoins; }
            set { _unlockedCoins = value; }
        }

        private static List<int> _unlockedCoins = new List<int>();

        // TODO: can the app know how many coins there are?
        // What happens when the user collected them all?
    }
}
