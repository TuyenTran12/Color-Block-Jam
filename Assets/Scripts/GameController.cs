using System.Collections;
using System.Collections.Generic;
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
    public static int currentIndex;
    private static int currentScene = 1; 
    private bool anyCubeClicked = false;

    // Tham chiếu đến Canvas con
    [SerializeField] private GameObject gameCanvas; // Canvas cho màn chơi
    [SerializeField] private GameObject homeCanvas; // Canvas cho trang chủ

    private List<CubeController> cubeControllers = new List<CubeController>();
    private GameCanvasController gameCanvasController;
    private HomeCanvasController homeCanvasController;

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
        gameCanvasController = FindObjectOfType<GameCanvasController>();
        homeCanvasController = GetComponentInChildren<HomeCanvasController>();
        currentTime = totalTime;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (gameCanvasController != null)
            UpdateUI();

        InitializeGameState();
    }

    void Update()
    {
        UpdateDataGame();
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
                    UpdateUI();
                    break;
                }
            }
        }
        else
        {
            if (currentTime > 0f)
            {
                if (GameObject.FindGameObjectsWithTag("Cube").Length == 0)
                {
                    gameCanvasController.ShowNextLevelPanel(true);
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

    private void UpdateDataGame()
    {
        gameCanvasController.UpdateCoins($"{GameData.totalCoin}");

        homeCanvasController.UpdateCoins($"{GameData.totalCoin}");
        homeCanvasController.UpdateNameLevelButton(currentScene);
    }
    private void UpdateUI()
    {
        if (gameCanvasController != null)
        {
            gameCanvasController.UpdateTimeDisplay(currentTime, totalTime);
            gameCanvasController.UpdateLevelText($"{currentIndex}");
            gameCanvasController.ShowSettingsPanel(false);
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
                UpdateDataGame();
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
        if (gameCanvasController != null)
        {
            gameCanvasController.SpawnCoins();
            while (!gameCanvasController.AreAllCoinsDestroyed())
            {
                yield return null;
            }
            gameCanvasController.ShowNextLevelPanel(false);
        }

        int nextIndex = currentIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings) 
        {
            SceneManager.LoadScene(nextIndex);
            GameData.AddCoin(40);
            yield return null;
            InitializeGameState();
        }
        else
        {
            SetState(GameState.GameOver);
        }
    }

    private void InitializeGameState()
    {
        anyCubeClicked = false;
        currentTime = totalTime;
        cubeControllers.Clear();
        cubeControllers = new List<CubeController>(FindObjectsByType<CubeController>(FindObjectsSortMode.None));
        SetState(GameState.Playing);
        UpdateUI();
    }

    public void NextLevel()
    {
        SetState(GameState.NextLevel);
    }

    private IEnumerator LoadScene(int currentIndex)
    {
        SceneManager.LoadScene(currentIndex);
        yield return null;
        InitializeGameState();
    }

    public void ResetLevelButton()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    public void CombackLevelButton()
    {
        StartCoroutine(LoadScene(currentScene));
    }

    // Public methods for UI interaction
    public void OpenSettingsButton()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
            gameCanvasController.ShowSettingsPanel(true);
        }
    }

    public void ExitSettingsButton()
    {
        if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
            gameCanvasController.ShowSettingsPanel(false);
        }
    }

    public void HomeButton()
    {
        currentScene = currentIndex;
        SceneManager.LoadScene(0);       
    }

    // Getters for UI or other scripts
    public int GetLevel() => currentIndex;
    public float GetCurrentTime() => currentTime;
    public GameState GetState() => currentState;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeGameState();
        string currentScene = scene.name;
        if (currentScene == "Home")
        {
            if (homeCanvas != null) homeCanvas.SetActive(true);
            if (gameCanvas != null) gameCanvas.SetActive(false);
        }
        else if (currentScene.StartsWith("Level"))
        {
            if (gameCanvas != null) gameCanvas.SetActive(true);
            if (homeCanvas != null) homeCanvas.SetActive(false);
        }
        UpdateUI();
        currentIndex = SceneManager.GetActiveScene().buildIndex;
    }
}