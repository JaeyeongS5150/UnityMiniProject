using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class BoomerangBullet : MonoBehaviour
{
    [Header("BoomerangBullet 정보")]
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

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
