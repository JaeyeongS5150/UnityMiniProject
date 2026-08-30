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
    //[SerializeField] private float _gameSpeed;

    [Header("오브젝트들 연결")]
    [SerializeField] private BaseCore _core;
    [SerializeField] private PoolManager _pool;
    [SerializeField] private WaveManager _wave;
    [SerializeField] private Spawner _spawner;

    #region 프로퍼티
    public bool IsLive => _isLive;
    public float GameTime => _gameTime;

    public BaseCore Core => _core;
    public PoolManager Pool => _pool;
    public WaveManager Wave => _wave;
    public Spawner Spawner => _spawner;

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
        GameStart();
    }


    void Update()
    {
        _gameTime += Time.deltaTime;

        _currentWave = _wave.CurrentWave;
    }

    private void GameStart()
    {
        _gameTime = 0f;
        _isLive = true;
        Time.timeScale = 1f;

        if (_core == null)
        {
            GameObject coreObj = GameObject.FindGameObjectWithTag("Core");
            if (coreObj != null)
            {
                _core = coreObj.GetComponent<BaseCore>();
            }
        }
    }

    private void GamePause()
    {
        _isLive = false;
        Time.timeScale = 0f;
    }

    private void GameResume()
    {
        _isLive = true;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {

    }
}
