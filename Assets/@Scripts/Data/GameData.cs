using System;
using UnityEngine;

namespace WoodBlock.Data
{
    /// <summary>
    /// 게임 진행 데이터
    /// </summary>
    [Serializable]
    public class GameData
    {
        public int currentScore;
        public int bestScore;
        public int combo;
        public bool[,] boardState;

        public GameData()
        {
            currentScore = 0;
            bestScore = PlayerPrefs.GetInt(Constants.SAVE_KEY_BEST_SCORE, 0);
            combo = 0;
            boardState = new bool[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
        }

        public void AddScore(int score)
        {
            currentScore += score;
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                SaveBestScore();
            }
        }

        public void ResetScore()
        {
            currentScore = 0;
            combo = 0;
        }

        public void SaveBestScore()
        {
            PlayerPrefs.SetInt(Constants.SAVE_KEY_BEST_SCORE, bestScore);
            PlayerPrefs.Save();
        }

        public void LoadBestScore()
        {
            bestScore = PlayerPrefs.GetInt(Constants.SAVE_KEY_BEST_SCORE, 0);
        }
    }

    /// <summary>
    /// 게임 설정 데이터
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public float bgmVolume = Constants.DEFAULT_BGM_VOLUME;
        public float sfxVolume = Constants.DEFAULT_SFX_VOLUME;
        public bool isSoundEnabled = true;
        public bool isMusicEnabled = true;
    }
}
