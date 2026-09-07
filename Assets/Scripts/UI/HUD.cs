using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("1. 상단 중앙: 시간 / 웨이브 / 배속")]
    [SerializeField] private TextMeshProUGUI _timeText;     
    [SerializeField] private TextMeshProUGUI _waveText;     
    [SerializeField] private TextMeshProUGUI _gameSpeedText;

    [Header("2. 좌측 상단: 레벨 / 경험치")]
    [SerializeField] private TextMeshProUGUI _levelText;    
    [SerializeField] private Slider _expSlider;             

    [Header("3. 하단 중앙: 체력")]
    [SerializeField] private TextMeshProUGUI _hpText;       
    [SerializeField] private Slider _hpSlider;

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsLive)
        {
            return;
        }

        UpdateTopInfo();
        UpdateLevelInfo();
        UpdateHpInfo();
    }

    /// <summary>
    /// 플레이 타임(분:초:밀리초), 웨이브, 배속 갱신
    /// </summary>
    private void UpdateTopInfo()
    {
        if (_timeText != null)
        {
            float t = GameManager.Instance.GameTime;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            int milliseconds = Mathf.FloorToInt((t * 100f) % 100f);

            _timeText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        }

        if (_waveText != null && GameManager.Instance.Wave != null)
        {
            _waveText.text = $"Wave {GameManager.Instance.Wave.CurrentWave}";
        }

        if (_gameSpeedText != null)
        {
            _gameSpeedText.text = $"x{Time.timeScale:F0}";
        }
    }

    /// <summary>
    /// 레벨 텍스트 및 경험치 게이지 갱신
    /// </summary>
    private void UpdateLevelInfo()
    {
        BaseCore core = GameManager.Instance.Core;
        if (core == null) return;

        if (_levelText != null)
        {
            _levelText.text = $"Level {core.Level}";
        }

        if (_expSlider != null)
        {
            _expSlider.maxValue = core.MaxExp;
            _expSlider.value = core.CurrentExp;
        }
    }

    /// <summary>
    /// 기지 체력 텍스트 및 슬라이더 갱신
    /// </summary>
    private void UpdateHpInfo()
    {
        BaseCore core = GameManager.Instance.Core;
        if (core == null) return;

        if (_hpText != null)
        {
            _hpText.text = $"{Mathf.CeilToInt(core.CurrentHp)} / {core.MaxHp}";
        }

        if (_hpSlider != null)
        {
            _hpSlider.maxValue = core.MaxHp;
            _hpSlider.value = core.CurrentHp;
        }
    }
}
