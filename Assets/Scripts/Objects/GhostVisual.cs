using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostVisual : MonoBehaviour
{
    [Header("점멸 주기 기본 설정")]
    [Tooltip("모습을 드러내고 있는 시간 (초)")]
    [SerializeField] private float _visibleDuration = 2.0f;

    [Tooltip("은신 시 반투명도")]
    [SerializeField] private float _stealthAlpha = 0.2f;

    private Enemy _parentEnemy;
    private MeshRenderer _meshRenderer;
    private Collider _rootCollider;
    private MaterialPropertyBlock _propBlock;
    private Coroutine _blinkRoutine;
    private static readonly int BaseColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _parentEnemy = GetComponentInParent<Enemy>();
        _rootCollider = GetComponentInParent<Collider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_propBlock == null)
        {
            _propBlock = new MaterialPropertyBlock();
        }
    }

    private void OnEnable()
    {
        if (_parentEnemy == null) _parentEnemy = GetComponentInParent<Enemy>();
        if (_rootCollider == null) _rootCollider = GetComponentInParent<Collider>();
        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();

        SetStealth(false);

        if (_blinkRoutine != null)
        {
            StopCoroutine(_blinkRoutine);
        }
        _blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        if (_blinkRoutine != null)
        {
            StopCoroutine(_blinkRoutine);
            _blinkRoutine = null;
        }

        SetStealth(false);
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            if (_parentEnemy != null && _parentEnemy.IsDead)
            {
                SetStealth(false);
                yield break;
            }

            SetStealth(false);
            yield return new WaitForSeconds(_visibleDuration);

            if (_parentEnemy != null && _parentEnemy.IsDead)
            {
                SetStealth(false);
                yield break;
            }

            float invisibleDuration = 1.0f;
            if (_parentEnemy != null && _parentEnemy.Data != null && _parentEnemy.Data.GhostTime > 0f)
            {
                invisibleDuration = _parentEnemy.Data.GhostTime;
            }

            SetStealth(true);
            yield return new WaitForSeconds(invisibleDuration);
        }
    }

    private void SetStealth(bool isStealth)
    {
        if (_meshRenderer != null && _meshRenderer.sharedMaterial != null)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _meshRenderer.GetPropertyBlock(_propBlock);
            Color col = _meshRenderer.sharedMaterial.color;
            col.a = isStealth ? _stealthAlpha : 1.0f;
            _propBlock.SetColor(BaseColorId, col);
            _meshRenderer.SetPropertyBlock(_propBlock);
        }

        if (_rootCollider != null)
        {
            _rootCollider.enabled = !isStealth;
        }
    }
}
