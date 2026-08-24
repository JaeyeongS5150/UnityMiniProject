using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private float _scanRange = 8f;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Transform _nearestTarget;

    private RaycastHit[] _targets;

    public Transform NearestTarget => _nearestTarget;
    public float ScanRange => _scanRange;

    private void FixedUpdate()
    {
        _targets = Physics.SphereCastAll(transform.position, _scanRange, Vector3.zero, 0, _targetLayer);

        _nearestTarget = GetNearest();
    }

    private Transform GetNearest()
    {
        Transform result = null;
        float diff = 100; // 임시의 큰 값

        foreach (RaycastHit target in _targets)
        {
            Vector3 myPos = transform.position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _scanRange);
    }
}
