using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Header("프리팹 목록")]
    [SerializeField] private GameObject[] _prefabs = null;

    private Queue<GameObject>[] _pools;

    private readonly List<GameObject> _aliveObjects = new List<GameObject>();
    private readonly Dictionary<GameObject, float> _lifeMap = new Dictionary<GameObject, float>();
    private readonly Dictionary<GameObject, int> _itemIndexMap = new Dictionary<GameObject, int>();

    private void Awake()
    {
        if (_prefabs == null || _prefabs.Length == 0)
        {
            Debug.Log("프리팹이 없음");
            enabled = false;
            return;
        }

        // 프리팹 종류만큼 Queue 배열 생성
        _pools = new Queue<GameObject>[_prefabs.Length];

        for (int i = 0; i < _prefabs.Length; i++)
        {
            _pools[i] = new Queue<GameObject>();
        }
    }

    void Update()
    {
        UpdateAlive();
    }

    /// <summary>
    /// 특정 풀 인덱스의 오브젝트 중 현재 필드에 살아있는 개수 조회
    /// </summary>
    public int GetAliveCount(int poolIndex)
    {
        int count = 0;
        for (int i = 0; i < _aliveObjects.Count; i++)
        {
            if (_aliveObjects[i] != null && _aliveObjects[i].activeSelf)
            {
                if (_itemIndexMap.TryGetValue(_aliveObjects[i], out int idx) && idx == poolIndex)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// index번째의 프리팹을 풀에서 꺼내거나 새로 만들기
    /// custommLifeTime에 해당 프리팹의 수명 입력
    /// 생략 or -1 or 0일 경우 수명없이 계속 살아있음
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject GetObjFromPool(int index, float customLifeTime = -1f)
    {
        if (index < 0 || index >= _prefabs.Length)
        {
            Debug.LogError($"잘못된 풀 인덱스: {index}");
            return null;
        }

        GameObject obj = null;

        if (_pools[index].Count > 0)
        {
            obj = _pools[index].Dequeue();
        }
        else
        {
            obj = Instantiate(_prefabs[index]);
        }

        _itemIndexMap[obj] = index;

        // 1. 활성화 및 부모 분리
        obj.transform.SetParent(null);
        obj.SetActive(true);

        _aliveObjects.Add(obj);

        // 2. 수명 및 살아있는 목록에 등록
        if (customLifeTime > 0f)
        {
            _lifeMap[obj] = customLifeTime;
        }

        return obj;
    }

    /// <summary>
    /// 풀에 넣을 때 비활성화 시키고, PoolManager에 넣고 물리힘 초기화 시키기
    /// </summary>
    /// <param name="obj"></param>
    private void ReturnToPool(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        // 어느 풀의 obj인지 확인
        if (!_itemIndexMap.TryGetValue(obj, out int poolIndex))
        {
            Debug.LogWarning($"풀 인덱스 정보가 없는 오브젝트입니다: {obj.name}");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(this.transform);

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        _pools[poolIndex].Enqueue(obj);
    }

    private void RemoveLife(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        if (_lifeMap.ContainsKey(obj))
        {
            _lifeMap.Remove(obj);
        }
    }

    private void UpdateAlive()
    {
        for (int i = _aliveObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = _aliveObjects[i];

            if (obj == null)
            {
                _aliveObjects.RemoveAt(i);
                continue;
            }

            if (!obj.activeSelf)
            {
                ReturnToPool(obj);
                _aliveObjects.RemoveAt(i);
                RemoveLife(obj);
                continue;
            }

            if (!_lifeMap.ContainsKey(obj))
            {
                //Debug.Log($"라이프 정보 없음 : {obj.name}");

                //ReturnToPool(obj);
                //_aliveObjects.RemoveAt(i);
                continue;
            }

            _lifeMap[obj] -= Time.deltaTime;

            if (_lifeMap[obj] <= 0.0f)
            {
                ReturnToPool(obj);
                _aliveObjects.RemoveAt(i);
                _lifeMap.Remove(obj);
            }
        }
    }

}
