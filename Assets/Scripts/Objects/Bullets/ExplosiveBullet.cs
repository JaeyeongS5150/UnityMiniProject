using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ExplosiveBullet : MonoBehaviour
{
    [Header("ExplosiveBullet 정보")]
    [SerializeField] private float _damage;
    [SerializeField] private float _splashRadius;

    private Rigidbody _rb;
    private Collider _coll;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        _rb.useGravity = false;
        _coll.isTrigger = true;
    }

    /// <summary>
    /// 더블 서클용 폭발 bullet 세팅
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="dir"></param>
    /// <param name="speed"></param>
    /// <param name="splashRadius"></param>
    public void SetupExplosive(float damage, Vector3 dir, float speed, float splashRadius)
    {
        _damage = damage;
        _splashRadius = splashRadius;

        Vector3 moveDir = dir.normalized;
        _rb.velocity = moveDir * speed;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("KillZone"))
        {
            // 착탄 지점 주변 광역 데미지
            Collider[] hits = Physics.OverlapSphere(transform.position, _splashRadius, LayerMask.GetMask("Enemy"));

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.TakeDamage(_damage);
                }
            }

            gameObject.SetActive(false);
        }
    }
}
