using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameTime = 300f; // 5 phút
    public int targetFishCount = 10;
    public FishManager fishManager;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public Text timeRemainingText;
    public Text resultText;

    private float currentGameTime;
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateGameTime();
            CheckGameConditions();
        }
    }

    void UpdateGameTime()
    {
        currentGameTime -= Time.deltaTime;
        
        if (timeRemainingText != null)
            timeRemainingText.text = $"Thời gian: {Mathf.RoundToInt(currentGameTime)}s";

        if (currentGameTime <= 0)
        {
            EndGame(false);
        }
    }

    void CheckGameConditions()
    {
        FishingSystem fishingSystem = FindObjectOfType<FishingSystem>();
        if (fishingSystem != null && fishingSystem.caughtFishes.Count >= targetFishCount)
        {
            EndGame(true);
        }
    }

    void EndGame(bool isVictory)
    {
        isGameOver = true;
        Time.timeScale = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            resultText.text = isVictory 
                ? "Chiến thắng! Bạn đã bắt đủ số cá!" 
                : "Hết giờ! Trò chơi kết thúc.";
        }
    }

    public void ResetGame()
    {
        currentGameTime = gameTime;
        isGameOver = false;
        Time.timeScale = 1;

        // Sinh ra số lượng cá ban đầu
        fishManager.SpawnFish(15);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void RestartGame()
    {
        // Reload scene hoặc reset game
        ResetGame();
    }
}
