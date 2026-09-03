using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("스폰 위치")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("클러스터 소환 시 배치 간격")]
    [SerializeField] private float _clusterGridSpacing = 0.5f;

    [Header("분열 시 기본 소환용 스카우트 데이터")]
    [SerializeField] private EnemyDataSO _defaultScouterData;

    private void Awake()
    {
        _spawnPoints = GetComponentsInChildren<Transform>();
    }

    /// <summary>
    /// 기본적인 적 소환 메서드
    /// </summary>
    /// <param name="enemyData"></param>
    public void SpawnEnemy(EnemyDataSO enemyData)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            return;
        }

        Transform point = _spawnPoints[Random.Range(1, _spawnPoints.Length)];   // Spawner 스크립트가 붙은 자기자신을 제위하고 자식으로 있는 spawnPoint들에만 소환하게 하기 위해 인덱스를 1부터 시작
        GameObject enemyObj = GameManager.Instance.Pool.GetObjFromPool(enemyData.EnemyPoolIndex);

        if (enemyObj != null)
        {
            enemyObj.transform.position = point.position;
            enemyObj.transform.localScale = Vector3.one;

            if (enemyObj.TryGetComponent<Enemy>(out var enemy))
            {
                Transform coreTarget = GameManager.Instance.Core != null ? GameManager.Instance.Core.transform : null;
                enemy.InitEnemy(enemyData, coreTarget);
            }
        }
        
    }

    /// <summary>
    /// 디바이드 몬스터가 죽었을 때 그 자리에 두체의 스카우터 소환용 메서드
    /// </summary>
    /// <param name="scouterData"></param>
    public void SpawnDividedScouters(Vector3 deathPos)
    {
        Transform coreTarget = GameManager.Instance.Core != null ? GameManager.Instance.Core.transform : null;

        for (int i = 0; i < 2; i++)
        {
            GameObject scouterObj = GameManager.Instance.Pool.GetObjFromPool(_defaultScouterData.EnemyPoolIndex);

            if (scouterObj != null)
            {
                scouterObj.transform.position = new Vector3(deathPos.x + -3f + i * 6f, 5f, deathPos.z);
                scouterObj.transform.localScale = Vector3.one;

                if (scouterObj.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.InitEnemy(_defaultScouterData, coreTarget);
                }
            }
        }
    }

    /// <summary>
    /// 9쌍의 클러스터가 소환되기 위한 메서드
    /// </summary>
    /// <param name="poolIndex"></param>
    public void SpawnClusters(EnemyDataSO clusterData)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            return;
        }

        Transform point = _spawnPoints[Random.Range(1, _spawnPoints.Length)];
        Transform corePos = GameManager.Instance.Core.transform;

        Vector3 dirToCore = (corePos.position - point.position).normalized;
        dirToCore.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(dirToCore);

        for (int i = 0; i < 9; i++)
        {
            int row = (i / 3) - 1; // -1, 0, 1 (전후)
            int col = (i % 3) - 1; // -1, 0, 1 (좌우)

            Vector3 localOffset = new Vector3(col * _clusterGridSpacing, 0f, row * _clusterGridSpacing);
            Vector3 worldSpawnPos = point.position + (lookRotation * localOffset);

            GameObject clusterObj = GameManager.Instance.Pool.GetObjFromPool(clusterData.EnemyPoolIndex);

            if (clusterObj != null)
            {
                clusterObj.transform.position = worldSpawnPos;
                clusterObj.transform.localScale = Vector3.one;

                if (clusterObj.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.InitEnemy(clusterData, corePos);
                }
            }
        }
    }

    /// <summary>
    /// 웨이브 마다 소환될 보스용 메서드
    /// 기존 적들의 크기를 키운 보스
    /// 추가 체력및 골드를 줌
    /// </summary>
    /// <param name="poolIndex"></param>
    public void SpawnBoss(EnemyDataSO bossData)
    {
        if (_spawnPoints == null || _spawnPoints.Length <= 1 || bossData == null)
        {
            return;
        }
        Transform point = _spawnPoints[Random.Range(1, _spawnPoints.Length)];
        GameObject bossObj = GameManager.Instance.Pool.GetObjFromPool(bossData.EnemyPoolIndex, -1f);

        if (bossObj != null)
        {
            bossObj.transform.position = point.position;
            bossObj.transform.localScale = Vector3.one * 2.2f; // 보스 크기 확대

            if (bossObj.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.InitEnemy(bossData, GameManager.Instance.Core != null ? GameManager.Instance.Core.transform : null);
            }
        }
    }
}
