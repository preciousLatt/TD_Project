using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    private readonly Stack<ICommand> undoStack = new();
    private readonly Stack<ICommand> redoStack = new();

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

    public void ExecuteCommand(ICommand command)
    {
        if (command.Execute())
        {
            undoStack.Push(command);
            redoStack.Clear();
        }
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;
        ICommand cmd = undoStack.Pop();
        cmd.Undo();
        redoStack.Push(cmd);
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;
        ICommand cmd = redoStack.Pop();
        cmd.Execute();
        undoStack.Push(cmd);
    }
}

public interface ICommand
{
    bool Execute();
    void Undo();
}
