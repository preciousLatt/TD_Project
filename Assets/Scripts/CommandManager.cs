using System.Collections.Generic;
using UnityEngine;

public interface IGameCommand
{
    bool Execute();
    void Undo();
}

public class CommandManager : MonoBehaviour
{
    private readonly Stack<IGameCommand> undoStack = new();
    private readonly Stack<IGameCommand> redoStack = new();

    public static CommandManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ExecuteCommand(IGameCommand command)
    {
        if (command.Execute())
        {
            undoStack.Push(command);
            redoStack.Clear();
        }
    }

    public void Undo()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState is not BuildState)
        {
            Debug.Log("Cannot Undo outside of Build Phase.");
            return;
        }

        if (undoStack.Count == 0) return;

        IGameCommand cmd = undoStack.Pop();
        cmd.Undo();
        redoStack.Push(cmd);
    }

    public void Redo()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState is not BuildState)
        {
            Debug.Log("Cannot Redo outside of Build Phase.");
            return;
        }

        if (redoStack.Count == 0) return;

        IGameCommand cmd = redoStack.Pop();

        if (cmd.Execute())
        {
            undoStack.Push(cmd);
        }
    }

    public void ClearStacks()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
}