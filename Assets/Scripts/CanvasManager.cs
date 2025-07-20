using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject gameController;
    private void Awake()
    {
        if (gameController != null && GameObject.Find("GameController") == null)
        {
            GameObject canvasObj = Instantiate(gameController);
            canvasObj.name = "GameController";
            DontDestroyOnLoad(canvasObj); // Giữ Canvas qua các scene (tùy chọn)            
        }
    }
}
