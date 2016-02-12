
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Assets.Scripts
{
	[Serializable]
	public class GlobalState {
		private static GlobalState _instance;

		public static GlobalState Instance {
			get 
			{
				if (_instance == null) {
					if (File.Exists(Application.persistentDataPath + "/globalState.dat")) {
						var bf = new BinaryFormatter ();
						var loadFrom = File.Open(Application.persistentDataPath + "/globalState.dat", FileMode.Open);
						_instance = (GlobalState) bf.Deserialize(loadFrom);
						loadFrom.Close ();
						Debug.Log ("Loaded persisted GlobalState");
					} else {
						_instance = new GlobalState ();
					}
				}
				return _instance;
			}
		}

		public static void Save() {
			var bf = new BinaryFormatter ();
			var saveTo = File.Create (Application.persistentDataPath + "/globalState.dat");
			bf.Serialize (saveTo , GlobalState.Instance);
			saveTo.Close ();
		}

        public Question CurrentQuestion { 
			get { return _currentQuestion; } 
			set { _currentQuestion = value; } 
		}

        public int CurrentCoin
        {
            get { return _currentCoin; }
            set { _currentCoin = value; }
        }

        public List<int> UnlockedCoins
        {
            get { return _unlockedCoins; }
            set { _unlockedCoins = value; }
        }

        public List<int> CollectedCoins
        {
            get { return _collectedCoins; }
            set { _collectedCoins = value; }
        }
			
        private List<int> _unlockedCoins = new List<int>();
        private List<int> _collectedCoins = new List<int>();

		[NonSerialized]
        private int _currentCoin = -1;

		[NonSerialized]
		private Question _currentQuestion = null;

        public void Reset()
        {
            CurrentCoin = -1;
            CurrentQuestion = null;
        }

        // TODO: can the app know how many coins there are?
        // What happens when the user collected them all?
    }
}
