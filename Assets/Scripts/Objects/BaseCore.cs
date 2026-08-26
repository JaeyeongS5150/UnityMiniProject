using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class BaseCore : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private float _currentHp;
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private int _currentExp = 0;
    [SerializeField] private int _maxExp = 5;
    [SerializeField] private int _level = 1;
    [SerializeField] private int _kill = 0;

    [Header("무기 시스템")]
    [SerializeField] private int _maxWeaponSlots = 4;
    [SerializeField] private WeaponDataSO _initialWeapon;
    [SerializeField] private Transform _weaponHolder;
    private readonly List<Weapon> _equippedWeapons = new List<Weapon>();

    [Header("오브젝트 연결")]
    [SerializeField] private Transform[] _towerSpawnPoints = new Transform[4];
    private Tower[] _equippedTowers = new Tower[4];

    public int Level => _level;
    public float CurrentHp => _currentHp;
    public float MaxHp => _maxHp;
    public float CurrentExp => _currentExp;
    public float MaxExp => _maxExp;
    public IReadOnlyList<Weapon> EquipWeapons => _equippedWeapons;

    private void Start()
    {
        InitCore();

        if (_initialWeapon != null)
        {
            AddOrUpgradeWeapon(_initialWeapon);
        }
    }

    private void InitCore()
    {
        _maxHp = 100;
        _currentHp = _maxHp;
        _currentExp = 0;
        _kill = 0;
        _level = 1;
        _maxExp = GetRequiredExp(_level);
    }

    /// <summary>
    /// 무기 획득 또는 기존 무기 레벨업 (레벨업 3지선다 카드 선택 시 호출)
    /// </summary>
    public bool AddOrUpgradeWeapon(WeaponDataSO weaponData)
    {
        if (weaponData == null)
        {
            return false;
        }

        Weapon existing = _equippedWeapons.Find(w => w.Data.Type == weaponData.Type);

        if (existing != null)
        {
            existing.Upgrade();
            return true;
        }

        if (_equippedWeapons.Count < _maxWeaponSlots)
        {
            Transform parent = _weaponHolder != null ? _weaponHolder : transform;

            GameObject weaponObj = new GameObject($"Weapon_{weaponData.WeaponName}");

            weaponObj.transform.SetParent(parent, false);

            Weapon newWeapon = weaponObj.AddComponent<Weapon>();

            newWeapon.InitWeapon(weaponData);

            _equippedWeapons.Add(newWeapon);

            return true;
        }

        return false;
    }

    /// <summary>
    /// 특정 슬롯(0~3)에 타워를 소환하고 장착
    /// </summary>
    public bool BuildTower(int slotIndex, TowerDataSO towerData)
    {
        if (slotIndex < 0 || slotIndex >= _towerSpawnPoints.Length)
        {
            return false;
        }

        if (_equippedTowers[slotIndex] != null)
        {
            return false;
        }

        if (_towerSpawnPoints[slotIndex] == null || towerData.TowerPrefab == null)
        {
            return false;
        }

        Transform spawnPoint = _towerSpawnPoints[slotIndex];
        GameObject towerObj = Instantiate(towerData.TowerPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

        if (towerObj.TryGetComponent<Tower>(out var tower))
        {
            tower.InitTower(towerData);
            _equippedTowers[slotIndex] = tower;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 경험치 획득 (Enemy 사망시 호출)
    /// </summary>
    /// <param name="amount"></param>
    public void GetExp(int amount)
    {
        if (!GameManager.Instance.IsLive)
        {
            return;
        }

        _currentExp += amount;
        _kill++;

        if (_currentExp >= _maxExp)
        {
            LevelUP();
        }
    }

    /// <summary>
    /// 레벨업에 필요한 경험치 도달 시 호출
    /// </summary>
    private void LevelUP()
    {
        _level++;
        _currentExp -= _maxExp;

        _maxHp += 10;
        _currentHp = Mathf.Min(_currentHp + 10, _maxHp);
        _maxExp = GetRequiredExp(_level);
        // Levelup 스크립트 구현 후 연결
    }

    private int GetRequiredExp(int level)
    {
        if (level <= 5)
        {
            return level * 5;
        }

        if (level <= 15)
        {
            return 25 + (level - 5) * 10;
        }

        return 135 + (level - 16) * 18;
    }

    /// <summary>
    /// BasceCore 피격 시 체력감소 (Enemy에서 호출)
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        if (!GameManager.Instance.IsLive)
        {
            return;
        }

        _currentHp = Mathf.Max(_currentHp - damage, 0);

        if (_currentHp <= 0)
        {
            Die();
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (!GameManager.Instance.IsLive || !collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
        {
            TakeDamage(enemy.Damage * Time.deltaTime);
        }
    }

    private void Die()
    {
        GameManager.Instance.GameOver();
        // 사망 애니메이션
        // Gameover연결하기
    }

}
