using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("패널 연결")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _howToPlayPanel;

    [Header("일시정지 버튼들")]
    [SerializeField] private Button _pauseButton;     // 화면 구석 일시정지 아이콘 버튼
    [SerializeField] private Button _resumeButton;    // 1. 계속하기
    [SerializeField] private Button _howToPlayButton; // 2. 게임 방법
    [SerializeField] private Button _exitGameButton;  // 3. 게임 종료

    [Header("게임 방법 닫기 버튼")]
    [SerializeField] private Button _howToPlayCloseButton;

    private void Awake()
    {
        if (_pauseButton != null) _pauseButton.onClick.AddListener(OpenPause);
        if (_resumeButton != null) _resumeButton.onClick.AddListener(ResumeGame);
        if (_howToPlayButton != null) _howToPlayButton.onClick.AddListener(OpenHowToPlay);
        if (_exitGameButton != null) _exitGameButton.onClick.AddListener(ExitToGameOver);

        if (_howToPlayCloseButton != null) _howToPlayCloseButton.onClick.AddListener(CloseHowToPlay);

        CloseAll();
    }

    private void Update()
    {
        // ESC 키 입력 대응
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_howToPlayPanel != null && _howToPlayPanel.activeSelf)
            {
                CloseHowToPlay();
            }
            else if (_pausePanel != null && _pausePanel.activeSelf)
            {
                ResumeGame();
            }
            else if (GameManager.Instance != null && GameManager.Instance.IsLive)
            {
                OpenPause();
            }
        }
    }

    public void OpenPause()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsLive) return;

        GameManager.Instance.GamePause();
        if (_pausePanel != null) _pausePanel.SetActive(true);
        if (_howToPlayPanel != null) _howToPlayPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        CloseAll();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameResume();
        }
    }

    public void OpenHowToPlay()
    {
        if (_howToPlayPanel != null) _howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        if (_howToPlayPanel != null) _howToPlayPanel.SetActive(false);
    }

    /// <summary>
    /// 게임 종료 클릭 시 즉시 런을 정산하고 GameOver 패널을 호출
    /// </summary>
    public void ExitToGameOver()
    {
        CloseAll();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private void CloseAll()
    {
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_howToPlayPanel != null) _howToPlayPanel.SetActive(false);
    }
}
