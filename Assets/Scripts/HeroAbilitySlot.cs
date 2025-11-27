using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeroAbilitySlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private Image cooldownMask;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private HeroCombat hero;
    private HeroAbility ability;
    private int index;
    private bool isCoolingDown;

    // NEW: We must track the active coroutine to stop it later
    private Coroutine currentCooldownRoutine;

    public void Bind(HeroCombat hero, HeroAbility ability)
    {
        this.hero = hero;
        this.ability = ability;

        if (ability != null)
            if (manaCostText != null)
            {
                manaCostText.text = $"{ability.manaCost}";
            }

        button.onClick.RemoveAllListeners();
        if (ability != null)
            button.onClick.AddListener(() => hero.UseAbility(hero.GetAbilityIndex(ability)));

        // This will now properly kill any running animation from the previous wave
        ResetCooldownUI();
    }

    private void ResetCooldownUI()
    {
        // 1. STOP the timer if it's running!
        if (currentCooldownRoutine != null)
        {
            StopCoroutine(currentCooldownRoutine);
            currentCooldownRoutine = null;
        }

        // 2. Reset visuals
        if (cooldownMask != null) cooldownMask.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = "";
        isCoolingDown = false;
    }

    public void StartCooldown(float duration)
    {
        if (!gameObject.activeInHierarchy) return;

        // Stop any existing routine before starting a new one (safety)
        ResetCooldownUI();

        // Start new routine and store the reference
        currentCooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        isCoolingDown = true;
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            float fill = timeLeft / duration;

            if (cooldownMask != null)
                cooldownMask.fillAmount = fill;

            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(timeLeft).ToString();

            yield return null;
        }

        // Routine finished naturally
        ResetCooldownUI();
    }
}