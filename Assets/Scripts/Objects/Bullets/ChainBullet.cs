using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ChainBullet : MonoBehaviour
{
    [Header("ChainBullet 정보")]
    [SerializeField] private float _damage;
    [SerializeField] private int _remainChains;
    [SerializeField] private float _chainRadius;
    [SerializeField] private float _speed;

    private readonly List<Enemy> _hitEnemyList = new List<Enemy>();

    private Rigidbody _rb;
    private Collider _coll;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        _rb.useGravity = false;
        _coll.isTrigger = true;
    }

    public void SetupChain(float damage, Vector3 dir, float speed, int chainCount, float chainRadius)
    {
        _damage = damage;
        _remainChains = chainCount;
        _chainRadius = chainRadius;
        _speed = speed;

        _hitEnemyList.Clear();

        Vector3 moveDir = dir.normalized;
        _rb.velocity = moveDir * speed;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<Enemy>(out var enemy))
        {
            // 이미 맞은 적은 무시
            if (_hitEnemyList.Contains(enemy))
            {
                return;
            }

            enemy.TakeDamage(_damage);
            _hitEnemyList.Add(enemy);

            if (_remainChains > 0)
            {
                _remainChains--;
                FindNextChainTarget(enemy.transform.position);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else if (other.CompareTag("KillZone"))
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ChainBullet을 맞은 적 위치 주변에 가까운 적을 찾아서 방향 전환
    /// Scanner.cs와 비슷한 원리
    /// </summary>
    /// <param name="hitposition"></param>
    private void FindNextChainTarget(Vector3 hitposition)
    {
        Collider[] hits = Physics.OverlapSphere(hitposition, _chainRadius, LayerMask.GetMask("Enemy"));
        Transform nextTarget = null;
        float shortestDistance = _chainRadius * _chainRadius;
        
        for (int i = 0;  i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent<Enemy>(out Enemy nextEnemy))
            {
                if (!_hitEnemyList.Contains(nextEnemy))
                {
                    float sqrDist = (nextEnemy.transform.position - hitposition).sqrMagnitude;
                    if (sqrDist < shortestDistance)
                    {
                        shortestDistance = sqrDist;
                        nextTarget = nextEnemy.transform;
                    }
                }
            }
        }

        if (nextTarget != null)
        {
            Vector3 newDir = (nextTarget.position - transform.position);
            newDir.y = 0f;
            _rb.velocity = newDir.normalized * _speed;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
