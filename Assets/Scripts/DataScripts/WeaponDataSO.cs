using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Scriptble Object/WeaponDataSO")]
public class WeaponDataSO : ScriptableObject
{
    public enum WeaponType
    {
        OriginalCircle, // 오리지널 서클 (단발 기본 사격)
        DotStream,      // 도트 스트림 (3연사)
        LinePiercer,    // 라인 피어서 (직선 관통)
        DoubleCircle,   // 더블 서클 (스플래시 폭발)
        HeavyCircle,    // 헤비 서클 (강력한 한 방, 넉백)
        ChainChord,     // 체인 코드 (연쇄 전이 번개)
        Moonmerang      // 문메랑 (왕복 부메랑)
    }

    [Header("기본 정보")]
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private string _weaponName;
    [SerializeField] private Sprite _weaponIcon;
    [TextArea(2, 4)]
    [SerializeField] private string _weaponDescription;

    [Header("무기 능력치")]
    [Tooltip("기본 데미지")]
    [SerializeField] private float _damage = 10f;
    [Tooltip("초당 발사 간격 or 쿨다운")]
    [SerializeField] private float _fireRate = 1f;
    [Tooltip("포탑의 탐색 및 유효 사거리")]
    [SerializeField] private float _range = 8f;
    [Tooltip("투사체 속도")]
    [SerializeField] private float _projectileSpeed = 12f;

    [Header("프리팹 연결")]
    [SerializeField] private GameObject _projectilePrefab;

    #region 프로퍼티
    public WeaponType Type => _weaponType;
    public string WeaponName => _weaponName;
    public Sprite WeaponIcon => _weaponIcon;
    public string WeaponDescription => _weaponDescription;
    public float Damage => _damage;
    public float FireRate => _fireRate;
    public float Range => _range;
    public float ProjectileSpeed => _projectileSpeed;


    public GameObject ProjectilePrefab => _projectilePrefab;
    #endregion
}
