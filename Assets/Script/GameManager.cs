using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int currentXP = 0;
    public int xpToLevelUp = 100;
    public int currentLevel = 1;

    [Header("XP Scaling")]
    [SerializeField] private float xpScaleFactor = 1.5f;

    private int capturedBaseXP = 100;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI mightText;

    [Header("Optional End-Game Score UI")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameWinScoreText;

    [Header("References")]
    public PlayerAttack player;
    public GameObject levelUpPanel;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;
    public GameObject textPrefab;

    private readonly Stack<string> upgradeHistory = new Stack<string>();

    public IReadOnlyCollection<string> UpgradeHistory => upgradeHistory;

    void Awake()
    {
        if (Instance == null) Instance = this;
        capturedBaseXP = xpToLevelUp;
    }

    void Start()
    {
        UpdateUI();
    }

    private static int ComputeXPThreshold(int level, int baseXP, float factor)
    {
        if (level <= 1) return baseXP;
        return Mathf.RoundToInt(ComputeXPThreshold(level - 1, baseXP, factor) * factor);
    }

    public void UpdateUI()
    {
        if (player != null && hpText != null)
        {
            hpText.text = $"HP: {Mathf.RoundToInt(player.currentHp)} / {Mathf.RoundToInt(player.maxHp)}";
        }

        if (xpText != null)
        {
            xpText.text = $"LVL {currentLevel} - {currentXP} / {xpToLevelUp} XP";
        }

        if (mightText != null && player != null)
        {
            mightText.text = "Damage: " + player.mightMultiplier.ToString("F1") + "x";
        }
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        ShowText($"+{amount} XP", Color.yellow);

        if (currentXP >= xpToLevelUp)
        {
            LevelUp();
        }

        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= xpToLevelUp;
        currentLevel++;
        xpToLevelUp = ComputeXPThreshold(currentLevel, capturedBaseXP, xpScaleFactor);

        ShowText("LEVEL UP!", Color.cyan);
        Invoke("PauseForMenu", 0.5f);
        UpdateUI();
    }

    public void ChooseHealth() => ApplyUpgrade("health");
    public void ChooseMight() => ApplyUpgrade("might");

    private void ApplyUpgrade(string id)
    {
        UpgradeOption option = UpgradeRegistry.Get(id);
        if (option != null && player != null)
        {
            option.Apply(player);
            upgradeHistory.Push(option.DisplayName);
        }
        UpdateUI();
        ResumeGame();
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0f;
        int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;
        HighScoreManager.RecordRun(currentLevel, wave);
        PopulateEndGameText(gameOverScoreText, wave);
        gameOverPanel.SetActive(true);
    }

    public void TriggerWin()
    {
        Time.timeScale = 0f;
        int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;
        HighScoreManager.RecordRun(currentLevel, wave);
        PopulateEndGameText(gameWinScoreText, wave);
        gameWinPanel.SetActive(true);
    }

    private void PopulateEndGameText(TextMeshProUGUI target, int wave)
    {
        if (target == null) return;
        string history = upgradeHistory.Count == 0 ? "None" : string.Join(", ", upgradeHistory);
        target.text =
            $"Level: {currentLevel}   Wave: {wave}\n" +
            $"Best Level: {HighScoreManager.HighestLevel}\n" +
            $"Best Wave: {HighScoreManager.HighestWave}\n" +
            $"Upgrades: {history}";
    }

    void PauseForMenu()
    {
        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void ShowText(string message, Color color)
    {
        if (textPrefab && player)
        {
            GameObject go = Instantiate(textPrefab, player.transform.position + Vector3.up * 2, Quaternion.identity);
            TextMeshProUGUI tm = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tm != null)
            {
                tm.text = message;
                tm.color = color;
            }
        }
    }
}
