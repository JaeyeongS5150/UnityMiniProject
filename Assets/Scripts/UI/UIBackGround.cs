using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBackGround : MonoBehaviour
{
    [Header("스프라이트 시퀀스")]
    [Tooltip("순서대로 재생할 배경 스프라이트들")]
    [SerializeField] private Sprite[] _frames;

    [Header("재생 속도")]
    [Tooltip("초당 프레임 수 (FPS)")]
    [SerializeField] private float _frameRate = 12f;

    [Tooltip("Time.timeScale의 영향을 받지 않는 언스케일드 시간 사용")]
    [SerializeField] private bool _useUnscaledTime = true;

    private Image _targetImage;
    private int _currentIndex = 0;
    private float _timer = 0f;

    private void Awake()
    {
        _targetImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (_frames == null || _frames.Length == 0 || _targetImage == null)
        {
            return;
        }

        float delta = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _timer += delta;

        float interval = 1f / Mathf.Max(1f, _frameRate);

        if (_timer >= interval)
        {
            _timer -= interval;
            _currentIndex = (_currentIndex + 1) % _frames.Length;
            _targetImage.sprite = _frames[_currentIndex];
        }
    }
}
