using System.Collections;
using UnityEngine;

public interface IWeaponStrategy
{
    void Fire(MonoBehaviour runner, Transform firePoint, Transform target, WeaponDataSO.WeaponLevelData stat, WeaponDataSO data);
}

[RequireComponent(typeof(Scanner))]
public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponDataSO _data;
    [SerializeField] private int _level = 1;
    [SerializeField] private Transform _firePoint;

    private Scanner _scanner;
    private IWeaponStrategy _strategy;
    private float _fireTimer = 0f;

    public WeaponDataSO Data => _data;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsLive || _data == null) return;

        HandleShooting();
    }

    public void InitWeapon(WeaponDataSO data)
    {
        _data = data;
        _level = 1;
        _fireTimer = 0f;
        _strategy = CreateStrategy(_data.Type);
        SyncScannerRange();
    }

    public void Upgrade()
    {
        if (_data != null && _level < _data.MaxLevel)
        {
            _level++;
            SyncScannerRange();
        }
    }

    private void SyncScannerRange()
    {
        if (_scanner != null && _data != null)
        {
            _scanner.ScanRange = _data.GetLevelData(_level).range;
        }
    }

    private void HandleShooting()
    {
        if (_strategy == null || _scanner == null) return;

        Transform target = _scanner.NearestTarget;
        var stat = _data.GetLevelData(_level);

        _fireTimer += Time.deltaTime;
        if (target != null && _fireTimer >= stat.fireRate)
        {
            _fireTimer = 0f;
            Transform spawnPoint = _firePoint != null ? _firePoint : transform;
            _strategy.Fire(this, spawnPoint, target, stat, _data);
        }
    }

    private IWeaponStrategy CreateStrategy(WeaponDataSO.WeaponType type)
    {
        switch (type)
        {
            case WeaponDataSO.WeaponType.OriginalCircle: return new OriginalCircleStrategy();
            case WeaponDataSO.WeaponType.DotStream: return new DotStreamStrategy();
            case WeaponDataSO.WeaponType.LinePiercer: return new LinePiercerStrategy();
            default: return new OriginalCircleStrategy();
        }
    }
}

// 1. 오리지널 서클 (단발 표준)
public class OriginalCircleStrategy : IWeaponStrategy
{
    public void Fire(MonoBehaviour runner, Transform firePoint, Transform target, WeaponDataSO.WeaponLevelData stat, WeaponDataSO data)
    {
        if (target == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : runner.transform.position;
        Vector3 dir = (target.position - spawnPos);
        dir.y = 0f;
        dir.Normalize();

        GameObject obj = Object.Instantiate(data.ProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        if (obj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Setup(stat.damage, stat.pierceCount, dir, stat.projectileSpeed, stat.range);
        }
    }
}

// 2. 도트 스트림 (점사)
public class DotStreamStrategy : IWeaponStrategy
{
    public void Fire(MonoBehaviour runner, Transform firePoint, Transform target, WeaponDataSO.WeaponLevelData stat, WeaponDataSO data)
    {
        runner.StartCoroutine(BurstRoutine(runner, firePoint, target, stat, data));
    }

    private IEnumerator BurstRoutine(MonoBehaviour runner, Transform firePoint, Transform target, WeaponDataSO.WeaponLevelData stat, WeaponDataSO data)
    {
        int count = stat.burstCount > 0 ? stat.burstCount : 3;
        float interval = stat.burstInterval > 0 ? stat.burstInterval : 0.1f;

        for (int i = 0; i < count; i++)
        {
            if (target == null || !target.gameObject.activeInHierarchy) yield break;

            Vector3 spawnPos = firePoint != null ? firePoint.position : runner.transform.position;
            Vector3 dir = (target.position - spawnPos);
            dir.y = 0f;
            dir.Normalize();

            GameObject obj = Object.Instantiate(data.ProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            if (obj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Setup(stat.damage, 0, dir, stat.projectileSpeed, stat.range);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}

// 3. 라인 피어서 (직선 관통)
public class LinePiercerStrategy : IWeaponStrategy
{
    public void Fire(MonoBehaviour runner, Transform firePoint, Transform target, WeaponDataSO.WeaponLevelData stat, WeaponDataSO data)
    {
        if (target == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : runner.transform.position;
        Vector3 dir = (target.position - spawnPos);
        dir.y = 0f;
        dir.Normalize();

        GameObject obj = Object.Instantiate(data.ProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        if (obj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Setup(stat.damage, stat.pierceCount, dir, stat.projectileSpeed, stat.range);
        }
    }
}