﻿using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Assets.Scripts
{
    [Serializable]
    public class GlobalState
    {
        private static GlobalState _instance;

        public static GlobalState Instance
        {
            get
            {
                if (_instance == null)
                {
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
                }
                return _instance;
            }
        }

        public static void Save()
        {
            var state = Instance;
            var bf = new BinaryFormatter();
            var saveTo = File.Create(Config.StatePath);
            bf.Serialize(saveTo, state);
            saveTo.Close();
            Debug.Log("GlobalState persisted");
        }

        public Question CurrentQuestion
        {
            get { return _currentQuestion; }
            set { _currentQuestion = value; }
        }

        public int CurrentCoin
        {
            get { return _currentCoin; }
            set { _currentCoin = value; }
        }

        public int CamWidth
        {
            get { return _camWidth; }
            set { _camWidth = value; }
        }

        public int CamHeight
        {
            get { return _camHeight; }
            set { _camHeight = value; }
        }

        private readonly List<int> _unlockedCoins = new List<int>();
        private readonly List<int> _collectedCoins = new List<int>();

        public bool IsCoinUnlocked(int id)
        {
            return _unlockedCoins.Contains(id);
        }

        public bool IsCoinCollected(int id)
        {
            return _collectedCoins.Contains(id);
        }

        public void UnlockCoin()
        {
            if (CurrentQuestion != null && !_unlockedCoins.Contains(CurrentQuestion.Id))
            {
                _unlockedCoins.Add(CurrentQuestion.Id);
            }
        }

        public bool CollectCoin()
        {
            if (CurrentCoin >= 0 && !_collectedCoins.Contains(CurrentCoin))
            {
                _collectedCoins.Add(CurrentCoin);
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

        [NonSerialized] private Question _currentQuestion;

        [NonSerialized] private int _camWidth;
        [NonSerialized] private int _camHeight;


        public void Reset()
        {
            CurrentCoin = -1;
            CurrentQuestion = null;
        }
    }
}