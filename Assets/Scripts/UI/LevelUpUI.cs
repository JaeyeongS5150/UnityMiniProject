using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _levelBadgeText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _selectButton;

    private LevelUpDataSO _currentData;
    private LevelUp _manager;

    public void SetUp(LevelUpDataSO data, LevelUp manager, BaseCore core)
    {
        _currentData = data;
        _manager = manager;

        _iconImage.sprite = data.Icon;
        _titleText.text = data.Name;

        switch (data.Type)
        {
            case LevelUpDataSO.RewardType.WeaponUpgrade:
                Weapon equiped = core.GetEquippedWeapon(data.WeaponData.Type);

                int currentLv = equiped != null ? equiped.Level : 0;
                if (currentLv == 0)
                {
                    _levelBadgeText.text = "NEW!";
                    _descriptionText.text = data.WeaponData.GetLevelData(1).levelDescription;
                }
                else
                {
                    _levelBadgeText.text = $"Lv.{currentLv} → Lv.{currentLv + 1}";
                    _descriptionText.text = data.WeaponData.GetLevelData(currentLv + 1).levelDescription;
                }
                break;

                // tower도 레벨업 시스템 구현시 수정 필요
            case LevelUpDataSO.RewardType.TowerUpgrade:
                _levelBadgeText.text = "New 포탑 배치";
                _descriptionText.text = data.TowerData.TowerDescription;
                break;
            case LevelUpDataSO.RewardType.StatBuff:
                _levelBadgeText.text = "스탯 강화";
                _descriptionText.text = data.Description;
                break;
        }
    }

    public void OnClickCard()
    {
        if (_manager != null && _currentData != null)
        {
            _manager.OnCardSelected(_currentData);
        }
    }

}
