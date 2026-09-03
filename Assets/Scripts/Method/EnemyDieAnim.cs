using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDieAnim : MonoBehaviour
{
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
    }
    public void OnDeathAnimationEnd()
    {
        if (_enemy != null)
        {
            _enemy.gameObject.SetActive(false);
        }
    }
}
