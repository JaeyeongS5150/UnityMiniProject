using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TowerDataSO;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class TowerBullet : MonoBehaviour
{
    [Header("TowerBullet 정보")]
    [SerializeField] private TowerDataSO.CCType _type;
    [SerializeField] private float _damage;
    [SerializeField] private float _ccDuration;
    [SerializeField] private float _ccValue;
    [SerializeField] private int _pierceCount = 0;

    private int _currentPierce = 0;

    private Rigidbody _rb;
    private Collider _coll;

    public float Damage => _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        _rb.useGravity = false;
        _coll.isTrigger = true;
    }

    public void Setup(TowerDataSO data, Vector3 dir, bool isPiercing)
    {
        _type = data.Type;
        _damage = data.SubDamage;
        _ccDuration = data.Duration;
        _pierceCount = isPiercing ? 99 : 0;
        _currentPierce = 0;

        if (_type == TowerDataSO.CCType.SlowField)
        {
            _ccValue = data.SlowRatio;
        }
        else if (_type == TowerDataSO.CCType.PushBarrier)
        {
            _ccValue = data.KnockbackForce;
        }

        Vector3 moveDir = dir.normalized;
        _rb.velocity = moveDir * data.ProjectileSpeed;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(_damage);

            switch (_type)
            {
                case TowerDataSO.CCType.SlowField:
                    enemy.ApplySlow(_ccValue, _ccDuration);
                    break;

                case TowerDataSO.CCType.ConfusionRay:
                    enemy.ApplyConfusion(_ccDuration);
                    break;

                case TowerDataSO.CCType.PushBarrier:
                    enemy.ApplyKnockback(_ccValue);
                    break;
            }

            if (_pierceCount > 0 && _currentPierce < _pierceCount)
            {
                _currentPierce++;
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
}
