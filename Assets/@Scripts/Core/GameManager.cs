using UnityEngine;
using Cysharp.Threading.Tasks;
using WoodBlock.Managers;
using WoodBlock.Gameplay;
using WoodBlock.Data;
using System.Collections.Generic;

namespace WoodBlock.Core
{
    /// <summary>
    /// 게임 전체 흐름 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("매니저들")]
        public BoardManager boardManager;
        public BlockManager blockManager;
        public ScoreManager scoreManager;
        public SoundManager soundManager;
        public UIManager uiManager;

        [Header("게임 상태")]
        private bool isGameOver = false;
        private bool isPlaying = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeManagers();
        }

        private void Start()
        {
            StartGame();
        }

        private void InitializeManagers()
        {
            // 매니저들 자동 찾기 또는 생성
            if (boardManager == null)
                boardManager = FindObjectOfType<BoardManager>();

            if (blockManager == null)
                blockManager = FindObjectOfType<BlockManager>();

            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>();

            if (soundManager == null)
                soundManager = FindObjectOfType<SoundManager>();

            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();

            // ScoreManager 이벤트 구독
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += OnScoreChanged;
                scoreManager.OnBestScoreChanged += OnBestScoreChanged;
                scoreManager.OnComboChanged += OnComboChanged;
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= OnScoreChanged;
                scoreManager.OnBestScoreChanged -= OnBestScoreChanged;
                scoreManager.OnComboChanged -= OnComboChanged;
            }
        }

        public void StartGame()
        {
            isGameOver = false;
            isPlaying = true;

            // 보드 초기화
            if (boardManager != null)
                boardManager.InitializeBoard();

            // 블록 생성
            if (blockManager != null)
                blockManager.InitializeBlockSlots();

            // 점수 초기화
            if (scoreManager != null)
            {
                scoreManager.ResetGame();
                scoreManager.LoadBestScore();
            }

            // BGM 재생
            if (soundManager != null)
                soundManager.PlayBGM();

            // UI 초기화
            if (uiManager != null)
            {
                uiManager.HideGameOverPanel();
                uiManager.UpdateCurrentScore(0);
                uiManager.UpdateBestScore(scoreManager?.BestScore ?? 0);
            }

            Debug.Log("게임 시작!");
        }

        public void RestartGame()
        {
            StartGame();
        }

        public async void OnBlockPlaced(Block block)
        {
            if (isGameOver) return;

            // 블록 배치 점수
            int cellCount = block.blockShape.GetCellCount();
            scoreManager?.AddScoreForBlockPlacement(cellCount);

            // 사운드 재생
            soundManager?.PlayBlockPlace(Random.Range(0, 2));

            // 블록 매니저에 알림
            blockManager?.OnBlockPlaced(block);

            // 라인 체크 및 제거
            await CheckAndClearLines();

            // 게임 오버 체크
            CheckGameOver();
        }

        private async UniTask CheckAndClearLines()
        {
            if (boardManager == null) return;

            int linesCleared = await boardManager.CheckAndClearLines();

            if (linesCleared > 0)
            {
                // 라인 제거 점수
                scoreManager?.AddScoreForLinesClear(linesCleared);

                // 사운드 재생
                soundManager?.PlayLineClear(linesCleared);

                Debug.Log($"{linesCleared}줄 제거됨! 콤보: {scoreManager?.CurrentCombo ?? 0}");
            }
        }

        private void CheckGameOver()
        {
            if (isGameOver || boardManager == null || blockManager == null) return;

            // 현재 활성화된 블록들
            List<BlockShape> activeBlocks = blockManager.GetActiveBlockShapes();

            // 배치 가능한 블록이 하나도 없으면 게임 오버
            if (activeBlocks.Count > 0 && !boardManager.HasAnyValidPlacement(activeBlocks))
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            isPlaying = false;

            Debug.Log("게임 오버!");

            // 최고 점수 저장
            scoreManager?.SaveBestScore();

            // UI 표시
            uiManager?.ShowGameOverPanel(scoreManager?.CurrentScore ?? 0);
        }

        private void OnScoreChanged(int newScore)
        {
            uiManager?.UpdateCurrentScore(newScore);
        }

        private void OnBestScoreChanged(int newBestScore)
        {
            uiManager?.UpdateBestScore(newBestScore);
        }

        private void OnComboChanged(int combo)
        {
            uiManager?.ShowCombo(combo);
        }

        public bool IsGameOver() => isGameOver;
        public bool IsPlaying() => isPlaying;
    }
}
