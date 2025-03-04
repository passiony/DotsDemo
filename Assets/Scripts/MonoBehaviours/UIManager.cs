using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Image redProgress;
    public Image blueProgress;
    public TextMeshProUGUI redHP;
    public TextMeshProUGUI blueHP;

    public TextMeshProUGUI redMoney;
    public TextMeshProUGUI blueMoney;

    void Awake()
    {
        Instance = this;
    }

    public void SetHP(Faction faction, float value)
    {
        switch (faction)
        {
            case Faction.Red:
                redProgress.fillAmount = value;
                redHP.text = ((int)(value * 100)).ToString();
                break;
            case Faction.Blue:
                blueProgress.fillAmount = value;
                blueHP.text = ((int)(value * 100)).ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
        }
    }

    public void SetMoney(Faction faction, int value)
    {
        switch (faction)
        {
            case Faction.Red:
                redMoney.text = value.ToString();
                break;
            case Faction.Blue:
                blueMoney.text = value.ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
        }
    }
}