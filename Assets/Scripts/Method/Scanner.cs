using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private float _scanRange = 8f;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Transform _nearestTarget;

    private Collider[] _hitBuffer = new Collider[50];

    public Transform NearestTarget => _nearestTarget;
    public float ScanRange
    { 
        get => _scanRange;
        set => _scanRange = value;
    }

    private void FixedUpdate()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _scanRange, _hitBuffer, _targetLayer);

        _nearestTarget = GetNearest(hitCount);
    }

    private Transform GetNearest(int hitCount)
    {
        Transform result = null;
        float shortestDistance = _scanRange * _scanRange;   // 임의의 큰 수
        Vector3 myPos = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _hitBuffer[i];
            if (col == null)
            {
                continue;
            }

            Vector3 targetPos = col.transform.position;
            targetPos.y = myPos.y;

            float sqrtDist = (targetPos - myPos).sqrMagnitude;

            if (sqrtDist < shortestDistance)
            {
                shortestDistance = sqrtDist;
                result = col.transform;
            }
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _nearestTarget != null ? Color.red : Color.green;

        Gizmos.DrawWireSphere(transform.position, _scanRange);
    }
}
