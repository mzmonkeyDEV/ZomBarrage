using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Level-Up Panel Buttons")]
    [Tooltip("Drag the two existing choice buttons here — slot 0 is the one wired to ChooseHealth, slot 1 is the one wired to ChooseMight.")]
    public Button[] upgradeButtons = new Button[2];

    [Header("Max-Level Fallback")]
    [Range(0f, 1f)] public float healPercentWhenAllMaxed = 0.1f;

    private readonly Stack<string> upgradeHistory = new Stack<string>();
    private readonly string[] slotAssignments = new string[2];

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

    // Inspector button bindings: slot 0 => ChooseHealth, slot 1 => ChooseMight.
    // Method names kept for scene compatibility; actual upgrade applied is whatever
    // was rolled into that slot for this level-up.
    public void ChooseHealth() => ResolveSlot(0);
    public void ChooseMight() => ResolveSlot(1);

    private void ResolveSlot(int index)
    {
        if (index < 0 || index >= slotAssignments.Length)
        {
            ResumeGame();
            return;
        }
        string id = slotAssignments[index];
        if (!string.IsNullOrEmpty(id)) ApplyUpgradeById(id);
        UpdateUI();
        ResumeGame();
    }

    private void ApplyUpgradeById(string id)
    {
        if (player == null) return;
        PlayerUpgrades upgrades = player.GetComponent<PlayerUpgrades>();
        if (upgrades == null) return;
        if (upgrades.ApplyUpgrade(id))
        {
            UpgradeDefinition def = UpgradeCatalog.Get(id);
            upgradeHistory.Push(def != null ? def.DisplayName : id);
        }
    }

    void PauseForMenu()
    {
        PlayerUpgrades upgrades = player != null ? player.GetComponent<PlayerUpgrades>() : null;

        if (upgrades == null)
        {
            // No upgrade system wired — fall back to just showing the panel.
            Time.timeScale = 0f;
            if (levelUpPanel != null) levelUpPanel.SetActive(true);
            Cursor.visible = true;
            return;
        }

        int slots = upgradeButtons != null ? upgradeButtons.Length : 0;
        List<UpgradeDefinition> picks = upgrades.PickRandom(slots);

        if (picks.Count == 0)
        {
            ApplyMaxLevelHeal();
            return;
        }

        AssignSlots(picks, upgrades);
        Time.timeScale = 0f;
        if (levelUpPanel != null) levelUpPanel.SetActive(true);
        Cursor.visible = true;
    }

    private void AssignSlots(List<UpgradeDefinition> picks, PlayerUpgrades upgrades)
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            Button btn = upgradeButtons[i];
            if (btn == null) continue;

            if (i < picks.Count)
            {
                UpgradeDefinition def = picks[i];
                slotAssignments[i] = def.Id;
                btn.gameObject.SetActive(true);
                btn.interactable = true;
                TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = def.DescribeNext(upgrades.GetLevel(def.Id));
                }
            }
            else
            {
                slotAssignments[i] = null;
                btn.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyMaxLevelHeal()
    {
        if (player == null) return;
        float heal = player.maxHp * healPercentWhenAllMaxed;
        player.currentHp = Mathf.Min(player.maxHp, player.currentHp + heal);
        ShowText($"+{Mathf.RoundToInt(heal)} HP (Maxed)", Color.green);
        UpdateUI();
        // Skip the panel entirely so the run keeps flowing.
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

    void ResumeGame()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
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
