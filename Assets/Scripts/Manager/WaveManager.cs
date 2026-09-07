using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public enum WaveState
    {
        Spawning,
        BossPhase,
        WaitingReward
    }

    [System.Serializable]
    public struct EnemySpawnData
    {
        public EnemyDataSO enemyData;
        [Range(1, 100)]
        [Tooltip("스폰 가중치 비율")]
        public int spawnWeight;
    }

    [System.Serializable]
    public struct RoundConfig
    {
        public float spawnInterval;                  // 일반 적 스폰 주기 (초)
        public EnemyDataSO bossData;                 // 라운드 보스 SO
        public int bossReward;                   // 보스 처치 시 지급 골드
        public List<EnemySpawnData> availableEnemies;   // 이번 라운드 출현 일반 적 목록
    }

    [Header("웨이브 매니저  정보")]
    [SerializeField] private int _wave = 1;
    [SerializeField] private WaveState _waveState = WaveState.Spawning;
    [SerializeField] private int _currentWaveKills;
    [SerializeField] private int[] _needKills = { 25, 40, 60, 85, 120, 150, 170, 190, 210, 230 };

    [Header("1 ~ 10 웨이브 설정")]
    [SerializeField] private List<RoundConfig> _roundConfig = new List<RoundConfig>();

    [Header("무한모드 데이터")]
    [SerializeField] private EnemyDataSO _infiniteBossData;
    [SerializeField] private List<EnemyDataSO> _infiniteEnemyPool;

    private float _spawnTimer = 0f;

    public WaveState CurrentState => _waveState;
    public int CurrentWave => _wave;
    public int CurrentWaveKills => _currentWaveKills;

    private void Start()
    {
        StartWave(_wave);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsLive)
        {
            return;
        }

        if (_waveState == WaveState.Spawning)
        {
            HandleSpawning();
        }
    }

    /// <summary>
    /// waveNum번호의 웨이브 시작
    /// </summary>
    /// <param name="waveNum"></param>
    public void StartWave(int waveNum)
    {
        _wave = waveNum;
        _currentWaveKills = 0;
        _spawnTimer = 0f;
        _waveState = WaveState.Spawning;

        // 추후에 UI랑 연결
    }

    private void HandleSpawning()
    {
        RoundConfig config = GetCurrentRoundConfig();
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= config.spawnInterval)
        {
            _spawnTimer = 0f;
            EnemyDataSO selectedEnemy = GetRandomEnemyFromConfig(config);

            if (selectedEnemy != null)
            {
                if (selectedEnemy.Type == EnemyDataSO.EnemyType.Cluster)
                {
                    GameManager.Instance.Spawner.SpawnClusters(selectedEnemy);
                }
                else
                {
                    GameManager.Instance.Spawner.SpawnEnemy(selectedEnemy);
                }
            }
        }
    }

    /// <summary>
    /// 무한 모드 비율(■40%, ◈20%, ※20%, ×10%, S10%) 또는 일반 리스트 가중치 반환
    /// </summary>
    private EnemyDataSO GetRandomEnemyFromConfig(RoundConfig config)
    {
        if (config.availableEnemies == null || config.availableEnemies.Count == 0)
        {
            return null;
        }

        // 무한 모드의 지정된 확률 비율 적용
        if (_wave >= 11 && config.availableEnemies.Count >= 5)
        {
            int rand = Random.Range(0, 100);
            if (rand < 40)
            {
                return _infiniteEnemyPool[0];       // ■ 브릭 (40%)
            }
            else if (rand < 60)
            {
                return _infiniteEnemyPool[1];      // ◈ 디바이드 (20%)
            }
            else if (rand < 80)
            {
                return _infiniteEnemyPool[2];      // ※ 클러스터 (20%)
            }
            else if (rand < 90)
            {
                return _infiniteEnemyPool[3];      // × 러쉬 (10%)
            }
            else
            {
                return _infiniteEnemyPool[4];      // S 고스트 (10%)
            }
        }

        if (config.availableEnemies == null || config.availableEnemies.Count == 0) return null;

        int totalWeight = 0;
        for (int i = 0; i < config.availableEnemies.Count; i++)
        {
            totalWeight += config.availableEnemies[i].spawnWeight;
        }

        if (totalWeight <= 0) return config.availableEnemies[0].enemyData;

        int randomPoint = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        for (int i = 0; i < config.availableEnemies.Count; i++)
        {
            currentWeightSum += config.availableEnemies[i].spawnWeight;
            if (randomPoint < currentWeightSum)
            {
                return config.availableEnemies[i].enemyData;
            }
        }

        return config.availableEnemies[0].enemyData;
    }

    /// <summary>
    /// Enemy.cs에서 적이 사망했을때 호출되는 메서드
    /// </summary>
    /// <param name="deadEnemy"></param>
    public void OnEnemyKilled(Enemy deadEnemy)
    {
        if (_waveState == WaveState.Spawning)
        {
            _currentWaveKills++;
            int targetKills = GetTargetKillsForCurrentWave();

            if (_currentWaveKills >= targetKills)
            {
                StartBossPhase();
            }
        }
        else if (_waveState == WaveState.BossPhase)
        {
            RoundConfig config = GetCurrentRoundConfig();

            if (deadEnemy.Data == config.bossData)
            {
                HandleBossVictory(config);
            }
        }
    }

    private void StartBossPhase()
    {
        _waveState = WaveState.BossPhase;
        RoundConfig config = GetCurrentRoundConfig();

        if (config.bossData != null)
        {
            GameManager.Instance.Spawner.SpawnBoss(config.bossData);
        }
    }

    private void HandleBossVictory(RoundConfig config)
    {
        _waveState = WaveState.WaitingReward;

        int unlockedSlotIndex = (_wave <= 4) ? (_wave - 1) : -1;

        if (unlockedSlotIndex != -1 && GameManager.Instance != null && GameManager.Instance.LevelUp != null)
        {
            GameManager.Instance.LevelUp.OpenWaveClearReward(unlockedSlotIndex);
        }
        else
        {
            // 4개 슬롯이 모두 채워진 이후 라운드는 보상 선택 없이 다음 웨이브 진행
            ProceedToNextWave();
        }
    }

    /// <summary>
    /// 포탑 보상 팝업에서 선택 완료 시 호출하여 다음 웨이브 시작
    /// </summary>
    public void ProceedToNextWave()
    {
        StartWave(_wave + 1);
    }

    /// <summary>
    /// 웨이브 목표 킬 수 (11W 이상: 30 + N * 20)
    /// </summary>
    public int GetTargetKillsForCurrentWave()
    {
        int index = _wave - 1;
        if (index >= 0 && index < _needKills.Length)
        {
            return _needKills[index];
        }

        // 11R 무한 모드 수식
        return 30 + (_wave * 20);
    }

    /// <summary>
    /// 현재 웨이브인 라운드 데이터 설정
    /// 11라운드 부터는 일정한 밸런스로 증가
    /// </summary>
    /// <returns></returns>
    private RoundConfig GetCurrentRoundConfig()
    {
        int index = _wave - 1;
        if (index >= 0 && index < _roundConfig.Count)
        {
            return _roundConfig[index];
        }

        // 무한모드 설정
        // 골드 보상이 5라운드부터 50씩 일정하게 증가함
        int nFromFive = _wave - 5;

        return new RoundConfig
        {
            spawnInterval = 0.25f,
            bossData = _infiniteBossData,
            bossReward = 170 + (nFromFive * 50),
            availableEnemies = null
        };
    }
}

