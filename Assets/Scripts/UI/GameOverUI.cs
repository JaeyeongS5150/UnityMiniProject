using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("결과 텍스트 연결")]
    [SerializeField] private TextMeshProUGUI _survivedTimeText;
    [SerializeField] private TextMeshProUGUI _reachedWaveText;
    [SerializeField] private TextMeshProUGUI _totalKillsText;
    [SerializeField] private TextMeshProUGUI _totalGoldText;

    [Header("버튼")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    private void Awake()
    {
        if (_restartButton != null)
            _restartButton.onClick.AddListener(OnClickRestart);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(OnClickMainMenu);
    }

    /// <summary>
    /// 게임오버 시 결과 데이터 바인딩
    /// </summary>
    public void DisplayResult(float survivedTime, int wave, int kills)
    {
        int minutes = Mathf.FloorToInt(survivedTime / 60F);
        int seconds = Mathf.FloorToInt(survivedTime % 60F);

        if (_survivedTimeText != null)
            _survivedTimeText.text = $"생존 시간: {minutes:00}:{seconds:00}";

        if (_reachedWaveText != null)
            _reachedWaveText.text = $"도달 웨이브: Wave {wave}";

        if (_totalKillsText != null)
            _totalKillsText.text = $"총 처치 수: {kills} Kills";

        if (_totalGoldText != null)
            _totalGoldText.text = $"보유 골드: {MetaShopData.PlayerGold} G";
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}