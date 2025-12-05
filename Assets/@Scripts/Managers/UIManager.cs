using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WoodBlock.Managers
{
    /// <summary>
    /// UI 표시 및 관리
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("점수 UI")]
        public TextMeshProUGUI currentScoreText;
        public TextMeshProUGUI bestScoreText;

        [Header("콤보 UI")]
        public GameObject comboPanel;
        public TextMeshProUGUI comboText;
        public CanvasGroup comboCanvasGroup;

        [Header("게임 오버 UI")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public Button restartButton;
        public Button exitButton;

        [Header("설정 UI")]
        public GameObject settingsPanel;
        public Button settingsButton;
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;
        public Button closeSettingsButton;

        [Header("일시정지 UI")]
        public GameObject pausePanel;
        public Button pauseButton;
        public Button resumeButton;

        private Sequence comboSequence;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeUI()
        {
            // 버튼 리스너 추가
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            // 슬라이더 리스너
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // 초기 상태
            HideGameOverPanel();
            HideSettingsPanel();
            HidePausePanel();
            HideComboPanel();
        }

        public void UpdateCurrentScore(int score)
        {
            if (currentScoreText != null)
            {
                currentScoreText.text = score.ToString();

                // 점수 증가 애니메이션
                currentScoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            }
        }

        public void UpdateBestScore(int score)
        {
            if (bestScoreText != null)
            {
                bestScoreText.text = $"Best: {score}";

                // 최고 기록 갱신 애니메이션
                bestScoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.5f);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBestRecord();
                }
            }
        }

        public void ShowCombo(int combo)
        {
            if (combo <= 1)
            {
                HideComboPanel();
                return;
            }

            if (comboPanel != null)
            {
                comboPanel.SetActive(true);

                if (comboText != null)
                {
                    comboText.text = $"COMBO x{combo}!";
                }

                // 콤보 애니메이션
                comboSequence?.Kill();
                comboSequence = DOTween.Sequence();

                if (comboCanvasGroup != null)
                {
                    comboCanvasGroup.alpha = 0;
                    comboSequence.Append(comboCanvasGroup.DOFade(1f, 0.2f));
                }

                comboSequence.Append(comboPanel.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 8, 0.8f));
                comboSequence.AppendInterval(Constants.COMBO_DISPLAY_DURATION);
                comboSequence.Append(comboCanvasGroup.DOFade(0f, 0.3f));
                comboSequence.AppendCallback(() => comboPanel.SetActive(false));

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayComboHit(combo);
                }
            }
        }

        public void HideComboPanel()
        {
            if (comboPanel != null)
            {
                comboSequence?.Kill();
                comboPanel.SetActive(false);
            }
        }

        public void ShowGameOverPanel(int finalScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (finalScoreText != null)
                {
                    finalScoreText.text = $"Score: {finalScore}";
                }

                // 게임 오버 패널 애니메이션
                gameOverPanel.transform.localScale = Vector3.zero;
                gameOverPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayGameOver();
                }
            }
        }

        public void HideGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        public void ShowSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);

                // 현재 설정 값 로드
                if (SoundManager.Instance != null)
                {
                    if (bgmVolumeSlider != null)
                        bgmVolumeSlider.value = SoundManager.Instance.bgmVolume;

                    if (sfxVolumeSlider != null)
                        sfxVolumeSlider.value = SoundManager.Instance.sfxVolume;
                }

                settingsPanel.transform.localScale = Vector3.zero;
                settingsPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
        }

        public void HideSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => settingsPanel.SetActive(false));
            }
        }

        public void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void HidePausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }

        private void OnRestartClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            HideGameOverPanel();

            var gameManager = FindObjectOfType<Core.GameManager>();
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }

        private void OnExitClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnSettingsClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            ShowSettingsPanel();
        }

        private void OnCloseSettingsClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            HideSettingsPanel();
        }

        private void OnPauseClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            ShowPausePanel();
        }

        private void OnResumeClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            HidePausePanel();
        }

        private void OnBGMVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetBGMVolume(value);
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetSFXVolume(value);
            }
        }
    }
}
