using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelUpData", menuName = "Scriptble Object/LevelUpDataSO")]
public class LevelUpDataSO : ScriptableObject
{
    public enum RewardType
    {
        WeaponUpgrade,
        TowerUpgrade,
        StatBuff
    }

    public enum StatBuffType
    {
        None,
        AttackPower,
        AttackSpeed,
        CoreHealth  
    }

    [Header("보상 분류")]
    [SerializeField] private RewardType _rewardType;
    [SerializeField] private StatBuffType _statBuffType = StatBuffType.None;

    [Header("데이터 연결 (무기, 타워)")]
    [SerializeField] private WeaponDataSO _weaponData;
    [SerializeField] private TowerDataSO _towerData;

    [Header("스탯 버프 전용 표시")]
    [SerializeField] private string _buffName;
    [SerializeField] private Sprite _buffIcon;
    [TextArea(2, 3)]
    [SerializeField] private string _buffDescription;
    [Tooltip("버프 배율 or 수치")]
    [SerializeField] private float _buffValue = 0.1f;

    public RewardType Type => _rewardType;
    public StatBuffType BuffType => _statBuffType;
    public WeaponDataSO WeaponData => _weaponData;
    public TowerDataSO TowerData => _towerData;
    public float BuffValue => _buffValue;

    public string Name => _weaponData != null ? _weaponData.WeaponName : (_towerData != null ? _towerData.TowerName : _buffName);
    public Sprite Icon => _weaponData != null ? _weaponData.WeaponIcon : (_towerData != null ? _towerData.TowerIcon : _buffIcon);
    public string Description => _buffDescription;
}
