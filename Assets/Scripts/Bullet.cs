using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet 정보")]
    [SerializeField] private float _damage;
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

    public void Setup(float damage, int pierce, Vector3 dir, float speed, float maxRange)
    {
        _damage = damage;
        _pierceCount = pierce;
        _currentPierce = 0;

        Vector3 moveDir = dir.normalized;
        _rb.velocity = moveDir * speed;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(_damage);

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
