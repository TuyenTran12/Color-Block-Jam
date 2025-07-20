using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    public static int totalCoin;
    
    public static void AddCoin(int coin)
    {
        totalCoin += coin;
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); 
    }
}
