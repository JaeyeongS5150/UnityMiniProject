using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Scanner))]
public class Tower : MonoBehaviour
{
    [Header("데이터 연결")]
    [SerializeField] private TowerDataSO _data;
    [SerializeField] private Transform _firePoint;

    [Header("시각 연출 및 애니메이터")]
    [SerializeField] private Animator _animator;

    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    private Scanner _scanner;
    private float _timer = 0f;

    public TowerDataSO Data => _data;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.IsLive || _data == null)
        {
            return;
        }

        HandleRotation();

        _timer += Time.deltaTime;

        if (_timer >= _data.Cooldown)
        {
            if (_scanner != null && _scanner.NearestTarget != null)
            {
                _timer = 0f;
                ExecuteCCAction();
            }
        }
    }

    public void InitTower(TowerDataSO data)
    {
        _data = data;
        _timer = 0f;

        if (_scanner != null && _data != null)
        {
            _scanner.ScanRange = _data.Range;
        }
    }

    private void HandleRotation()
    {
        if (_scanner == null || _scanner.NearestTarget == null)
        {
            return;
        }

        Vector3 dir = _scanner.NearestTarget.position - transform.position;

        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void ExecuteCCAction()
    {
        Transform spawnPos = _firePoint != null ? _firePoint : transform;
        Transform target = _scanner.NearestTarget;

        if (_animator != null)
        {
            _animator.SetTrigger(AnimAttack);
        }

        switch (_data.Type)
        {
            case TowerDataSO.CCType.SlowField:
                FireTowerBullet(spawnPos, target, isPiercing: true);
                break;

            case TowerDataSO.CCType.ConfusionRay:
                FireTowerBullet(spawnPos, target, isPiercing: false);
                break;

            case TowerDataSO.CCType.WarningStun:
                StartCoroutine(WarningStunAreaRoutine());
                break;

            case TowerDataSO.CCType.PushBarrier:
                FireTowerBullet(spawnPos, target, isPiercing: true);
                break;
        }
    }

    #region 각 타워별 메서드
    /// <summary>
    /// 투사체형 CC 포탑 발사 처리 (슬로우, 혼란, 푸쉬)
    /// </summary>
    private void FireTowerBullet(Transform origin, Transform target, bool isPiercing)
    {
        float lifeTime = _data.Range / _data.ProjectileSpeed;
        Vector3 dir = (target.position - origin.position);
        dir.y = 0f;
        dir.Normalize();

        GameObject bulletObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, lifeTime);

        if (bulletObj != null)
        {
            bulletObj.transform.position = origin.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(dir);

            if (bulletObj.TryGetComponent<TowerBullet>(out var bullet))
            {
                bullet.Setup(_data, dir, isPiercing);
            }
        }
    }

    /// <summary>
    /// 워닝 스턴 전용 장판 코루틴 (랜덤 적 중심 경고 -> 시간차 광역 기절)
    /// </summary>
    private IEnumerator WarningStunAreaRoutine()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _data.Range, LayerMask.GetMask("Enemy"));
        if (hits.Length == 0)
        {
            yield break;
        }

        Transform randomEnemy = hits[Random.Range(0, hits.Length)].transform;
        if (randomEnemy == null)
        {
            yield break;
        }

        Vector3 areaPos = randomEnemy.position;
        areaPos.y = 0.05f;

        float visualLifeTime = _data.WarningDelay + 0.15f;
        GameObject stunObj = GameManager.Instance.Pool.GetObjFromPool(_data.BulletPoolIndex, visualLifeTime);

        if (stunObj != null)
        {
            stunObj.transform.position = areaPos;
            stunObj.transform.rotation = Quaternion.identity;

            stunObj.transform.localScale = Vector3.one * (_data.StunAreaRadius * 2f);

            yield return new WaitForSeconds(_data.WarningDelay);

            Collider[] targets = Physics.OverlapSphere(areaPos, _data.StunAreaRadius, LayerMask.GetMask("Enemy"));

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null && targets[i].TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.ApplyStun(_data.Duration);
                    enemy.TakeDamage(_data.SubDamage);
                }
            }
        }
        
    }
    #endregion
}
