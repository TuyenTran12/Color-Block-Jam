using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeCanvasController : MonoBehaviour
{
    [Header("UI Elements")]
    public Text coinText;

    public Text txtLevel1;
    public Text txtLevel2;
    public Text txtLevel3;
    public Text txtLevel4;
    public void UpdateCoins(string coins)
    {
        coinText.text = coins;
    }
    public void UpdateNameLevelButton(int currentLevel)
    {
        txtLevel1.text = currentLevel.ToString();
        txtLevel2.text = (currentLevel + 1).ToString();
        txtLevel3.text = (currentLevel + 2).ToString();
        txtLevel4.text = (currentLevel + 3).ToString();
    }
}
