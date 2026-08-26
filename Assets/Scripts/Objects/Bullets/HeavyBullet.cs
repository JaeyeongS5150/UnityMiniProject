using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class HeavyBullet : MonoBehaviour
{
    [Header("HeavyBullet 정보")]
    [SerializeField] private float _damage;
    [SerializeField] private float _knockBackForce;

    private Rigidbody _rb;
    private Collider _coll;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        _rb.useGravity = false;
        _coll.isTrigger = true;
    }

    public void SetupHeavy(float damage, Vector3 dir, float speed, float knockbackForce)
    {
        _damage = damage;
        _knockBackForce = knockbackForce;

        Vector3 moveDir = dir.normalized;
        _rb.velocity = moveDir * speed;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(_damage);
            enemy.ApplyKnockback(_knockBackForce);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("KillZone"))
        {
            gameObject.SetActive(false);
        }
    }


}
