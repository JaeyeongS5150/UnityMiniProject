using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Scriptble Object/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public enum CCType
    {
        SlowField,      // 가로 막대 관통 투사체 -> 경로상 적 감속
        ConfusionRay,   // 단일 투사체 -> 첫 적 180도 역주행
        WarningStun,    // 랜덤 적 중심 장판 경고 후 광역 기절
        PushBarrier     // 관통 충격파 -> 적 뒤로 넉백
    }

    [Header("식별 정보")]
    [SerializeField] private CCType _ccType;
    [SerializeField] private string _towerName;
    [SerializeField] private Sprite _towerIcon;
    [TextArea(2, 3)]
    [SerializeField] private string _towerDescription;

    [Header("기본 쿨다운 및 사거리")]
    [SerializeField] private float _cooldown = 4.0f;
    [SerializeField] private float _range = 8.0f;
    [SerializeField] private float _subDamage = 5f;
    [SerializeField] private float _projectileSpeed = 10f;

    [Header("상태이상 수치")]
    [Tooltip("슬로우 감속 비율 (0.4 = 40% 감속)")]
    [Range(0f, 0.9f)]
    [SerializeField] private float _slowRatio = 0.4f;
    [Tooltip("CC 지속 시간 (슬로우, 역주행, 스턴 초)")]
    [SerializeField] private float _duration = 2.5f;
    [Tooltip("푸쉬 배리어 넉백 힘")]
    [SerializeField] private float _knockbackForce = 6.0f;
    [Tooltip("워닝 스턴 장판 반경")]
    [SerializeField] private float _stunAreaRadius = 3.5f;
    [Tooltip("워닝 스턴 발동 전 경고 시간")]
    [SerializeField] private float _warningDelay = 0.5f;

    [Header("오브젝트 풀링 인덱스")]
    [Tooltip("풀 매니저의 프리팹 배열에서의 인덱스 번호")]
    [SerializeField] private int _bulletPoolIndex;

    [Header("3D 프리팹")]
    [SerializeField] private GameObject _towerPrefab;
    [SerializeField] private GameObject _effectPrefab; // 투사체 또는 장판 프리팹

    #region 프로퍼티
    public CCType Type => _ccType;
    public string TowerName => _towerName;
    public Sprite TowerIcon => _towerIcon;
    public string TowerDescription => _towerDescription;
    public float Cooldown => _cooldown;
    public float Range => _range;
    public float SubDamage => _subDamage;
    public float ProjectileSpeed => _projectileSpeed;
    public float SlowRatio => _slowRatio;
    public float Duration => _duration;
    public float KnockbackForce => _knockbackForce;
    public float StunAreaRadius => _stunAreaRadius;
    public float WarningDelay => _warningDelay;
    public int BulletPoolIndex => _bulletPoolIndex;
    public GameObject TowerPrefab => _towerPrefab;
    public GameObject EffectPrefab => _effectPrefab;
    #endregion
}
