using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("게임 컨트롤")]
    [SerializeField] private bool _isLive;
    [SerializeField] private float _gameTime;
    [SerializeField] private int _currentWave;

    [Header("게임 배속 설정 (1x ~ 4x)")]
    [SerializeField] private float[] _gameSpeeds = {1f, 2f, 3f};
    [SerializeField] private int _currentSpeedIndex = 0;

    [Header("오브젝트들 연결")]
    [SerializeField] private BaseCore _core;
    [SerializeField] private PoolManager _pool;
    [SerializeField] private WaveManager _wave;
    [SerializeField] private Spawner _spawner;
    [SerializeField] private LevelUp _levelUp;

    [Header("게임오버 UI 연결")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameOverUI _gameOverUI;

    private bool _isGameOver = false;

    #region 프로퍼티
    public bool IsLive => _isLive;
    public float GameTime => _gameTime;

    public BaseCore Core => _core;
    public PoolManager Pool => _pool;
    public WaveManager Wave => _wave;
    public Spawner Spawner => _spawner;
    public LevelUp LevelUp => _levelUp;

    #endregion


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



    }

    void Start()
    {
        _gameOverPanel.SetActive(false);

        GameStart();
    }


    void Update()
    {
        _gameTime += Time.deltaTime;

        _currentWave = _wave.CurrentWave;

        HandleSpeedInput();
    }

    /// <summary>
    /// 배속 단축키 (F1~F4 직접 설정)
    /// </summary>
    private void HandleSpeedInput()
    {
        if (Input.GetKeyDown(KeyCode.F1)) SetGameSpeed(0);
        if (Input.GetKeyDown(KeyCode.F2)) SetGameSpeed(1);
        if (Input.GetKeyDown(KeyCode.F3)) SetGameSpeed(2);
    }

    /// <summary>
    /// 특정 인덱스의 속도로 변경
    /// </summary>
    public void SetGameSpeed(int speedIndex)
    {
        if (speedIndex < 0 || speedIndex >= _gameSpeeds.Length) return;

        _currentSpeedIndex = speedIndex;
        ApplyTimeScale();
    }

    private void ApplyTimeScale()
    {
        if (!_isLive) return;

        float targetSpeed = _gameSpeeds[_currentSpeedIndex];
        Time.timeScale = targetSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Debug.Log($"<color=lime>[GameManager] 게임 배속 변경: {targetSpeed:F1}x</color>");
    }

    private void GameStart()
    {
        _gameTime = 0f;
        _isLive = true;
        _isGameOver = false;
        Time.timeScale = 1f;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInGameBgm();
        }

        if (_core == null)
        {
            GameObject coreObj = GameObject.FindGameObjectWithTag("Core");
            if (coreObj != null)
            {
                _core = coreObj.GetComponent<BaseCore>();
            }
        }
    }

    public void GamePause()
    {
        _isLive = false;
        Time.timeScale = 0f;
    }

    public void GameResume()
    {
        _isLive = true;
        Time.timeScale = _gameSpeeds[_currentSpeedIndex];
    }

    public void GameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        _isLive = false;
        Time.timeScale = 0f;

        int totalKills = _core != null ? _core.TotalKills : 0;
        int currentWave = _wave != null ? _wave.CurrentWave : 1;

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBgm();
            AudioManager.instance.PlaySfx(AudioManager.SFX.GameOver);
        }

        if (_gameOverUI != null)
        {
            _gameOverUI.DisplayResult(_gameTime, currentWave, totalKills);
        }
    }
}
