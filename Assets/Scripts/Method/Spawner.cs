using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("스폰 위치")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("클러스터 소환 시 배치 간격")]
    [SerializeField] private float _clusterGridSpacing = 0.5f;

    private void Awake()
    {
        _spawnPoints = GetComponentsInChildren<Transform>();
    }

    /// <summary>
    /// 기본적인 적 소환 메서드
    /// </summary>
    /// <param name="poolIndex"></param>
    public void SpawnEnemy(int poolIndex)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            return;
        }

        Transform point = _spawnPoints[Random.Range(1, _spawnPoints.Length)];   // Spawner 스크립트가 붙은 자기자신을 제위하고 자식으로 있는 spawnPoint들에만 소환하게 하기 위해 인덱스를 1부터 시작
        GameObject enemy = GameManager.Instance.Pool.GetObjFromPool(poolIndex);
        enemy.transform.position = point.position; 
    }

    /// <summary>
    /// 디바이드 몬스터가 죽었을 때 그 자리에 두체의 스카우터 소환용 메서드
    /// </summary>
    /// <param name="scouterPoolIndex"></param>
    public void SpawnDividedScouters(int scouterPoolIndex, Vector3 deathPos)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject scouterObj = GameManager.Instance.Pool.GetObjFromPool(scouterPoolIndex);

            if (scouterObj != null)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 0.35f;
                scouterObj.transform.position = new Vector3(deathPos.x + randomOffset.x, 0f, deathPos.z + randomOffset.y);
            }
        }
    }

    /// <summary>
    /// 9쌍의 클러스터가 소환되기 위한 메서드
    /// </summary>
    /// <param name="poolIndex"></param>
    public void SpawnClusters(int clusterPoolIndex)
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

            GameObject clusterObj = GameManager.Instance.Pool.GetObjFromPool(clusterPoolIndex);

            if (clusterObj != null)
            {
                clusterObj.transform.position = worldSpawnPos;
            }
        }
    }

    /// <summary>
    /// 웨이브 마다 소환될 보스용 메서드
    /// </summary>
    /// <param name="poolIndex"></param>
    public void SpawnBoss(int poolIndex)
    {

    }
}
