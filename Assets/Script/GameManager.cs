using UnityEngine;
using TMPro; // Don't forget this!

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int currentXP = 0;
    public int xpToLevelUp = 100;
    public int currentLevel = 1;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI mightText;

    [Header("References")]
    public PlayerAttack player;
    public GameObject levelUpPanel;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;
    public GameObject textPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateUI(); // Set initial text
    }

    // Call this whenever HP or XP changes
    public void UpdateUI()
    {
        // Format: HP: 10 / 10
        if (player != null)
        {
            hpText.text = $"HP: {Mathf.RoundToInt(player.currentHp)} / {Mathf.RoundToInt(player.maxHp)}";
        }

        // Format: LVL 1 - 10 / 100 XP
        xpText.text = $"LVL {currentLevel} - {currentXP} / {xpToLevelUp} XP";

        if (mightText != null)
        {
            // Displays as "Might: 1.2x"
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

        UpdateUI(); // Refresh UI after getting XP
    }

    void LevelUp()
    {
        currentXP -= xpToLevelUp;
        xpToLevelUp = Mathf.RoundToInt(xpToLevelUp * 1.5f); // Scaling gets harder
        currentLevel++;

        ShowText("LEVEL UP!", Color.cyan);
        Invoke("PauseForMenu", 0.5f);
        UpdateUI();
    }

    // --- Button Functions Updated to Refresh UI ---

    public void ChooseHealth()
    {
        player.maxHp += 20;
        player.currentHp += 20; // Heal on upgrade
        UpdateUI();
        ResumeGame();
    }

    public void ChooseMight()
    {
        player.mightMultiplier += 0.1f;
        UpdateUI();
        ResumeGame();
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0f; // Freeze the action
        gameOverPanel.SetActive(true);
      
    }

    public void TriggerWin()
    {
        Time.timeScale = 0f;
        gameWinPanel.SetActive(true);
        
    }
    // Existing Pause/Resume/ShowText logic goes here...
    void PauseForMenu() { Time.timeScale = 0f; levelUpPanel.SetActive(true); Cursor.visible = true; }
    void ResumeGame() { levelUpPanel.SetActive(false); Time.timeScale = 1f; }
    void ShowText(string message, Color color) {
        if (textPrefab && player)
        {
            
            GameObject go = Instantiate(textPrefab, player.transform.position + Vector3.up * 2, Quaternion.identity);
            var tm = go.GetComponentInChildren<TextMeshProUGUI>();
            tm.text = message;
            tm.color = color;
        }
    }
}