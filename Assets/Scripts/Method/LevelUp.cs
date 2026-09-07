using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    [Header("UI 패널")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private LevelUpUI[] _cardUIs = new LevelUpUI[3];

    [Header("데이터")]
    [SerializeField] private List<LevelUpDataSO> _weaponCards = new List<LevelUpDataSO>();
    [SerializeField] private List<LevelUpDataSO> _towerCards = new List<LevelUpDataSO>();
    [SerializeField] private List<LevelUpDataSO> _statBuffCards = new List<LevelUpDataSO>();

    private bool _isWaveClearMode = false;
    private int _targetTowerSlot = 0;

    private void Awake()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    /// <summary>
    /// BaseCore.cs 레벨업 시 호출
    /// </summary>
    public void OpenLevelUp()
    {
        _isWaveClearMode = false;
        GameManager.Instance.GamePause();
        _panel.SetActive(true);

        BaseCore core = GameManager.Instance.Core;

        List<LevelUpDataSO> validWeapons = new List<LevelUpDataSO>();
        foreach (var card in _weaponCards)
        {
            if (card.WeaponData == null)
            {
                continue;
            }

            Weapon equipped = core.GetEquippedWeapon(card.WeaponData.Type);

            // 만렙확인
            if (equipped != null && equipped.Level >= card.WeaponData.MaxLevel)
            {
                continue;
            }
            // 무기 슬롯 확인
            if (equipped == null && core.EquipWeapons.Count >= 4)
            {
                continue;
            }

            validWeapons.Add(card);
        }

        List<LevelUpDataSO> selectedCards = new List<LevelUpDataSO>();
        int pickCount = Mathf.Min(3, validWeapons.Count);   // 유효한 무기 중 최대 3개 랜덤

        for (int i = 0;  i < pickCount; i++)
        {
            int randIdx = Random.Range(0, validWeapons.Count);
            selectedCards.Add(validWeapons[randIdx]);
            validWeapons.RemoveAt(randIdx);
        }

        // 더이상 업그레이드 할 무기가 없어 무기카드가 3장이 안될 경우 스탯 강화 카드로 보충
        List<LevelUpDataSO> tempBuffs = new List<LevelUpDataSO>(_statBuffCards);
        while (selectedCards.Count < 3 && tempBuffs.Count > 0)
        {
            int randIdx = Random.Range(0, tempBuffs.Count);
            selectedCards.Add(tempBuffs[randIdx]);
            tempBuffs.RemoveAt(randIdx);
        }

        BindCards(selectedCards, core);

    }

    /// <summary>
    /// WaveManager.cs에서 웨이브 클리어시 호출
    /// </summary>
    /// <param name="unlockSlotIndex"></param>
    public void OpenWaveClearReward(int unlockSlotIndex)
    {
        _isWaveClearMode = true;
        _targetTowerSlot = unlockSlotIndex;

        GameManager.Instance.GamePause();
        _panel.SetActive(true);

        BaseCore core = GameManager.Instance.Core;

        List<LevelUpDataSO> availableTowers = new List<LevelUpDataSO>();
        foreach (var card in _towerCards)
        {
            if (card.TowerData != null && !core.HasTower(card.TowerData))
            {
                availableTowers.Add(card);
            }
        }

        List<LevelUpDataSO> selectedCards = new List<LevelUpDataSO>();

        int pickCount = Mathf.Min(3, availableTowers.Count);
        for (int i = 0; i < pickCount; i++)
        {
            int randIdx = Random.Range(0, availableTowers.Count);
            selectedCards.Add(availableTowers[randIdx]);
            availableTowers.RemoveAt(randIdx);
        }

        BindCards(selectedCards, core);
    }

    private void BindCards(List<LevelUpDataSO> cards, BaseCore core)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (i < cards.Count)
            {
                _cardUIs[i].gameObject.SetActive(true);
                _cardUIs[i].SetUp(cards[i], this, core);
            }
            else
            {
                _cardUIs[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnCardSelected(LevelUpDataSO data)
    {
        BaseCore core = GameManager.Instance.Core;

        switch (data.Type)
        {
            case LevelUpDataSO.RewardType.WeaponUpgrade:
                core.AddOrUpgradeWeapon(data.WeaponData);
                break;
            case LevelUpDataSO.RewardType.TowerUpgrade:
                core.BuildTower(_targetTowerSlot, data.TowerData);
                break;
            case LevelUpDataSO.RewardType.StatBuff:
                ApplyStatBuff(data, core);
                break;
        }

        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        GameManager.Instance.GameResume();

        if (_isWaveClearMode)
        {
            _isWaveClearMode = false;
            if (GameManager.Instance.Wave != null)
            {
                GameManager.Instance.Wave.ProceedToNextWave();
            }
        }
    }

    private void ApplyStatBuff(LevelUpDataSO data, BaseCore core)
    {
        if (core != null && data != null)
        {
            core.ApplyStatBuff(data.BuffType, data.BuffValue);
        }
    }
}
