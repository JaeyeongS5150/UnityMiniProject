using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly HashSet<Enemy> _hitEnemies = new HashSet<Enemy>();

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

    // TODO 체인 전이 구현하기
}
