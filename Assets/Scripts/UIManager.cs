using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton.Singleton<UIManager>
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Button sellButton; 

    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private HeroAbilitySlot[] abilitySlots;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button startWaveButton;

    [SerializeField] private GameObject goldPopupPrefab;

    private Tower currentTower;
    private RebindRowUI[] rebindRows;

    public Tower CurrentTower => currentTower;
    public HeroAbilitySlot[] AbilitySlots => abilitySlots;

    private void Awake()
    {
        rebindRows = FindObjectsOfType<RebindRowUI>(true);

        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(() => {
                if (WaveManager.Instance != null)
                    WaveManager.Instance.StartNextWave();
            });
        }
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowGoldPopup(Vector3 worldPos, int amount)
    {
        if (goldPopupPrefab == null || currencyText == null) return;

        Vector3 fixedOffset = Vector3.down * 60f;
        Vector3 spawnPos = currencyText.transform.position + fixedOffset;

        GameObject go = Instantiate(goldPopupPrefab, currencyText.transform.parent);

        go.transform.SetAsLastSibling();
        go.transform.position = spawnPos;

        Vector3 localPos = go.transform.localPosition;
        localPos.z = 0f;
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;

        go.transform.localScale = Vector3.one * 0.7f;

        var popup = go.GetComponent<CurrencyPopupUI>();
        if (popup != null)
        {
            popup.Initialize(amount, currencyText.rectTransform);
        }
    }

    public void UpdateCurrencyUI(int newAmount)
    {
        if (currencyText != null)
            currencyText.text = $"${newAmount}";
    }

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

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();

            int refundAmount = Mathf.FloorToInt(tower.cost * 0.5f);

            var sellText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
            if (sellText != null)
            {
                sellText.text = $"Sell (+{refundAmount})";
            }

            sellButton.onClick.AddListener(() => {
                BuildManager.Instance.SellTower(tower.gameObject);
            });
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