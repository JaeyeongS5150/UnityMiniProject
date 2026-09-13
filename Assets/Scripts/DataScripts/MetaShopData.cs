using UnityEngine;

public static class MetaShopData
{
    private const string KeyGold = "Meta_Gold";
    private const string KeyHpLevel = "Meta_HpLevel";
    private const string KeyAtkLevel = "Meta_AtkLevel";
    private const string KeyExpLevel = "Meta_ExpLevel";
    private const string KeyGoldLevel = "Meta_GoldLevel";

    public static int PlayerGold
    {
        get => PlayerPrefs.GetInt(KeyGold, 0);
        set => PlayerPrefs.SetInt(KeyGold, value);
    }

    public static int HpLevel { get => PlayerPrefs.GetInt(KeyHpLevel, 0); set => PlayerPrefs.SetInt(KeyHpLevel, value); }
    public static int AtkLevel { get => PlayerPrefs.GetInt(KeyAtkLevel, 0); set => PlayerPrefs.SetInt(KeyAtkLevel, value); }
    public static int ExpLevel { get => PlayerPrefs.GetInt(KeyExpLevel, 0); set => PlayerPrefs.SetInt(KeyExpLevel, value); }
    public static int GoldLevel { get => PlayerPrefs.GetInt(KeyGoldLevel, 0); set => PlayerPrefs.SetInt(KeyGoldLevel, value); }

    public const int MaxHpLevel = 10;
    public const int MaxAtkLevel = 10;
    public const int MaxExpLevel = 5;
    public const int MaxGoldLevel = 5;

    public static int GetHpUpgradeCost() => Mathf.RoundToInt(50f * Mathf.Pow(1.5f, HpLevel));
    public static int GetAtkUpgradeCost() => Mathf.RoundToInt(70f * Mathf.Pow(1.5f, AtkLevel));
    public static int GetExpUpgradeCost() => Mathf.RoundToInt(100f * Mathf.Pow(2.0f, ExpLevel));
    public static int GetGoldUpgradeCost() => Mathf.RoundToInt(150f * Mathf.Pow(2.0f, GoldLevel));

    public static float GetBonusMaxHp() => HpLevel * 15f;
    public static float GetBonusAtkMultiplier() => 1f + (AtkLevel * 0.1f);
    public static float GetBonusExpMultiplier() => 1f + (ExpLevel * 0.1f);
    public static float GetBonusGoldMultiplier() => 1f + (GoldLevel * 0.1f);
}