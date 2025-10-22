using UnityEngine;

public class CommandTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            CommandManager.Instance.Undo();

        if (Input.GetKeyDown(KeyCode.Y))
            CommandManager.Instance.Redo();
    }
}