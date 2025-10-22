using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RebindRowUI : MonoBehaviour
{
    public string actionName;
    public TextMeshProUGUI keyText;
    public Button rebindButton;

    private PauseMenuUI pauseUI;

    void Start()
    {
        pauseUI = FindObjectOfType<PauseMenuUI>();
        rebindButton.onClick.AddListener(() => pauseUI.OnStartRebind(actionName));
        Refresh();
    }

    public void Refresh()
    {
        var input = FindObjectOfType<InputHandler>();
        if (input != null && keyText != null)
        {
            KeyCode key = input.GetKey(actionName);
            keyText.text = key.ToString();
        }
    }

}