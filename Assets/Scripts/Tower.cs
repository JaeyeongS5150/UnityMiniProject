using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("데이터 연결")]
    [SerializeField] private TowerDataSO _data;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Scanner _scanner;

    private Transform _coreTransform;
    private float _slotAngle;
    private float _orbitRadius = 1.8f;
    private float _timer = 0f;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();

    }


    private void Update()
    {
        if (!GameManager.Instance.IsLive || _data == null) return;

        HandleRotation();

        _timer += Time.deltaTime;

        if (_timer >= _data.Cooldown)
        {
            _timer = 0f;
            //ExecuteCCAction();
        }
    }

    public void InitTower(TowerDataSO data)
    {
        _data = data;
        _timer = 0f;

        if (_scanner != null && _data != null)
        {
            // Scanner에 ScanRange setter 또는 내부 필드 동기화 처리
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }

    //private void ExecuteCCAction()
    //{
    //    Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
    //    Vector3 forwardDir = transform.forward;
    //    Transform target = _scanner != null ? _scanner.NearestTarget : null;

    //    switch (_data.Type)
    //    {
    //        case TowerDataSO.CCType.SlowField:
    //            CCTowerActions.FireSlowField(spawnPos, forwardDir, _data);
    //            break;

    //        case TowerDataSO.CCType.ConfusionRay:
    //            if (target != null) CCTowerActions.FireConfusionRay(spawnPos, forwardDir, _data);
    //            break;

    //        case TowerDataSO.CCType.WarningStun:
    //            StartCoroutine(CCTowerActions.TriggerWarningStunRoutine(transform.position, _data));
    //            break;

    //        case TowerDataSO.CCType.PushBarrier:
    //            CCTowerActions.FirePushBarrier(spawnPos, forwardDir, _data);
    //            break;
    //    }
    //}
}
