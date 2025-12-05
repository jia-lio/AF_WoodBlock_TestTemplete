using UnityEngine;
using System.Collections.Generic;

namespace WoodBlock.Managers
{
    /// <summary>
    /// 사운드 관리 (BGM, SFX)
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("오디오 소스")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;

        [Header("BGM")]
        public AudioClip bgmClip;

        [Header("효과음")]
        public AudioClip buttonClickClip;
        public AudioClip blockPlaceClip;
        public AudioClip lineClearClip;
        public AudioClip comboClip;
        public AudioClip bestRecordClip;
        public AudioClip gameOverClip;

        [Header("콤보 효과음 (5단계)")]
        public AudioClip[] comboHitClips = new AudioClip[5];

        [Header("라인 제거 효과음 (4단계)")]
        public AudioClip[] lineClearClips = new AudioClip[4];

        [Header("블록 배치 효과음 (2단계)")]
        public AudioClip[] blockPlaceClips = new AudioClip[2];

        [Header("볼륨 설정")]
        [Range(0f, 1f)] public float bgmVolume = Constants.DEFAULT_BGM_VOLUME;
        [Range(0f, 1f)] public float sfxVolume = Constants.DEFAULT_SFX_VOLUME;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                LoadSounds();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                GameObject bgmObj = new GameObject("BGM_Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX_Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            bgmSource.volume = bgmVolume;
            sfxSource.volume = sfxVolume;
        }

        private void LoadSounds()
        {
            // BGM
            if (bgmClip == null)
                bgmClip = Resources.Load<AudioClip>("Sound/BGM");

            // 기본 효과음
            if (buttonClickClip == null)
                buttonClickClip = Resources.Load<AudioClip>("Sound/ButtonClick");

            if (bestRecordClip == null)
                bestRecordClip = Resources.Load<AudioClip>("Sound/BestRecord");

            // 콤보 히트 사운드 (5개)
            for (int i = 0; i < comboHitClips.Length; i++)
            {
                if (comboHitClips[i] == null)
                    comboHitClips[i] = Resources.Load<AudioClip>($"Sound/combo hit/combo_hit_{i + 1}");
            }

            // 라인 제거 사운드 (4개)
            for (int i = 0; i < lineClearClips.Length; i++)
            {
                if (lineClearClips[i] == null)
                    lineClearClips[i] = Resources.Load<AudioClip>($"Sound/remove line/remove_line_{i + 1}");
            }

            // 블록 배치 사운드 (2개)
            for (int i = 0; i < blockPlaceClips.Length; i++)
            {
                if (blockPlaceClips[i] == null)
                    blockPlaceClips[i] = Resources.Load<AudioClip>($"Sound/place block/place_block_{i + 1}");
            }
        }

        public void PlayBGM()
        {
            if (bgmClip != null && !bgmSource.isPlaying)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlayButtonClick()
        {
            PlaySFX(buttonClickClip);
        }

        public void PlayBlockPlace(int variation = 0)
        {
            if (blockPlaceClips.Length > 0)
            {
                int index = Mathf.Clamp(variation, 0, blockPlaceClips.Length - 1);
                PlaySFX(blockPlaceClips[index]);
            }
        }

        public void PlayLineClear(int lineCount)
        {
            if (lineClearClips.Length > 0)
            {
                int index = Mathf.Clamp(lineCount - 1, 0, lineClearClips.Length - 1);
                PlaySFX(lineClearClips[index]);
            }
        }

        public void PlayComboHit(int comboLevel)
        {
            if (comboHitClips.Length > 0)
            {
                int index = Mathf.Clamp(comboLevel - 1, 0, comboHitClips.Length - 1);
                PlaySFX(comboHitClips[index]);
            }
        }

        public void PlayBestRecord()
        {
            PlaySFX(bestRecordClip);
        }

        public void PlayGameOver()
        {
            PlaySFX(gameOverClip);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            bgmSource.volume = bgmVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        public void MuteAll(bool mute)
        {
            bgmSource.mute = mute;
            sfxSource.mute = mute;
        }
    }
}
