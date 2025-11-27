using UnityEngine;

public interface IGameState
{
    void Enter(GameManager gm);
    void Update(GameManager gm);
    void Exit(GameManager gm);

    float GetManaCostMultiplier();
}

public class BuildState : IGameState
{
    public void Enter(GameManager gm)
    {
        Debug.Log("State: Build Phase. Abilities are FREE!");
        Time.timeScale = 1f;
    }

    public void Update(GameManager gm)
    {
        if (gm.GetNexusHealth() <= 0)
        {
            gm.ChangeState(new GameOverState());
        }
    }

    public void Exit(GameManager gm) { }

    public float GetManaCostMultiplier() => 0f; 
}

public class CombatState : IGameState
{
    public void Enter(GameManager gm)
    {
        Debug.Log("State: Combat Phase. Normal Rules Apply.");
        Time.timeScale = 1f;
    }

    public void Update(GameManager gm)
    {
        if (gm.GetNexusHealth() <= 0)
        {
            gm.ChangeState(new GameOverState());
        }
    }

    public void Exit(GameManager gm) { }

    public float GetManaCostMultiplier() => 1f; 
}

public class GameOverState : IGameState
{
    public void Enter(GameManager gm)
    {
        Debug.Log("State: Game Over.");
        Time.timeScale = 0f;
    }

    public void Update(GameManager gm) { }
    public void Exit(GameManager gm) { }

    public float GetManaCostMultiplier() => 1f;
}

public class VictoryState : IGameState
{
    public void Enter(GameManager gm)
    {
        Debug.Log("State: Victory!");
        Time.timeScale = 0f;
    }

    public void Update(GameManager gm) { }
    public void Exit(GameManager gm) { }

    public float GetManaCostMultiplier() => 1f;
}