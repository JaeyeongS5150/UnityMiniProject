using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BaseCore : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private float _currentHp;
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _currentExp = 0;
    [SerializeField] private float _maxExp = 5;
    [SerializeField] private int _level = 1;
    [SerializeField] private int _kill = 0;

    [Header("추가 스탯")]
    [SerializeField] private float _bonusDamage = 0f;
    [SerializeField] private float _bonusFireRate = 1.0f;

    [Header("무기 시스템")]
    [SerializeField] private int _maxWeaponSlots = 4;
    [SerializeField] private WeaponDataSO _initialWeapon;
    [SerializeField] private Transform _weaponHolder;

    private readonly List<Weapon> _allAvailableWeapons = new List<Weapon>();
    private readonly List<Weapon> _equippedWeapons = new List<Weapon>();

    [Header("시각 연출 및 애니메이터")]
    [SerializeField] private Animator _animator;

    private static readonly int AnimDamaged = Animator.StringToHash("Damaged");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimLevelUp = Animator.StringToHash("LevelUp");

    private float _damageVisualTimer = 0f;
    private const float DamageVisualInterval = 0.2f;

    [Header("타워 오브젝트 연결")]
    [SerializeField] private Transform[] _towerSpawnPoints = new Transform[4];
    private Tower[] _equippedTowers = new Tower[4];

    [Header("피격 효과음 쿨다운")]
    [SerializeField] private float _hitSoundCooldown = 0.15f;
    private float _lastHitSoundTime = -1f;

    public int Level => _level;
    public float CurrentHp => _currentHp;
    public float MaxHp => _maxHp;
    public float CurrentExp => _currentExp;
    public float MaxExp => _maxExp;
    public int TotalKills => _kill;
    public IReadOnlyList<Weapon> EquipWeapons => _equippedWeapons;
    public IReadOnlyList<Tower> EquipTowers => _equippedTowers;
    public float BonusDamage => _bonusDamage;
    public float BonusFireRate => _bonusFireRate;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        _allAvailableWeapons.Clear();
        _equippedWeapons.Clear();

        if (_weaponHolder != null)
        {
            Weapon[] childWeapons = _weaponHolder.GetComponentsInChildren<Weapon>(true);
            foreach (var w in childWeapons)
            {
                _allAvailableWeapons.Add(w);
                w.gameObject.SetActive(false);
            }
        }
    }
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
        _maxHp = 100f + MetaShopData.GetBonusMaxHp();
        _currentHp = _maxHp;
        _currentExp = 0;
        _kill = 0;
        _level = 1;
        _bonusDamage = 0f;
        _bonusFireRate = 1.0f;
        _maxExp = GetRequiredExp(_level);
    }

    /// <summary>
    /// 특정 무기 타입이 이미 장착되어 있는지 검색
    /// </summary>
    public Weapon GetEquippedWeapon(WeaponDataSO.WeaponType type)
    {
        for (int i = 0; i < _equippedWeapons.Count; i++)
        {
            if (_equippedWeapons[i].Data != null && _equippedWeapons[i].Data.Type == type)
            {
                return _equippedWeapons[i];
            }
        }
        return null;
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

        Weapon existing = GetEquippedWeapon(weaponData.Type);

        if (existing != null)
        {
            existing.Upgrade();
            return true;
        }

        if (_equippedWeapons.Count < _maxWeaponSlots)
        {
            Weapon targetWeapon = _allAvailableWeapons.Find(w => w.Data != null && w.Data.Type == weaponData.Type);

            if (targetWeapon != null)
            {
                targetWeapon.gameObject.SetActive(true);
                targetWeapon.InitWeapon(weaponData);    
                _equippedWeapons.Add(targetWeapon);     
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 슬롯(0~3)에 타워를 소환하고 장착
    /// </summary>
    public bool BuildTower(int slotIndex, TowerDataSO towerData)
    {
        if (_towerSpawnPoints == null || slotIndex < 0 || slotIndex >= _towerSpawnPoints.Length)
        {
            return false;
        }

        if (_equippedTowers[slotIndex] != null)
        {
            return false;
        }

        if (towerData == null || towerData.TowerPrefab == null)
        {
            return false;
        }

        Transform spawnPoint = _towerSpawnPoints[slotIndex];
        if (spawnPoint == null)
        {
            return false;
        }

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
    /// 특정 타워를 이미 보유하고 있는지 확인 (LevelUp 추첨 필터링용)
    /// </summary>
    public bool HasTower(TowerDataSO towerData)
    {
        if (towerData == null) return false;

        for (int i = 0; i < _equippedTowers.Length; i++)
        {
            if (_equippedTowers[i] != null && _equippedTowers[i].Data == towerData)
            {
                return true;
            }
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

        if (_animator != null)
        {
            _animator.SetTrigger(AnimLevelUp);
        }

        if (GameManager.Instance != null && GameManager.Instance.LevelUp != null)
        {
            GameManager.Instance.LevelUp.OpenLevelUp();
        }
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

        PlayHitSound();

        if (_currentHp <= 0)
        {
            Die();
        }

        _damageVisualTimer += Time.deltaTime;
        if (_damageVisualTimer >= DamageVisualInterval)
        {
            _damageVisualTimer = 0f;
            if (_animator != null)
            {
                _animator.SetTrigger(AnimDamaged);
            }
        }

    }

    private void PlayHitSound()
    {
        if (Time.unscaledTime - _lastHitSoundTime >= _hitSoundCooldown)
        {
            _lastHitSoundTime = Time.unscaledTime;

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(AudioManager.SFX.Hit);
            }
        }
    }

    /// <summary>
    /// LevelUp에서 스탯 버프 카드 선택 시 호출
    /// </summary>
    public void ApplyStatBuff(LevelUpDataSO.StatBuffType buffType, float buffValue)
    {
        switch (buffType)
        {
            case LevelUpDataSO.StatBuffType.AttackPower:
                _bonusDamage += buffValue;
                break;

            case LevelUpDataSO.StatBuffType.AttackSpeed:
                _bonusFireRate = Mathf.Max(0.3f, _bonusFireRate - buffValue);
                break;

            case LevelUpDataSO.StatBuffType.CoreHealth:
                _maxHp += buffValue;
                _currentHp = Mathf.Min(_currentHp + buffValue, _maxHp);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!GameManager.Instance.IsLive || !other.CompareTag("Enemy"))
        {
            return;
        }

        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            TakeDamage(enemy.Damage * Time.deltaTime);
        }
    }

    private void Die()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.SFX.GameOver);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// 무기 발사 시 Weapon.cs 등에서 호출
    /// </summary>
    public void PlayAttackMotion()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(AnimAttack);
        }
    }
}
