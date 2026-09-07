using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Chasing,    // 기지 추적 접근
        AttackCore, // 기지 콜라이더 접촉 후 정지 및 공격
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

    [Header("공격 모션")]
    [SerializeField] private float _attackInterval = 1.0f;
    [SerializeField] private Animator _animator;

    private static readonly int AnimIsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int AnimAttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AnimDamagedTrigger = Animator.StringToHash("Damaged");
    private static readonly int AnimDieTrigger = Animator.StringToHash("Die");

    private Rigidbody _rb;
    private Collider _coll;
    private Coroutine _ccRoutine;
    private float _attackTimer = 0f;

    public float Damage => _damage;
    public bool IsDead => _state == EnemyState.Dead;
    public EnemyDataSO Data => _data;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        _rb.useGravity = false;
        _rb.isKinematic = false;
    }
    private void OnEnable()
    {
        if (_coll != null)
        {
            _coll.enabled = true;
        }

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

        switch (_state)
        {
            case EnemyState.Chasing:
            case EnemyState.Confused:
                MoveToCore();
                break;

            case EnemyState.AttackCore:
                HandleCoreAttack();
                break;
        }
    }

    /// <summary>
    /// Enemy 초기화 함수
    /// </summary>
    /// <param name="data"></param>
    /// <param name="targetCore"></param>
    public void InitEnemy(EnemyDataSO data, Transform targetCore = null)
    {
        _data = data;
        _currentHP = _data.MaxHP;
        _currentMoveSpeed = _data.MoveSpeed;
        _damage = _data.DamageToCore;
        _exp = _data.ExpReward;

        _state = EnemyState.Chasing;
        _attackTimer = 0f;

        if (_animator != null)
        {
            _animator.SetBool(AnimIsAttacking, false);
            _animator.ResetTrigger(AnimAttackTrigger);
            _animator.ResetTrigger(AnimDamagedTrigger);
            _animator.ResetTrigger(AnimDieTrigger);

            _animator.Play("Idle(Move)", 0, 0f);
            _animator.Update(0f);

            _animator.transform.localPosition = Vector3.zero;
            _animator.transform.localRotation = Quaternion.identity;
            _animator.transform.localScale = Vector3.one;
        }

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

        Vector3 targetPos = _mainCore.position;
        targetPos.y = transform.position.y;

        Vector3 moveDir = (targetPos - transform.position).normalized;

        if (_state == EnemyState.Confused)
        {
            moveDir = -moveDir;
        }

        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
        }

        transform.position += moveDir * (_currentMoveSpeed * Time.deltaTime);
    }

    #region 공격 관련
    /// <summary>
    /// 기지 접촉 시 1초마다 애니메이션 트리거 및 데미지 부여
    /// </summary>
    private void HandleCoreAttack()
    {
        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _attackInterval)
        {
            _attackTimer = 0f;

            if (_animator != null)
            {
                _animator.SetBool(AnimIsAttacking, true);
                _animator.SetTrigger(AnimAttackTrigger);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core") && _state == EnemyState.Chasing)
        {
            _state = EnemyState.AttackCore;
            _attackTimer = _attackInterval;

            if (_animator != null)
            {
                _animator.SetBool(AnimIsAttacking, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Core") && _state == EnemyState.AttackCore)
        {
            _state = EnemyState.Chasing;

            if (_animator != null)
            {
                _animator.SetBool(AnimIsAttacking, false);
            }
        }
    }
    #endregion

    /// <summary>
    /// Enemy의 체력이 감소하는 함수 (Bullet에서 호출)
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        if (_state == EnemyState.Dead)
        {
            return;
        }

        _currentHP -= amount;

        if (_currentHP <= 0)
        {
            _state = EnemyState.Dead;

            if (_animator != null)
            {
                _animator.SetBool(AnimIsAttacking, false);
                _animator.SetTrigger(AnimDieTrigger);
            }

            if (_data != null && _data.Type == EnemyDataSO.EnemyType.Divide)
            {
                GameManager.Instance.Spawner.SpawnDividedScouters(transform.position);
            }

            GameManager.Instance.Core.GetExp(_exp);

            if (GameManager.Instance.Wave != null)
            {
                GameManager.Instance.Wave.OnEnemyKilled(this);
            }
        }

        _animator.SetTrigger(AnimDamagedTrigger);
    }

    // 각 타워 or 무기에서 관리
    #region 상태이상
    /// <summary>
    /// 슬로우 적용 (이동속도 비율만큼 감소)
    /// </summary>
    /// <param name="ratio"></param>
    /// <param name="duration"></param>
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

    /// <summary>
    /// 혼란 적용 (이동방향 반대로 잠시 이동)
    /// </summary>
    /// <param name="duration"></param>
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

        if (_animator != null)
        {
            _animator.SetBool(AnimIsAttacking, false);
        }

        yield return new WaitForSeconds(duration);
        if (_state == EnemyState.Confused)
        {
            _state = EnemyState.Chasing;
        }
    }

    /// <summary>
    /// 구역안의 적 일시적으로 이동 멈춤
    /// </summary>
    /// <param name="duration"></param>
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

        if (_animator != null)
        {
            _animator.SetBool(AnimIsAttacking, false);
        }

        yield return new WaitForSeconds(duration);
        if (_state == EnemyState.Stunned)
        {
            _state = EnemyState.Chasing;
        }
    }

    /// <summary>
    /// 잠시 뒤로 force만큼 밀려남
    /// </summary>
    /// <param name="force"></param>
    public void ApplyKnockback(float force)
    {
        if (_rb != null && _state != EnemyState.Dead)
        {
            Vector3 corePos = _mainCore.transform.position;
            Vector3 dirVec = transform.position - corePos;
            dirVec.y = 0f;

            _rb.AddForce(dirVec.normalized * force, ForceMode.Impulse);
            _state = EnemyState.Chasing;

            if (_animator != null)
            {
                _animator.SetBool(AnimIsAttacking, false);
            }
        }
    }

    #endregion
}
