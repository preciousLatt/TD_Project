using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton.Singleton<UIManager>
{
    [Header(" Panels ")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject pausePanel;

    [Header(" Upgrade UI ")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;

    [Header(" HUD ")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private HeroAbilitySlot[] abilitySlots;

    [Header(" Wave UI ")]
    [SerializeField] private TextMeshProUGUI waveText;      // Assign in Inspector
    [SerializeField] private Button startWaveButton;        // Assign in Inspector

    private Tower currentTower;
    private RebindRowUI[] rebindRows;

    // --- RESTORED PUBLIC PROPERTIES ---
    public Tower CurrentTower => currentTower;
    public HeroAbilitySlot[] AbilitySlots => abilitySlots;
    // ----------------------------------

    private void Awake()
    {
        rebindRows = FindObjectsOfType<RebindRowUI>(true);

        // Hook up the start button automatically
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(() => {
                if (WaveManager.Instance != null)
                    WaveManager.Instance.StartNextWave();
            });
        }
    }

    // --- Wave Logic ---
    public void UpdateWaveUI(int current, int total)
    {
        if (waveText != null)
            waveText.text = $"Wave: {current} / {total}";
    }

    public void SetWaveButtonState(bool interactable)
    {
        if (startWaveButton != null)
        {
            startWaveButton.interactable = interactable;
            var txt = startWaveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = interactable ? "Start Wave" : "Spawning...";
        }
    }
    // ------------------

    public void SetupHeroUI(HeroCombat hero)
    {
        if (abilitySlots != null)
        {
            for (int i = 0; i < abilitySlots.Length; i++)
                abilitySlots[i].Bind(hero, hero.GetAbility(i));
        }
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null)
            pausePanel.SetActive(show);
    }

    public void UpdateHeroBars(HeroStats stats)
    {
        if (healthBar != null)
            healthBar.value = stats.currentHealth / stats.maxHealth;
        if (manaBar != null)
            manaBar.value = stats.currentMana / stats.maxMana;
    }

    public void SetActiveTower(Tower tower)
    {
        if (tower == currentTower) return;
        currentTower = tower;

        if (tower == null)
        {
            HideUpgradePanel();
            return;
        }

        ShowUpgradePanel(tower);
    }

    public void UpdateCurrencyUI(int newAmount)
    {
        if (currencyText != null)
            currencyText.text = $"${newAmount}";
    }

    public void ShowUpgradePanel(Tower tower)
    {
        if (tower == null) return;

        upgradePanel.SetActive(true);
        ClearButtons();

        for (int i = 0; i < tower.upgradePaths.Count; i++)
        {
            UpgradeStep step = tower.GetNextUpgrade(i);
            if (step == null) continue;

            var buttonObj = Instantiate(upgradeButtonPrefab, buttonContainer);
            var btn = buttonObj.GetComponent<UnityEngine.UI.Button>();
            var txt = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            txt.text = $"{tower.upgradePaths[i].pathName} ({step.cost})";
            int index = i;
            btn.onClick.AddListener(() => tower.ApplyUpgrade(index));
        }
    }

    public void HideUpgradePanel()
    {
        upgradePanel.SetActive(false);
        currentTower = null;
        ClearButtons();
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);
    }

    public void UpdateRemapUI()
    {
        if (rebindRows == null || rebindRows.Length == 0)
            rebindRows = FindObjectsOfType<RebindRowUI>(true);

        foreach (var row in rebindRows)
        {
            row.Refresh();
        }
    }
}