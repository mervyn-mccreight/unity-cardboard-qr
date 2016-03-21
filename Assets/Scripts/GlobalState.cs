using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Assets.Scripts
{
    /// <summary>
    /// Class used to persist and manage global game state between different scenes.
    /// </summary>
    [Serializable]
    public class GlobalState
    {
        private static GlobalState _instance;

        /// <summary>
        /// Returns a reference to the global state's instance. If no state has been initialized, tries to load it from storage.
        /// </summary>
        public static GlobalState Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                if (File.Exists(Config.StatePath) && new FileInfo(Config.StatePath).Length > 0)
                {
                    var bf = new BinaryFormatter();
                    var loadFrom = File.Open(Config.StatePath, FileMode.Open);
                    _instance = (GlobalState) bf.Deserialize(loadFrom);
                    loadFrom.Close();
                    Debug.Log("Loaded persisted GlobalState");
                }
                else
                {
                    _instance = new GlobalState();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Saves state to persistent storage.
        /// Storage path is set in <see cref="Config" />.
        /// </summary>
        public static void Save()
        {
            var state = Instance;
            var bf = new BinaryFormatter();
            var saveTo = File.Create(Config.StatePath);
            bf.Serialize(saveTo, state);
            saveTo.Close();
            Debug.Log("GlobalState persisted");
        }

        /// <summary>
        /// Currently displayed question.
        /// </summary>
        public int CurrentQuestion
        {
            get { return _currentQuestion; }
            set { _currentQuestion = value; }
        }

        /// <summary>
        /// Currently displayed coin.
        /// </summary>
        public int CurrentCoin
        {
            get { return _currentCoin; }
            set { _currentCoin = value; }
        }

        /// <summary>
        /// Current camera texture width.
        /// </summary>
        public int CamWidth
        {
            get { return _camWidth; }
            set { _camWidth = value; }
        }

        /// <summary>
        /// Current camera texture height.
        /// </summary>
        public int CamHeight
        {
            get { return _camHeight; }
            set { _camHeight = value; }
        }

        /// <summary>
        /// All questions received from API.
        /// </summary>
        public Questions AllQuestions
        {
            get { return _allQuestions; }
            set { _allQuestions = value; }
        }

        /// <summary>
        /// Reference to <see cref="WebCamTexture"/> used in <see cref="CameraScript"/>. Required to speed up scene switching.
        /// </summary>
        public WebCamTexture WebCamTexture
        {
            get { return _webCamTexture; }
            set { _webCamTexture = value; }
        }

        /// <summary>
        /// Scene to switch to on next frame update. Required to trigger scene changes from threads other than the Unity main thread.
        /// </summary>
        public Config.Scenes SceneToSwitchTo
        {
            get { return _sceneToSwitchTo; }
            set { _sceneToSwitchTo = value; }
        }

        /// <summary>
        /// Height of plane displaying the camera image in <see cref="CameraScript"/>. Required to calculate the correct position of objects in front of the QR-code.
        /// </summary>
        public float PlaneHeight
        {
            get { return _planeHeight; }
            set { _planeHeight = value; }
        }

        /// <summary>
        /// Width of plane displaying the camera image in <see cref="CameraScript"/>. Required to calculate the correct position of objects in front of the QR-code.
        /// </summary>
        public float PlaneWidth
        {
            get { return _planeWidth; }
            set { _planeWidth = value; }
        }

        private readonly List<int> _unlockedCoins = new List<int>();
        private readonly List<int> _collectedCoins = new List<int>();

        /// <summary>
        /// Checks whether the given coin is unlocked (= the corresponding question has been answered correctly).
        /// </summary>
        /// <param name="id">Id of coin to check</param>
        /// <returns>true if coin is unlocked, false otherwise</returns>
        public bool IsCoinUnlocked(int id)
        {
            return _unlockedCoins.Contains(id);
        }

        /// <summary>
        /// Checks whether the given coin is collected (= the user has tapped the coin while it was on screen).
        /// </summary>
        /// <param name="id">Id of coin to check</param>
        /// <returns>True if coin is collected, false otherwise.</returns>
        public bool IsCoinCollected(int id)
        {
            return _collectedCoins.Contains(id);
        }

        /// <summary>
        /// Unlocks the currently displayed coin. Persists the state.
        /// </summary>
        public void UnlockCoin()
        {
            // Only the currently displayed coin can be unlocked, and only if we haven't done so already.
            if (CurrentQuestion >= 0 && !_unlockedCoins.Contains(CurrentQuestion))
            {
                _unlockedCoins.Add(CurrentQuestion);
                Instance.CurrentQuestion = -1;
                Save();
            }
        }

        /// <summary>
        /// Collects the currently displayed coin. Persists the state.
        /// </summary>
        /// <returns>True if the coin has been unlocked, false if there is no current coin, or it is already unlocked.</returns>
        public bool CollectCoin()
        {
            if (CurrentCoin >= 0 && !_collectedCoins.Contains(CurrentCoin))
            {
                _collectedCoins.Add(CurrentCoin);
                Save();
                return true;
            }
            return false;
        }

        public int CollectedCoinCount()
        {
            return _collectedCoins.Count;
        }

        public int UnlockedCoinCount()
        {
            return _unlockedCoins.Count;
        }

        [NonSerialized] private int _currentCoin = -1;
        [NonSerialized] private int _currentQuestion = -1;
        [NonSerialized] private int _camWidth;
        [NonSerialized] private int _camHeight;
        [NonSerialized] private float _planeWidth;
        [NonSerialized] private float _planeHeight;
        [NonSerialized] private Questions _allQuestions;
        [NonSerialized] private WebCamTexture _webCamTexture;
        [NonSerialized] private Config.Scenes _sceneToSwitchTo = Config.Scenes.None;

        /// <summary>
        /// Reset the current question and current coin. All other values remain, since they should be static throughout the life time of the app.
        /// </summary>
        public void Reset()
        {
            CurrentCoin = -1;
            CurrentQuestion = -1;
        }
    }
}