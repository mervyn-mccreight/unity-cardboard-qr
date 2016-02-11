﻿
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class GlobalState {

        public static Question CurrentQuestion { get; set; }

        public static int CurrentCoin { get; set; }

        public static HashSet<int> UnlockedCoins
        {
            get { return _unlockedCoins; }
            set { _unlockedCoins = value; }
        }

        public static HashSet<int> CollectedCoins
        {
            get { return _collectedCoins; }
            set { _collectedCoins = value; }
        }

        private static HashSet<int> _unlockedCoins = new HashSet<int>();

        private static HashSet<int> _collectedCoins = new HashSet<int>();

        // TODO: can the app know how many coins there are?
        // What happens when the user collected them all?
    }
}
