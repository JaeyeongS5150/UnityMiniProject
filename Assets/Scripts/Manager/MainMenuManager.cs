using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("씬 전환 설정")]
    [Tooltip("전투가 진행되는 인게임 씬 이름")]
    [SerializeField] private string _gameSceneName = "GameScene";

    [Header("팝업 패널 연결")]
    [SerializeField] private GameObject _howToPlayPanel;
    [SerializeField] private GameObject _metaShopPanel;

    private void Awake()
    {
        CloseAllPanels();
    }

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMainMenuBgm();
        }
    }

    #region 버튼 이벤트 바인딩

    /// <summary>
    /// 1. 게임 시작 버튼
    /// </summary>
    public void OnClickGameStart()
    {
        // 씬 비동기 로드로 부드러운 전환
        SceneManager.LoadScene(_gameSceneName);
    }

    /// <summary>
    /// 2. 게임 설명 버튼
    /// </summary>
    public void OnClickHowToPlay()
    {
        if (_howToPlayPanel != null)
        {
            _howToPlayPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 3. 업그레이드 상점(메타 상점) 버튼
    /// </summary>
    public void OnClickMetaShop()
    {
        if (_metaShopPanel != null)
        {
            _metaShopPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 4. 게임 종료 버튼
    /// </summary>
    public void OnClickExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 팝업 창 내 닫기(X) 버튼 공용
    /// </summary>
    public void CloseAllPanels()
    {
        if (_howToPlayPanel != null) _howToPlayPanel.SetActive(false);
        if (_metaShopPanel != null) _metaShopPanel.SetActive(false);
    }

    #endregion
}
