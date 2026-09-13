using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaShopUI : MonoBehaviour
{
    [Header("보유 골드 UI")]
    [SerializeField] private TextMeshProUGUI _playerGoldText;

    [Header("기지 체력 강화 UI")]
    [SerializeField] private TextMeshProUGUI _hpInfoText;
    [SerializeField] private Button _hpBuyBtn;

    [Header("공격력 강화 UI")]
    [SerializeField] private TextMeshProUGUI _atkInfoText;
    [SerializeField] private Button _atkBuyBtn;

    [Header("경험치 획득 강화 UI")]
    [SerializeField] private TextMeshProUGUI _expInfoText;
    [SerializeField] private Button _expBuyBtn;

    [Header("골드 파밍 강화 UI")]
    [SerializeField] private TextMeshProUGUI _goldInfoText;
    [SerializeField] private Button _goldBuyBtn;

    private void OnEnable()
    {
        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        _playerGoldText.text = $"보유 골드: {MetaShopData.PlayerGold} G";

        UpdateItemUI("체력 강화", MetaShopData.HpLevel, MetaShopData.MaxHpLevel, MetaShopData.GetHpUpgradeCost(), _hpInfoText, _hpBuyBtn);
        UpdateItemUI("공격력 강화", MetaShopData.AtkLevel, MetaShopData.MaxAtkLevel, MetaShopData.GetAtkUpgradeCost(), _atkInfoText, _atkBuyBtn);
        UpdateItemUI("경험치 획득", MetaShopData.ExpLevel, MetaShopData.MaxExpLevel, MetaShopData.GetExpUpgradeCost(), _expInfoText, _expBuyBtn);
        UpdateItemUI("추가 골드", MetaShopData.GoldLevel, MetaShopData.MaxGoldLevel, MetaShopData.GetGoldUpgradeCost(), _goldInfoText, _goldBuyBtn);
    }

    private void UpdateItemUI(string itemName, int currentLevel, int maxLevel, int cost, TextMeshProUGUI infoText, Button buyBtn)
    {
        buyBtn.interactable = true;
        TextMeshProUGUI btnText = buyBtn.GetComponentInChildren<TextMeshProUGUI>();

        if (currentLevel >= maxLevel)
        {
            infoText.text = $"{itemName}\n<color=yellow>Lv.MAX</color>";
            buyBtn.GetComponentInChildren<TextMeshProUGUI>().text = "강화 완료";
        }
        else
        {
            infoText.text = $"{itemName}\nLv.{currentLevel} -> Lv.{currentLevel + 1}";
            bool canAfford = MetaShopData.PlayerGold >= cost;
            buyBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"{cost} G";
        }
    }

    public void BuyHpUpgrade()
    {
        TryUpgrade(
            currentLevel: MetaShopData.HpLevel,
            maxLevel: MetaShopData.MaxHpLevel,
            cost: MetaShopData.GetHpUpgradeCost(),
            onSuccess: () => MetaShopData.HpLevel++
        );
    }

    public void BuyAtkUpgrade()
    {
        TryUpgrade(
            currentLevel: MetaShopData.AtkLevel,
            maxLevel: MetaShopData.MaxAtkLevel,
            cost: MetaShopData.GetAtkUpgradeCost(),
            onSuccess: () => MetaShopData.AtkLevel++
        );
    }

    public void BuyExpUpgrade()
    {
        TryUpgrade(
            currentLevel: MetaShopData.ExpLevel,
            maxLevel: MetaShopData.MaxExpLevel,
            cost: MetaShopData.GetExpUpgradeCost(),
            onSuccess: () => MetaShopData.ExpLevel++
        );
    }

    public void BuyGoldUpgrade()
    {
        TryUpgrade(
            currentLevel: MetaShopData.GoldLevel,
            maxLevel: MetaShopData.MaxGoldLevel,
            cost: MetaShopData.GetGoldUpgradeCost(),
            onSuccess: () => MetaShopData.GoldLevel++
        );
    }

    /// <summary>
    /// 강화 구매 조건 검사 및 성공/실패 사운드 분기 공용 메서드
    /// </summary>
    private void TryUpgrade(int currentLevel, int maxLevel, int cost, System.Action onSuccess)
    {
        // 1. 이미 만렙이거나 골드가 부족한 경우 -> 실패음
        if (currentLevel >= maxLevel || MetaShopData.PlayerGold < cost)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(AudioManager.SFX.InvalidClick);
            }
            return;
        }

        // 2. 구매 성공 -> 골드 차감 및 성공음
        MetaShopData.PlayerGold -= cost;
        onSuccess?.Invoke();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.SFX.ValidClick);
        }

        UpdateAllUI();
    }
}