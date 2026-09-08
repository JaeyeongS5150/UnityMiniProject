using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class BoomerangBullet : MonoBehaviour
{
    [Header("BoomerangBullet 정보")]
    [SerializeField] private float _damage;
    [SerializeField] private float _speed;
    [SerializeField] private float _maxRange;
    [SerializeField] private float _returnDelay;
    [SerializeField] private float _spinSpeed = 720f;

    private bool _isFlying = false;

    private Transform _originTransform;
    private Rigidbody _rb;
    private Collider _coll;

    public bool IsFlying => _isFlying;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        _rb.useGravity = false;
        _coll.isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime, Space.Self);
    }

    public void SetupBoomerang(float damage, Vector3 dir, float speed, float maxRange, float returnDelay, Transform origin)
    {
        _damage = damage;
        _speed = speed;
        _maxRange = maxRange;
        _returnDelay = returnDelay;
        _originTransform = origin;

        StartCoroutine(BoomerangRoutine(dir.normalized));
    }

    private IEnumerator BoomerangRoutine(Vector3 forwardDir)
    {
        float traveled = 0f;
        _isFlying = true;

        while (traveled < _maxRange)
        {
            if (GameManager.Instance.IsLive)
            {
                float step = _speed * Time.deltaTime;
                transform.position += forwardDir * step;
                traveled += step;
            }
            yield return null;
        }

        yield return new WaitForSeconds(_returnDelay);

        while (_originTransform != null)
        {
            if (GameManager.Instance.IsLive)
            {
                Vector3 toOrigin = (_originTransform.position - transform.position);
                toOrigin.y = 0f;

                if (toOrigin.magnitude <= 0.5f)
                {
                    break;
                }

                transform.position += toOrigin.normalized * (_speed * 1.2f * Time.deltaTime);
            }
            yield return null;
        }

        _isFlying = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(_damage);
        }
    }

}
