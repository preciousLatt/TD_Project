using TMPro;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rebindPromptText;

    public void OnResumePressed()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnPausePressed()
    {
        GameManager.Instance.PauseGame();
    }

    public void OnStartRebind(string action)
    {
        var handler = FindObjectOfType<InputHandler>();
        if (handler != null)
        {
            rebindPromptText.text = $"Press any key for {action}...";
            handler.StartRebind(action, () =>
            {
                rebindPromptText.text = "";
            });
        }
    }
}
