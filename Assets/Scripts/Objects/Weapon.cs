using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Scanner))]
public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponDataSO _data;
    [SerializeField] private int _level = 1;
    [SerializeField] private Transform _firePoint;

    private Scanner _scanner;
    private float _fireTimer = 0f;
    private Coroutine _streamRoutine;

    public WeaponDataSO Data => _data;
    public int Level => _level;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();
    }

    private void Start()
    {
        InitWeapon(_data);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsLive || _data == null)
        {
            return;
        }

        HandleShooting();
    }

    /// <summary>
    /// 무기 생성 시 소출되는 초기화 함수
    /// </summary>
    /// <param name="data"></param>
    public void InitWeapon(WeaponDataSO data)
    {
        _data = data;
        _level = 1;
        _fireTimer = 0f;

        SyncScannerRange();
    }

    /// <summary>
    /// 무기 레벨업 시 호출 (레벨업 선택 카드에서 선택될 때 호출)
    /// </summary>
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
        if (_scanner == null)
        {
            return;
        }

        Transform target = _scanner.NearestTarget;
        var stat = _data.GetLevelData(_level);

        _fireTimer += Time.deltaTime;

        if (_streamRoutine == null && target != null && _fireTimer >= stat.fireRate)
        {
            _fireTimer = 0f;
            Transform spawnPoint = _firePoint != null ? _firePoint : transform;

            if (GameManager.Instance != null && GameManager.Instance.Core != null)
            {
                GameManager.Instance.Core.PlayAttackMotion();
            }

            switch (_data.Type)
            {
                case WeaponDataSO.WeaponType.OriginalCircle:
                    FireOriginalCircle(spawnPoint, target, stat);
                    break;
                case WeaponDataSO.WeaponType.DotStream:
                    _streamRoutine = StartCoroutine(DotStreamRoutine(spawnPoint, target, stat));
                    break;
                case WeaponDataSO.WeaponType.LinePiercer:
                    _streamRoutine = StartCoroutine(LinePiercerStreamRoutine(spawnPoint, target, stat));
                    break;
                case WeaponDataSO.WeaponType.DoubleCircle:
                    FireDoubleCircle(spawnPoint, target, stat);
                    break;
                case WeaponDataSO.WeaponType.HeavyCircle:
                    FireHeavyCircle(spawnPoint, target, stat);
                    break;
                case WeaponDataSO.WeaponType.ChainChord:
                    FireChainChord(spawnPoint, target, stat);
                    break;
                case WeaponDataSO.WeaponType.Moonmerang:
                    FireMoonMerang(spawnPoint, target, stat);
                    break;
            }
        }
    }

    #region 각 무기별 발사 메서드
    private void FireOriginalCircle(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        float lifeTime = stat.range / stat.projectileSpeed;
        Vector3 dir = (target.position - spawnPoint.position);
        dir.y = 0f;
        dir.Normalize();

        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);

        if (bulletObj != null)
        {
            bulletObj.transform.position = spawnPoint.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Setup(stat.damage, stat.pierceCount, dir, stat.projectileSpeed);
            }
        }
    }

    /// <summary>
    ///  점사로 (3 ~ 5점사) 총알 발사
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="target"></param>
    /// <param name="stat"></param>
    /// <returns></returns>
    private IEnumerator DotStreamRoutine(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        int count = stat.burstCount;
        float interval = stat.burstInterval;
        float lifeTime = stat.range / stat.projectileSpeed;

        for (int i = 0; i < count; i++)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                _streamRoutine = null;
                yield break;
            }

            Vector3 dir = (target.position - spawnPoint.position);
            dir.y = 0f;
            dir.Normalize();

            GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);

            if (bulletObj != null)
            {
                bulletObj.transform.position = spawnPoint.position;
                bulletObj.transform.rotation = Quaternion.LookRotation(dir);

                if (bulletObj.TryGetComponent<Bullet>(out var bullet))
                {
                    bullet.Setup(stat.damage, stat.pierceCount, dir, stat.projectileSpeed);
                }
            }

            yield return new WaitForSeconds(interval);
        }
        _streamRoutine = null;

    }

    /// <summary>
    /// [ 총알을 먼저 발사
    /// ■를 계속 발사
    /// 적이 죽으면 ]를 발사 후 공격 중단
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="target"></param>
    /// <param name="stat"></param>
    /// <returns></returns>
    private IEnumerator LinePiercerStreamRoutine(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        float lifeTime = stat.range / stat.projectileSpeed;
        float bodyInterval = 0.08f;

        if (target != null)
        {
            Vector3 dir = (target.position - spawnPoint.position);
            dir.y = 0f;
            dir.Normalize();

            SpawnLinePiercer(_data.HeadPoolIndex, spawnPoint.position, dir, stat, lifeTime);
        }

        yield return new WaitForSeconds(bodyInterval);

        while (true)
        {
            // 죽었거나(비활성화) null이거나 안보일때(고스트 enemy)
            bool isTargetLost = (target == null || !target.gameObject.activeInHierarchy);
            
            if (target.TryGetComponent<Enemy>(out var enemy) && enemy.IsDead)
            {
                isTargetLost = true;
            }
            else
            {
                float sqrDist = (target.position - spawnPoint.position).sqrMagnitude;
                if (sqrDist > (stat.range * stat.range))
                {
                    isTargetLost = true;
                }
            }

            if (isTargetLost)
            {
                break;
            }

            Vector3 dir = (target.position - spawnPoint.position);
            dir.y = 0f;
            dir.Normalize();

            SpawnLinePiercer(_data.BodyPoolIndex, spawnPoint.position, dir, stat, lifeTime);

            yield return new WaitForSeconds(bodyInterval);
        }

        Vector3 lastDir = spawnPoint.forward;
        if (target != null)
        {
            Vector3 dir = (target.position - spawnPoint.position);
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                lastDir = dir.normalized;
            }
        }

        SpawnLinePiercer(_data.TailPoolIndex, spawnPoint.position, lastDir, stat, lifeTime);

        _streamRoutine = null;
    }

    private void SpawnLinePiercer(int poolIndex, Vector3 pos, Vector3 dir, WeaponDataSO.WeaponLevelData stat, float lifeTime)
    {
        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(poolIndex, lifeTime);

        if (bulletObj != null)
        {
            bulletObj.transform.position = pos;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Setup(stat.damage, stat.pierceCount,dir,stat.projectileSpeed);
            }
        }
    }

    private void FireDoubleCircle(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        float lifeTime = stat.range / stat.projectileSpeed;
        Vector3 dir = (target.position - spawnPoint.position);
        dir.y = 0f;
        dir.Normalize();

        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);

        if (bulletObj != null)
        {
            bulletObj.transform.position = spawnPoint.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            if (bulletObj.TryGetComponent<ExplosiveBullet>(out var explosiveBullet))
            {
                explosiveBullet.SetupExplosive(stat.damage, dir, stat.projectileSpeed, stat.splashRadius);
            }
        }
    }

    private void FireHeavyCircle(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        float lifeTime = stat.range / stat.projectileSpeed;
        Vector3 dir = (target.position - spawnPoint.position);
        dir.y = 0f;
        dir.Normalize();

        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);

        if (bulletObj != null)
        {
            bulletObj.transform.position = spawnPoint.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            float scale = stat.projectileScale;
            bulletObj.transform.localScale = Vector3.one * scale;

            if (bulletObj.TryGetComponent<HeavyBullet>(out var heavyBullet))
            {
                heavyBullet.SetupHeavy(stat.damage, dir, stat.projectileSpeed, 8f);
            }
        }
    }

    private void FireChainChord(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        float lifeTime = stat.range / stat.projectileSpeed;
        Vector3 dir = (target.position - spawnPoint.position);
        dir.y = 0f;
        dir.Normalize();

        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);

        if (bulletObj != null)
        {
            bulletObj.transform.position = spawnPoint.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            if (bulletObj.TryGetComponent<ChainBullet>(out var chainBullet))
            {
                int chainCount = stat.chainCount;
                float chainRadius = stat.chainRadius;
                chainBullet.SetupChain(stat.damage, dir, stat.projectileSpeed, chainCount, chainRadius);
            }
        }
    }

    // 관통하는 부메랑 형식이기 때문에 lifeTime으로만 SetActive(false)됨
    private void FireMoonMerang(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {
        Vector3 dir = (target.position - spawnPoint.position);
        dir.y = 0f;
        dir.Normalize();

        float lifeTime = (stat.range / stat.projectileSpeed * 2.2f) + stat.returnDelay;

        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);
        if (bulletObj != null)
        {
            bulletObj.transform.position = spawnPoint.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            if (bulletObj.TryGetComponent<BoomerangBullet>(out var boomBullet))
            {
                boomBullet.SetupBoomerang(stat.damage, dir, stat.projectileSpeed, stat.range, stat.returnDelay, spawnPoint);
            }
        }
    }

    #endregion
}