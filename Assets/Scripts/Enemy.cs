using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Chasing,    // 기지 추적 접근
        Stunned,    // 기절 (완전 정지)
        Confused,   // 혼란 (180도 역주행)
        Dead        // 사망 처리
    }

    [Header("데이터 연결")]
    [SerializeField] private EnemyDataSO _data;

    [Header("타겟 연결")]
    [SerializeField] private Transform _mainCore;

    [Header("기본 정보")]
    [SerializeField] private float _currentHP;
    [SerializeField] private float _damage;
    [SerializeField] private int _exp;
    [SerializeField] private float _currentMoveSpeed;
    [SerializeField] private EnemyState _state = EnemyState.Chasing;

    private Rigidbody _rb;
    private Collider _coll;
    private Coroutine _ccRoutine;

    public float Damage => _damage;
    public bool IsDead => _state == EnemyState.Dead;
    public EnemyDataSO Data => _data;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        _rb.useGravity = false;
        _rb.isKinematic = false;
    }
    private void OnEnable()
    {
        if (_data != null)
        {
            InitEnemy(_data, _mainCore);
        }
    }

    void Update()
    {
        if (!GameManager.Instance.IsLive || _state == EnemyState.Dead || _state == EnemyState.Stunned)
        {
            return;
        }

        MoveToCore();
    }

    public void InitEnemy(EnemyDataSO data, Transform targetCore = null)
    {
        _data = data;
        _currentHP = _data.MaxHP;
        _currentMoveSpeed = _data.MoveSpeed;
        _damage = _data.DamageToCore;
        _exp = _data.ExpReward;

        _state = EnemyState.Chasing;

        if (targetCore != null)
        {
            _mainCore = targetCore;
        }
        else if(_mainCore == null)
        {
            GameObject coreObj = GameObject.FindGameObjectWithTag("Core");

            if (coreObj != null)
            {
                _mainCore = coreObj.transform;
            }
        }
    }

    private void MoveToCore()
    {
        if (_mainCore == null)
        {
            return;
        }

        // 추후에 메인 코어 모델링 후 위치 제대로 잡기!!
        Vector3 targetPos = _mainCore.position;
        targetPos.y = transform.position.y;

        Vector3 moveDir = (targetPos - transform.position).normalized;

        if (_state == EnemyState.Confused)
        {
            moveDir = -moveDir;
        }

        transform.position += moveDir * (_currentMoveSpeed * Time.deltaTime);
    }



    public void TakeDamage(float amount)
    {
        if (_state == EnemyState.Dead)
        {
            return;
        }

        _currentHP -= amount;

        if (_currentHP <= 0)
        {
            Die();
        }
    }

    // 각 타워 or 무기에서 관리
    #region 상태이상
    public void ApplySlow(float ratio, float duration)
    {
        if (_state == EnemyState.Dead)
        {
            return;
        }

        StartCoroutine(SlowRoutine(ratio, duration));
    }

    private IEnumerator SlowRoutine(float ratio, float duration)
    {
        _currentMoveSpeed = _data.MoveSpeed * (1f - ratio);
        yield return new WaitForSeconds(duration);
        _currentMoveSpeed = _data.MoveSpeed;
    }

    public void ApplyConfusion(float duration)
    {
        if (_state == EnemyState.Dead)
        {
            return;
        }

        if (_ccRoutine != null)
        {
            StopCoroutine(_ccRoutine);
        }

        _ccRoutine = StartCoroutine(ConfusionRoutine(duration));
    }

    private IEnumerator ConfusionRoutine(float duration)
    {
        _state = EnemyState.Confused;
        yield return new WaitForSeconds(duration);
        if (_state == EnemyState.Confused)
        {
            _state = EnemyState.Chasing;
        }
    }

    public void ApplyStun(float duration)
    {
        if (_state == EnemyState.Dead)
        {
            return;
        }

        if (_ccRoutine != null)
        {
            StopCoroutine(_ccRoutine);
        }

        _ccRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        _state = EnemyState.Stunned;
        yield return new WaitForSeconds(duration);
        if (_state == EnemyState.Stunned)
        {
            _state = EnemyState.Chasing;
        }
    }

    public void ApplyKnockback(Vector3 pushDir, float force)
    {
        if (_rb != null && _state != EnemyState.Dead)
        {
            Vector3 corePos = _mainCore.transform.position;
            Vector3 dirVec = transform.position - corePos;
            dirVec.y = 0f;

            _rb.AddForce(dirVec.normalized * force, ForceMode.Impulse);
        }
    }

    #endregion


    private void Die()
    {
        _state = EnemyState.Dead;

        if (_mainCore != null && _mainCore.TryGetComponent<BaseCore>(out BaseCore core))
        {
            core.GetExp(_exp);
        }

        gameObject.SetActive(false);
    }

}
