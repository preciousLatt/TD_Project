using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton.Singleton<UIManager>
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private HeroAbilitySlot[] abilitySlots;


    private Tower currentTower;
    public Tower CurrentTower => currentTower;
    public HeroAbilitySlot[] AbilitySlots => abilitySlots;


    public void SetupHeroUI(HeroCombat hero)
    {
        if (abilitySlots != null)
        {
            for (int i = 0; i < abilitySlots.Length; i++)
                abilitySlots[i].Bind(hero, hero.GetAbility(i));
        }
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
}
