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

    private void Update()
    {
        if (!GameManager.Instance.IsLive || _data == null) return;

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

            switch (_data.Type)
            {
                case WeaponDataSO.WeaponType.OriginalCircle:
                    FireOriginalCircle(spawnPoint, target, stat);
                    break;
                case WeaponDataSO.WeaponType.DotStream:
                    StartCoroutine(LinePiercerStreamRoutine(spawnPoint, target, stat));
                    break;
                case WeaponDataSO.WeaponType.LinePiercer:
                    break;
                case WeaponDataSO.WeaponType.DoubleCircle:
                    break;
                case WeaponDataSO.WeaponType.HeavyCircle:
                    break;
                case WeaponDataSO.WeaponType.ChainChord:
                    break;
                case WeaponDataSO.WeaponType.Moonmerang:
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

    private void FireDotStream(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {

    }

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

            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Setup(stat.damage, stat.pierceCount, dir, stat.projectileSpeed);
            }
        }
    }

    private void FireHeavyCircle(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {

    }

    private void FireChainChord(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {

    }

    private void FireMoonMerang(Transform spawnPoint, Transform target, WeaponDataSO.WeaponLevelData stat)
    {

    }

    #endregion
}