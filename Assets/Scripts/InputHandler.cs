using System;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Dictionary<string, KeyCode> keyBindings = new()
    {
        { "MoveUp", KeyCode.W },
        { "MoveDown", KeyCode.S },
        { "MoveLeft", KeyCode.A },
        { "MoveRight", KeyCode.D },
        { "Attack", KeyCode.Mouse1 },
        { "Ability1", KeyCode.Alpha1 },
        { "Ability2", KeyCode.Alpha2 },
        { "Ability3", KeyCode.Alpha3 },
        { "Ability4", KeyCode.Alpha4 }
    };

    private string actionToRebind = null;
    public static InputHandler Instance { get; private set; }
    private System.Action rebindCompleteCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (actionToRebind != null)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    keyBindings[actionToRebind] = key;
                    Debug.Log($"{actionToRebind} rebound to {key}");
                    actionToRebind = null;
                    rebindCompleteCallback?.Invoke();
                    rebindCompleteCallback = null;
                    UIManager.Instance.UpdateRemapUI();
                    return;
                }
            }
        }
    }

    public void StartRebind(string action, System.Action onComplete = null)
    {
        actionToRebind = action;
        rebindCompleteCallback = onComplete;
        Debug.Log($"Listening for new key for {action}...");
    }


    public void RemapKey(string action, KeyCode newKey)
    {
        keyBindings[action] = newKey;
        Debug.Log($"{action} mapped to {newKey}");
    }

    public KeyCode GetKey(string action)
    {
        return keyBindings.ContainsKey(action) ? keyBindings[action] : KeyCode.None;
    }
}
