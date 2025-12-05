using UnityEngine;
using System;
using WoodBlock.Data;

namespace WoodBlock.Managers
{
    /// <summary>
    /// 점수 및 콤보 관리
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("점수 데이터")]
        private GameData gameData;

        [Header("콤보")]
        private int currentCombo = 0;
        private float comboTimer = 0f;
        private const float COMBO_TIMEOUT = 2f;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnBestScoreChanged;
        public event Action<int> OnComboChanged;

        public int CurrentScore => gameData?.currentScore ?? 0;
        public int BestScore => gameData?.bestScore ?? 0;
        public int CurrentCombo => currentCombo;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameData = new GameData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // 콤보 타이머
            if (currentCombo > 0 && comboTimer > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0)
                {
                    ResetCombo();
                }
            }
        }

        public void AddScoreForBlockPlacement(int cellCount)
        {
            int score = cellCount * Constants.SCORE_PER_CELL;
            AddScore(score);
        }

        public void AddScoreForLinesClear(int lineCount)
        {
            if (lineCount <= 0) return;

            // 라인 제거 점수
            int baseScore = lineCount * Constants.SCORE_PER_LINE;

            // 콤보 증가
            currentCombo++;
            comboTimer = COMBO_TIMEOUT;

            // 콤보 보너스
            float comboMultiplier = 1f + (currentCombo - 1) * (Constants.COMBO_MULTIPLIER - 1f);
            int finalScore = Mathf.RoundToInt(baseScore * comboMultiplier);

            AddScore(finalScore);

            OnComboChanged?.Invoke(currentCombo);
        }

        private void AddScore(int score)
        {
            gameData.AddScore(score);
            OnScoreChanged?.Invoke(gameData.currentScore);

            if (gameData.currentScore > gameData.bestScore)
            {
                OnBestScoreChanged?.Invoke(gameData.bestScore);
            }
        }

        private void ResetCombo()
        {
            currentCombo = 0;
            OnComboChanged?.Invoke(0);
        }

        public void ResetGame()
        {
            gameData.ResetScore();
            ResetCombo();

            OnScoreChanged?.Invoke(0);
            OnComboChanged?.Invoke(0);
        }

        public void SaveBestScore()
        {
            gameData.SaveBestScore();
        }

        public void LoadBestScore()
        {
            gameData.LoadBestScore();
            OnBestScoreChanged?.Invoke(gameData.bestScore);
        }
    }
}
