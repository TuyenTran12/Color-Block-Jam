using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private GameObject coinUIPrefab; // Prefab UI coin (Image)
    [SerializeField] private RectTransform spawnPoint;  // Điểm xuất phát
    [SerializeField] private RectTransform endPoint;    // Điểm đến
    [SerializeField] private int coinCount = 40;        // Số lượng coin
    [SerializeField] private float moveSpeed = 2f;      // Tốc độ di chuyển
    [SerializeField] private float spreadAngle = 45f;   // Góc lan tỏa

    private int activeCoins = 0; // Đếm số coin đang tồn tại

    [Header("UI Elements")]
    public Text textLevel;
    public Text timeText;
    public GameObject NextLevelPanel;
    public GameObject SettingsPanel;

    void Awake()
    {
        // Đảm bảo Canvas ở Screen Space - Overlay
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    public void UpdateTimeDisplay(float currentTime, float totalTime)
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateLevelText(string level)
    {
        textLevel.text = level;
    }

    public void ShowNextLevelPanel(bool show) => NextLevelPanel?.SetActive(show);

    public void ShowSettingsPanel(bool show) => SettingsPanel?.SetActive(show);


    public void SpawnCoins() => StartCoroutine(SpawnAndMoveCoins());

    private IEnumerator SpawnAndMoveCoins()
    {
        if (coinUIPrefab == null || spawnPoint == null || endPoint == null)
        {
            Debug.LogError("Missing UI prefab, spawn point, or end point!");
            yield break;
        }

        Vector2 spawnPosition = spawnPoint.anchoredPosition;
        Vector2 endPosition = endPoint.anchoredPosition;
        Debug.Log("Spawning coins from: " + spawnPosition + " to: " + endPosition);

        activeCoins = coinCount;

        for (int i = 0; i < coinCount; i++)
        {
            GameObject coin = Instantiate(coinUIPrefab, spawnPoint);
            coin.transform.SetParent(spawnPoint.parent);
            coin.GetComponent<RectTransform>().anchoredPosition = spawnPoint.anchoredPosition;
            coin.tag = "Coin";

            Image img = coin.GetComponent<Image>();
            if (img == null || img.sprite == null)
            {
                Debug.LogError("Coin UI prefab missing Image or Sprite!");
                Destroy(coin);
                continue;
            }

            float angleOffset = -spreadAngle / 2 + (spreadAngle * i / (coinCount - 1));
            Vector2 direction = (endPosition - spawnPosition).normalized;
            direction = RotateVector2(direction, angleOffset);

            StartCoroutine(MoveCoin(coin, direction, endPosition));
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator MoveCoin(GameObject coin, Vector2 direction, Vector2 endPosition)
    {
        if (coin == null) { Debug.LogError("Coin is null!"); yield break; }

        Image img = coin.GetComponent<Image>();
        if (img != null)
            Debug.Log("Coin spawned at: " + coin.GetComponent<RectTransform>().anchoredPosition);

        float elapsed = 0f;
        float duration = 1f;
        Vector2 startPosition = coin.GetComponent<RectTransform>().anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            coin.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            coin.GetComponent<RectTransform>().localScale = Vector3.one * Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        Destroy(coin);
        activeCoins--;
        Debug.Log("Coin destroyed, remaining: " + activeCoins);
    }

    private Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    public bool AreAllCoinsDestroyed() => activeCoins == 0;
}