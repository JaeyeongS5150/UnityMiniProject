using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    [System.Serializable]
    public struct WeaponLevelData
    {
        [Header("기본 전투 수치")]
        public float damage;
        public float fireRate;
        public float range;
        public float projectileSpeed;

        [Header("투사체 판정 수치")]
        [Tooltip("투사체 수")]
        public int projectileCount;
        [Tooltip("크기 배율")]
        public float projectileScale;
        [Tooltip("관통 횟수")]
        public int pierceCount;
        [Tooltip("폭발 반경")]
        public float splashRadius;
        [Tooltip("점사 탄환 수")]
        public int burstCount;
        [Tooltip("점사 간격")]
        public float burstInterval;
        [Tooltip("최대 연쇄 전이 횟수")]
        public int chainCount;
        [Tooltip("연쇄 탐색 반경")]
        public float chainRadius;
        [Tooltip("부메랑 복귀 딜레이")]
        public float returnDelay;

        [Header("레벨별 설명")]
        [TextArea(1, 3)]
        public string levelDescription;
    }

    [Header("고유 식별 정보")]
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private string _weaponName;
    [SerializeField] private Sprite _weaponIcon;
    [TextArea(2, 4)]
    [SerializeField] private string _weaponDescription;

    [Header("프리팹 연결")]
    [SerializeField] private GameObject _projectilePrefab;
    // vfx prefabs
    // audioclip

    [Header("레벨별 스탯 (Index 0: Lv1 ~ Index 4: LvMax)")]
    [SerializeField] private WeaponLevelData[] _levelDataArray = new WeaponLevelData[5];

    #region 프로퍼티
    public WeaponType Type => _weaponType;
    public string WeaponName => _weaponName;
    public Sprite WeaponIcon => _weaponIcon;

    public GameObject ProjectilePrefab => _projectilePrefab;
    public int MaxLevel => _levelDataArray != null ? _levelDataArray.Length : 0;
    #endregion

    // 레벨을 받아 해당 레벨의 스탯 데이터를 안전하게 반환하기
    public WeaponLevelData GetLevelData(int level)
    {
        if (_levelDataArray == null || _levelDataArray.Length == 0)
        {
            Debug.LogWarning($"[{name}] WeaponDataSO에 레벨 데이터가 설정되지 않았습니다.");
            return default;
        }

        int targetIndex = Mathf.Clamp(level - 1, 0, _levelDataArray.Length - 1);

        return _levelDataArray[targetIndex];
    }

}
