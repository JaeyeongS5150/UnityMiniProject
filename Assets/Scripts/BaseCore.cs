using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCore : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private float _currentHp;
    [SerializeField] private float _maxhp;
    [SerializeField] private int _currentExp;
    [SerializeField] private int _maxExp;
    [SerializeField] private int _level;
    [SerializeField] private int _kill;

    [Header("오브젝트 연결")]
    [SerializeField] Scanner _scanner;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
