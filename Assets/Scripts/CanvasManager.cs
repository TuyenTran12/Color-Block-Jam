using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject canvasPrefab; // Prefab Canvas chứa CanvasController

    void Awake()
    {
        // Spawn Canvas nếu chưa có trong scene
        if (canvasPrefab != null && FindObjectOfType<CanvasController>() == null)
        {
            GameObject canvasObj = Instantiate(canvasPrefab);
            canvasObj.name = "GameController";
            DontDestroyOnLoad(canvasObj); // Giữ Canvas qua các scene (tùy chọn)
        }
    }
}