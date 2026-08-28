using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("웨이브 매니저  정보")]
    [SerializeField] private int _wave;
    [SerializeField] private int _currentWaveKills;
    [SerializeField] private int[] _needKills = { 25, 40, 60, 85, 120, 150, 170, 190, 210, 230 };



    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
