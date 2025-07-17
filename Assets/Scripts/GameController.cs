using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Singleton instance
    public static GameController Instance { get; private set; }

    // Trạng thái game
    public enum GameState { Menu, Playing, Paused, GameOver, NextLevel }
    private GameState currentState = GameState.Menu;

    // Dữ liệu game
    private float totalTime = 300f;
    private float currentTime;
    private int score = 0;
    private int currentIndex;
    private bool anyCubeClicked = false;

    [Header("Components")]
    private List<CubeController> cubeControllers = new List<CubeController>();
    private CanvasController canvasController;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 

        // Khởi tạo
        canvasController = FindAnyObjectByType<CanvasController>();
        currentTime = totalTime;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh rò rỉ bộ nhớ
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void Start()
    {
        canvasController = FindObjectOfType<CanvasController>();
        if (canvasController != null)
            UpdateUI();

        InitializeGameState();
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameLogic();
        }       
        else if (currentState == GameState.NextLevel)
        {
            StartCoroutine(LoadNextLevel()); 
            SetState(GameState.Playing); 
        }
    }

    private void UpdateGameLogic()
    {        
        if (!anyCubeClicked)
        {
            foreach (CubeController cube in cubeControllers)
            {
                if (cube.IsClickCube())
                {
                    anyCubeClicked = true;
                    score += 10; // Tăng điểm khi nhấp cube
                    UpdateUI();
                    break;
                }
            }
        }
        else {
            if (currentTime > 0f)
            {
                if (GameObject.FindGameObjectsWithTag("Cube").Length == 0)
                {
                    canvasController.ShowNextLevelPanel(true);
                    currentTime -= 0;
                }
                else
                    currentTime -= Time.deltaTime;
                UpdateUI();
            }
            else
            {
                currentTime = 0f;
                SetState(GameState.GameOver);
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        if (canvasController != null)
        {
            canvasController.UpdateTimeDisplay(currentTime, totalTime);
            canvasController.UpdateLevelText($"{currentIndex}");
        }
    }

    public void SetState(GameState state)
    {
        currentState = state;
        switch (state)
        {
            case GameState.Paused:
                Time.timeScale = 0;
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                break;
            case GameState.GameOver:
                Time.timeScale = 0;
                break;
            case GameState.NextLevel:
                Time.timeScale = 0;
                break;
        }
        Debug.Log($"Game state changed to: {state}");
    }

    private IEnumerator LoadNextLevel()
    {
        if (canvasController != null)
        {
            canvasController.SpawnCoins();
            while (!canvasController.AreAllCoinsDestroyed())
            {
                yield return null;
            }
            canvasController.ShowNextLevelPanel(false);
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (int.TryParse(sceneName.Replace("Level", ""), out int currentLevel))
        {
            currentIndex = currentLevel; // Gán currentIndex từ tên scene
            int nextIndex = currentIndex + 1;
            if (nextIndex <= SceneManager.sceneCountInBuildSettings) // Sử dụng <= để bao gồm index 0
            {
                SceneManager.LoadScene($"Level{nextIndex}");
                yield return null;
                InitializeGameState();
            }
            else
            {
                SetState(GameState.GameOver); // Kết thúc game nếu hết scene
            }
        }
        else
        {
            Debug.LogError($"Scene name '{sceneName}' does not match expected format (e.g., 'Level1'). Defaulting to GameOver.");
            SetState(GameState.GameOver);
        }
    }
    private void InitializeGameState()
    {
        anyCubeClicked = false; 
        currentTime = totalTime; 
        cubeControllers = new List<CubeController>(FindObjectsByType<CubeController>(FindObjectsSortMode.None));
        string sceneName = SceneManager.GetActiveScene().name;
        if (int.TryParse(sceneName.Replace("Level", ""), out int newIndex))
        {
            currentIndex = newIndex; // Gán giá trị từ tên scene
        }
        else
        {
            Debug.LogWarning($"Failed to parse scene name '{sceneName}' to index. Setting currentIndex to 0.");
            currentIndex = 0; // Giá trị mặc định
        }
        SetState(GameState.Playing);         
        UpdateUI(); 
    }
    public void NextLevel()
    {
        SetState(GameState.NextLevel);
    }
    private IEnumerator ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        yield return null;
        InitializeGameState();
    }
    public void ResetLevelButton()
    {
        StartCoroutine(ResetLevel());
    }
    // Public methods for UI interaction
    public void OnPauseButton()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
            canvasController.ShowSettingsPanel(true);
        }
    }

    public void OnResumeButton()
    {
        if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
            canvasController.ShowSettingsPanel(false);
        }
    }

    // Getters for UI or other scripts
    public int GetScore() => score;
    public int GetLevel() => currentIndex;
    public float GetCurrentTime() => currentTime;
    public GameState GetState() => currentState;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded. Initializing game state...");
        InitializeGameState(); // Chạy mỗi khi load scene
        canvasController = FindObjectOfType<CanvasController>(); // Cập nhật lại tham chiếu Canvas
    }
}