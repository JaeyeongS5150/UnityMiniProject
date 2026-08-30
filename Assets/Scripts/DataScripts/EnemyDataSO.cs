using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Scriptble Object/EnemyDataSO")]
public class EnemyDataSO : ScriptableObject
{
    public enum EnemyType
    {
        Scouter,
        Rush,
        Brick,
        Divide,
        Cluster,
        Ghost
    }

    [Header("고유 식별 정보")]
    [SerializeField] private EnemyType _enemyType;
    [SerializeField] private string _enemyName;
    [SerializeField] private Sprite _enemyIcon;
    [TextArea(2, 3)]
    [SerializeField] private string _enemyDescription;

    [Header("전투 스탯")]
    [SerializeField] private float _maxHP = 10f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _damageToCore = 1f;
    [SerializeField] private int _expReward = 1;
    //[Tooltip("라운드 난이도 계산용 스폰 코스트")]
    //[SerializeField] private int _spawnCost = 1;

    [Header("디바이드 특성 전용")]
    [SerializeField] private int _splitCount = 2;
    [SerializeField] private GameObject _dividedPrefab;

    [Header("클러스터 특성 전용")]
    [SerializeField] private int _clusterCount = 9;

    [Header("고스트 특성 전용")]
    [SerializeField] private float _ghostTime = 0.3f;

    [Header("오브젝트 풀링 인덱스")]
    [Tooltip("풀 매니저의 프리팹 배열에서의 인덱스 번호")]
    [SerializeField] private int _enemyPoolIndex;

    [Header("프리팹 연결")]
    [SerializeField] private GameObject _enemyPrefab;

    #region 프로퍼티
    public EnemyType Type => _enemyType;
    public string Name => _enemyName;
    public Sprite Icon => _enemyIcon;
    public float MaxHP => _maxHP;
    public float MoveSpeed => _moveSpeed;
    public float DamageToCore => _damageToCore;
    public int ExpReward => _expReward;
    //public int Cost => _spawnCost;

    public int SplitCount => _splitCount;
    public int ClusterCount => _clusterCount;
    public float GhostTime => _ghostTime;

    public int EnemyPoolIndex => _enemyPoolIndex;
    public GameObject EnemyPrefab => _enemyPrefab;
    #endregion
}
