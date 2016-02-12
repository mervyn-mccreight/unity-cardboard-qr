
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class GlobalState : MonoBehaviour {
		private static GlobalState _instance;

		public static GlobalState Instance {
			get 
			{
				if (_instance == null) {
					_instance = new GameObject ("GlobalState").AddComponent<GlobalState>();
				}
				return _instance;
			}
		}

		public void OnApplicationQuit() {
			_instance = null;
		}

        public Question CurrentQuestion { get; set; }

        public int CurrentCoin
        {
            get { return _currentCoin; }
            set { _currentCoin = value; }
        }

        public HashSet<int> UnlockedCoins
        {
            get { return _unlockedCoins; }
            set { _unlockedCoins = value; }
        }

        public HashSet<int> CollectedCoins
        {
            get { return _collectedCoins; }
            set { _collectedCoins = value; }
        }

        private HashSet<int> _unlockedCoins = new HashSet<int>();
        private HashSet<int> _collectedCoins = new HashSet<int>();
        private int _currentCoin = -1;

        public void Reset()
        {
            CurrentCoin = -1;
            CurrentQuestion = null;
        }

        // TODO: can the app know how many coins there are?
        // What happens when the user collected them all?
    }
}
