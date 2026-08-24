using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("게임 컨트롤")]
    [SerializeField] private float _gameTime;
    //[SerializeField] private float _gameSpeed;

    [Header("오브젝트들 연결")]
    [SerializeField] private PoolManager _pool;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }



    }

    void Start()
    {
        
    }


    void Update()
    {
        _gameTime += Time.deltaTime;
    }

    private void GamePause()
    {

    }

    private void GameResume()
    {

    }

    private void GameFinished()
    {

    }
}
